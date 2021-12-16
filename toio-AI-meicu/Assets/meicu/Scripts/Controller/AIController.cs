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
        internal float intervalBegin = 3f;
        internal float intervalEnd = 1f;
        internal ref readonly float[,] Heatmap => ref heatmap;
        internal bool isMoving { get; private set; } = false;

        bool isActReceived = false;
        Env.Action action;
        Vector2Int targetCoords;
        float[,] heatmap = new float[9, 9];
        float[,] heatmapBuffer = new float[9, 9];


        protected override void Awake()
        {
            base.Awake();
            ins = this;
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

        internal void Move2Center()
        {
            Device.TargetMove(id, 4, 4, -10, 10);
        }

        internal void RequestMove(Env.Action action)
        {
            this.action = action;
            (var r, var c) = Env.Translate(action, game.envA.row, game.envA.col);

            if (ieMotion != null && !isPerforming && this.targetCoords.x == r && this.targetCoords.y == c)
                return;

            this.targetCoords = new Vector2Int(r, c);

            StopMotion();
            ieMotion = IE_Move();
            StartCoroutine(ieMotion);
        }

        internal void RequestMove(int row, int col)
        {
            if (ieMotion != null && !isPerforming && this.targetCoords.x == row && this.targetCoords.y == col)
                return;

            this.targetCoords = new Vector2Int(row, col);

            StopMotion();
            ieMotion = IE_Move();
            StartCoroutine(ieMotion);
        }

        internal override void StopMotion(bool sendCmd = false)
        {
            isMoving = false;
            base.StopMotion(sendCmd);
        }




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
                // Predicting Heatmap
                this.isPredicting = true;
                ClearHeatmapBuffer();

                Debug.Log("AICon.IE_Think : predict heatmap");
                yield return IE_PredictHeatmap(game.envA.Clone(), this.heatmapPredictSteps);
                Array.Copy(this.heatmapBuffer, this.heatmap, this.heatmap.Length);
                if (isPause) continue;
                this.isPredicting = false;
                this.heatmapCallback?.Invoke();
                this.thinkCallback?.Invoke(1);

                // Interval - simulate time of thinking
                var interval = (float) game.envA.passedColorSpaceCnt / game.envA.quest.Length;
                interval = interval * intervalEnd + (1-interval) * intervalBegin;
                yield return new WaitForSecondsRealtime(interval);
                if (isPause) continue;
                this.thinkCallback?.Invoke(2);

                // Request Agent Action
                this.isActReceived = false;
                agent.RequestAct(game.envA);
                Debug.Log("AICon.IE_Think : RequestAct");
                yield return new WaitUntil(()=>isActReceived);
                yield return new WaitForEndOfFrame();
                if (isPause) continue;

                Debug.Log("AICon.IE_Think : wait moving");
                yield return new WaitUntil(()=>!isMoving);

                Debug.Log("AICon.IE_Think : end");
                this.thinkCallback?.Invoke(3);
            }
        }

        IEnumerator IE_Move()
        {
            Debug.Log($"AICon.IE_Move : Begin");
            isMoving = true;

            Debug.Log($"AICon.IE_Move : TargetMove({targetCoords.x}, {targetCoords.y})");
            Device.TargetMove(id, targetCoords.x, targetCoords.y, -10, 10);

            float retryTime = 0;

            // Wait Cube to Arrive
            while (Device.ID2SpaceCoord(cube.x, cube.y) != targetCoords)
            {
                yield return new WaitForSecondsRealtime(0.5f);
                retryTime += 0.5f;


                if (!cube.isConnected)
                {
                    yield return new WaitUntil(()=>cube.isConnected);
                    Debug.Log($"AICon.IE_Move : TargetMove({targetCoords.x}, {targetCoords.y}) again on reconnection");
                    Device.TargetMove(id, targetCoords.x, targetCoords.y, -10, 10);
                }
                else if (retryTime > 10)
                {
                    retryTime = 10;
                    Debug.Log($"AICon.IE_Move : TargetMove({targetCoords.x}, {targetCoords.y}) again on timeout(10s)");
                    Device.TargetMove(id, targetCoords.x, targetCoords.y, -10, 10);
                }
            }

            // Simulate Chant time 1s
            yield return new WaitForSecondsRealtime(1f);

            // Apply Step to game
            game.MoveA(action);
            isMoving = false;
        }

        IEnumerator IE_PredictHeatmap(Env env, int maxSteps=6, float parentProb=1, int step=0)
        {
            if (maxSteps == step) yield break;
            if (parentProb < 0.01f) yield break;

            this.isActReceived = false;
            this.agent.RequestAct(env);
            yield return new WaitUntil(()=>isActReceived);

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
                RequestMove(action);
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
