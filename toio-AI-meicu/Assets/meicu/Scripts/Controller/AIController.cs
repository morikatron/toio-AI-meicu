using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;


namespace toio.AI.meicu
{
    public class AIController : BaseController
    {
        internal static AIController ins { get; private set; }

        public GameAgent agent;
        public Game game;
        public int heatmapPredictSteps = 8;

        protected override int id => 1;

        internal bool isPredicting { get; private set; } = false;
        internal event Action heatmapCallback;
        internal event Action<int> thinkCallback;
        internal bool isPause = false;
        internal Config.StageSetting setting;
        internal ref readonly float[,] Heatmap => ref heatmap;

        bool isActReceived = false;
        Env.Action action;
        float[,] heatmap = new float[9, 9];
        float[,] heatmapBuffer = new float[9, 9];


        protected override void Awake()
        {
            base.Awake();
            ins = this;
            targetMatCoordBias = new Vector2Int(-10, 10);

            agent.actCallback = OnAgentAct;
            game.startCallback += OnGameStarted;
            game.stepCallbackA += OnGameStep;
            game.overCallbackA += OnGameOver;
            game.stopCallback += OnGameStop;
        }

        void Stop()
        {
            StopMotion();

            StopAllCoroutines();
            isPredicting = false;
            isActReceived = false;
            ClearHeatmap();
            ClearHeatmapBuffer();
        }

        internal void LoadModelByLevel(int lv)
        {
            agent.LoadModelByName(Config.levelSettings[lv-1].modelName);
        }
        internal void LoadBestModel()
        {
            agent.LoadModelByName(Config.bestModelName);
        }

        // Start ienumerator which trys TargetMove in loop until target reached or overwritten.
        protected void RequestMoveWithSetting(Env.Action action)
        {
            this.action = action;
            (var r, var c) = Env.Translate(action, game.envA.row, game.envA.col);
            RequestMoveWithSetting(r, c);
        }

        // Start ienumerator which trys TargetMove in loop until target reached or overwritten.
        protected void RequestMoveWithSetting(int row, int col)
        {
            if (ieMotion != null && !isPerforming && this.targetCoords.x == row && this.targetCoords.y == col)
                return;

            this.targetCoords = new Vector2Int(row, col);

            var spd = setting.speeds[game.envA.passedSpaceCnt];
            var confirmTime = setting.confirmTimes[game.envA.passedSpaceCnt];

            StopMotion();
            ieMotion = IE_MoveInGame(spd, confirmTime);
            StartCoroutine(ieMotion);
        }

        IEnumerator IE_MoveInGame(byte spd, float confirmTime, bool timeCorrection = false)
        {
            yield return IE_Move(spd, confirmTime, timeCorrection);
            // Apply Step to game
            game.MoveA(action);
        }

        // Control Loop in Game
        IEnumerator IE_Think()
        {
            while (true)
            {
                if (isPause)
                {
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }

                this.thinkCallback?.Invoke(0);

                float t = Time.realtimeSinceStartup;

                // Predicting Heatmap
                Debug.Log($"AICon.IE_Think : predict heatmap (t={t})");
                this.isPredicting = true;
                ClearHeatmapBuffer();

                yield return IE_PredictHeatmap(game.envA.Clone(), this.heatmapPredictSteps);
                Array.Copy(this.heatmapBuffer, this.heatmap, this.heatmap.Length);
                this.isPredicting = false;
                if (isPause) continue;
                this.heatmapCallback?.Invoke();
                this.thinkCallback?.Invoke(1);

                // Interval - simulate time of thinking
                var interval = setting.thinkTimes[game.envA.passedSpaceCnt];
                yield return new WaitForSecondsRealtime(interval - (Time.realtimeSinceStartup - t));
                if (isPause) continue;
                this.thinkCallback?.Invoke(2);

                // Request Agent Action
                Debug.Log($"AICon.IE_Think : RequestAct (t={Time.realtimeSinceStartup})");
                this.isActReceived = false;
                agent.RequestAct(game.envA);
                yield return new WaitUntil(()=>isActReceived);
                yield return new WaitForEndOfFrame();
                if (isPause) continue;

                Debug.Log("AICon.IE_Think : wait moving");
                yield return new WaitUntil(()=>!isMoving);

                Debug.Log("AICon.IE_Think : end");
                this.thinkCallback?.Invoke(3);
            }
        }


        // IEnumerator of getting heatmap (Used automatically by IE_Think)
        IEnumerator IE_PredictHeatmap(Env env, int maxSteps=6, float parentProb=1, int step=0)
        {
            // End condition
            if (maxSteps == step) yield break;
            // Igonre small prob branches
            if (parentProb < 0.01f) yield break;

            // Request agent act
            this.isActReceived = false;
            this.agent.RequestAct(env);
            yield return new WaitUntil(()=>isActReceived);

            // Get & Process features
            var features = agent.additionalOuput;
            if (features.Count == 0) yield break;

            var feature = features[agent.m_additionalOutputNames[0]];
            float[] ps = new float[4];
            float maxLogit = Mathf.Max(feature[0], feature[1], feature[2], feature[3]);
            var e0 = Mathf.Exp(feature[0] - maxLogit + 10);
            var e1 = Mathf.Exp(feature[1] - maxLogit + 10);
            var e2 = Mathf.Exp(feature[2] - maxLogit + 10);
            var e3 = Mathf.Exp(feature[3] - maxLogit + 10);
            var sum = e0+e1+e2+e3;
            ps[0] = e0 / sum;
            ps[1] = e1 / sum;
            ps[2] = e2 / sum;
            ps[3] = e3 / sum;
            // Debug.Log($"feature: {feature[0]}, {feature[1]}, {feature[2]}, {feature[3]}");
            // Debug.Log($"e: {e0}, {e1}, {e2}, {e3}");
            // Debug.Log($"p: {ps[0]}, {ps[1]}, {ps[2]}, {ps[3]}");

            // Recursive calls
            for (int actionCode = 0; actionCode < 4; actionCode++)
            {
                var p = ps[actionCode];
                if (p < 0.01f) continue;    // Ignore low probability action

                string info = "";
                for (int i = 0; i < step; i++) info += " -- ";

                var subEnv = env.Clone();
                var res = subEnv.Step((Env.Action)actionCode);

                heatmapBuffer[subEnv.row, subEnv.col] += parentProb * p;
                if (res == Env.Response.None || res == Env.Response.StepWhite || res == Env.Response.StepColor)
                {
                    yield return IE_PredictHeatmap(subEnv, maxSteps, parentProb * p, step+1);
                }
            }
        }

        // Get Heatmap manually. Will pause IE_Think if in game.
        internal IEnumerator PredictHeatmapOnce(Env env, int steps)
        {
            if (this.isPredicting)
                yield return new WaitUntil(() => !this.isPredicting);
            agent.Restart();
            this.isPause = true;
            this.isPredicting = true;
            ClearHeatmapBuffer();
            yield return IE_PredictHeatmap(env.Clone(), steps);
            Array.Copy(this.heatmapBuffer, this.heatmap, this.heatmap.Length);
            this.heatmapCallback?.Invoke();
            this.isPredicting = false;
            this.isPause = false;
        }


        protected override void OnCubeIDMissed(Cube cube)
        {
            Debug.LogWarning("AIController.OnCubeIDMissed.");
        }

        protected override void OnCubeTargetMove(Cube cube, int configId, Cube.TargetMoveRespondType type)
        {
            if (type != Cube.TargetMoveRespondType.Normal)
                Debug.LogWarning("AIController.OnCubeTargetMove : type = " + type.ToString());
        }


        #region ======== Game Event ========
        void OnGameStarted(int countDown)
        {
            if (countDown == 0)
            {
                agent.Restart();
                StartCoroutine(IE_Think());
            }
        }

        void OnGameStep(Env.Response res)
        {
        }

        void OnGameOver(Game.PlayerState state)
        {
            StopAllCoroutines();
            isPredicting = false;
        }

        void OnGameStop()
        {
            Stop();
        }
        #endregion


        void OnAgentAct(Env.Action action)
        {
            this.isActReceived = true;

            if (!isPredicting)
                RequestMoveWithSetting(action);
        }

        void ClearHeatmap()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                    this.heatmap[i, j] = 0;
            }
        }

        void ClearHeatmapBuffer()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                    this.heatmapBuffer[i, j] = 0;
            }
        }
    }

}
