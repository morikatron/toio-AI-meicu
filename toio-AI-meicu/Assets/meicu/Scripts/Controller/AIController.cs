using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;


namespace toio.AI.meicu
{
    public class AIController : MonoBehaviour
    {
        internal static AIController ins { get; private set; }

        public GameAgent agent;
        public Game game;
        public int predictSteps = 8;

        internal bool isPredicting { get; private set; } = false;
        // internal float[,] heatmap { get; private set; }
        internal event Action heatmapCallback;
        internal bool isPause = false;
        internal float speedFactor = 0.1f;
        internal ref readonly float[,] Heatmap => ref heatmap;

        Cube cube;
        bool isActReceived = false;
        Env.Action action;
        Vector2Int targetCoords;
        float[,] heatmap = new float[9, 9];
        float[,] heatmapBuffer = new float[9, 9];


        void OnEnable()
        {
            ins = this;
            agent.actCallback = OnAgentAct;
            game.startCallback += OnGameStarted;
            game.stepCallbackA += OnGameStep;
            game.overCallbackA += OnGameOver;
            game.stopCallback += OnGameStop;
        }

        void Stop()
        {
            StopAllCoroutines();
            isPredicting = false;
            isActReceived = false;
            ClearHeatmap();
            ClearHeatmapBuffer();
        }

        internal void Init()
        {
            if (Device.cubes.Count >= 2)
            {
                cube = Device.cubes[1];
                cube.idCallback.AddListener("A", OnID);
                cube.idMissedCallback.AddListener("A", OnIDMissed);
            }
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

                // Predicting Heatmap
                this.isPredicting = true;
                ClearHeatmapBuffer();
                yield return IE_PredictHeatmap(game.envA.Clone(), this.predictSteps);
                Array.Copy(this.heatmapBuffer, this.heatmap, this.heatmap.Length);
                if (isPause) continue;
                this.heatmapCallback?.Invoke();
                this.isPredicting = false;

                // Interval
                var interval = Mathf.Max(0.3f, (game.envA.quest.Length - game.envA.passedColorSpaceCnt) * (0.8f + 0.3f * (1 - speedFactor)) + 2f * (1 - speedFactor) - 2f);
                yield return new WaitForSecondsRealtime(interval);
                if (isPause) continue;

                // Request Agent Action
                this.isActReceived = false;
                agent.RequestAct(game.envA);
                yield return new WaitUntil(()=>isActReceived);
                yield return new WaitForSecondsRealtime(0.1f);
                if (isPause) continue;

                // Wait Cube to Arrive
                while (true)
                {
                    var coord = Device.ID2SpaceCoord(cube.x, cube.y);
                    if (coord == targetCoords)
                        break;
                    else
                    {
                        MoveCube();
                        yield return new WaitForSecondsRealtime(0.5f);
                    }
                }
                yield return new WaitForSecondsRealtime(0.5f);
                if (isPause) continue;

                // Apply Step to game
                game.MoveA(action);
                yield return new WaitForSecondsRealtime(0.5f);
            }
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
            var e0 = Mathf.Exp(feature[0]);
            var e1 = Mathf.Exp(feature[1]);
            var e2 = Mathf.Exp(feature[2]);
            var e3 = Mathf.Exp(feature[3]);
            var sum = e0+e1+e2+e3;
            ps[0] = e0 / sum;
            ps[1] = e1 / sum;
            ps[2] = e2 / sum;
            ps[3] = e3 / sum;

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


        void OnID(Cube c)
        {

        }

        void OnIDMissed(Cube c)
        {

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
            {
                this.action = action;
                (var r, var c) = Env.Translate(this.action, game.envA.row, game.envA.col);
                this.targetCoords = new Vector2Int(r, c);
                MoveCube();
            }
        }

        void MoveCube()
        {
            if (Device.cubes.Count < 2) return;
            Device.TargetMove(1, targetCoords.x, targetCoords.y, -10, 10);
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
