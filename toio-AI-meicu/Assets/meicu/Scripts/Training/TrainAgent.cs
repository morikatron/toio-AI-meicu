using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using UnityEngine.Rendering;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.UI;


namespace toio.AI.meicu.Training
{

    public class TrainAgent : Agent
    {
        public UIBoard uiBoard;
        public UIQuest uiQuest;

        public EnvParas paras;

        [Header("For Test")]
        public float[] accuracies;
        public Text textEpisodes;
        public Text textAccuracy;
        public bool isConstQuest = false;

        private Env env = new Env();

        // For displaying
        private int[] questCounts;
        private int[] successCounts;
        private List<Vector2Int> traj = new List<Vector2Int>();
        private int episodes = 0;
        private List<bool> shiftSuccesss = new List<bool>();
        private int shiftSuccessCount = 0;


        bool isRendering { get { return SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null;} }


        public void Start()
        {
            uiQuest?.Reset();
            uiQuest?.HideP();
            uiQuest?.HideA();

            // Not training
            if (!Academy.Instance.IsCommunicatorOn)
            {
                accuracies = new float[9];
                questCounts = new int[9];
                successCounts = new int[9];
            }
        }

        public override void OnEpisodeBegin()
        {
            isEnded = false;

            // Get Environment Parameters
            paras.Fetch();
            var questSize = UnityEngine.Random.Range(paras.questMinScale, paras.questMaxScale+1);
            int startRow = 4; int startCol = 4;
            if (paras.randomStart)
            {
                startRow = UnityEngine.Random.Range(0, 9);
                startCol = startRow % 2 == 0 ? UnityEngine.Random.Range(0, 5) * 2 : UnityEngine.Random.Range(0, 4) * 2 + 1;
            }

            // Reset Environment
            env.Reset(startRow, startCol);

            // Generate Quest
            if (!isConstQuest || env.quest == null)
                env.SetQuest(env.GenerateQuest(questSize));

            // UI if not training in Headless mode
            if (isRendering)
            {
                traj.Clear();

                uiBoard.Reset();
                uiBoard.biasA = Vector2Int.zero;
                uiBoard.ShowKomaA(env.row, env.col, 1.6f);
                uiBoard.ShowGoal(env.quest.goalRow, env.quest.goalCol);

                uiQuest.ShowQuest(env.quest);
                uiQuest.HideP();
                uiQuest.ShowA(0);
            }
        }


        public override void CollectObservations(VectorSensor sensor)
        {
            // Vecotr Observation size = 275
            // passed size = 81
            var state = this.env.GetState();
            List<float> passed = new List<float>();
            foreach (var space in state)
                passed.Add(space == Env.Space.Passed? 1:0);
            sensor.AddObservation(passed);

            // self size = 81
            sensor.AddOneHotObservation(env.row * 9 + env.col, 81);

            // goal size = 81
            sensor.AddOneHotObservation(env.quest.goalRow * 9 + env.quest.goalCol, 81);

            // prob (type1) size = 8 * 4 = 32
            for (int i=0; i<8; i++) // max length is 8
            {
                var step = i + env.passedColorSpaceCnt;
                if (step < env.quest.Length && env.quest.colors[step] != Env.Space.None)
                    sensor.AddOneHotObservation((int)env.quest.colors[step]-2, 4);
                else
                {
                    var z = new float[4]{0, 0, 0, 0};
                    sensor.AddObservation(z);
                }
            }
        }


        public override void OnActionReceived(ActionBuffers actions)
        {
            if (isEnded) return;

            var a = actions.DiscreteActions[0];

            // Take Action
            Env.Response res = env.Step((Env.Action)a);

            // UI
            if (isRendering)
            {
                if (res != Env.Response.FailOut)
                    traj.Add(new Vector2Int(env.row, env.col));

                uiBoard.ShowKomaA(env.row, env.col, 1.6f);
                uiBoard.ShowTrajA(traj.ToArray());

                uiQuest.ShowA(traj.Count - (Env.IsResponseFail(res)?1:0));
            }

            // Episode control
            if (res == Env.Response.Goal)
            {
                SetReward(1);
                CountResult(true);
                DelayedEndEpisode(true);
            }
            else if (Env.IsResponseFail(res))
            {
                CountResult(false);
                DelayedEndEpisode(false);
            }
            else if (res == Env.Response.StepColor)
            {
                SetReward(paras.colorReward);
            }
        }

        private void CountResult(bool success)
        {
            if (Academy.Instance.IsCommunicatorOn) return;

            int len = env.quest.Length;
            questCounts[len] += 1;
            if (success)
                successCounts[len] += 1;
            accuracies[len] = (float)successCounts[len] / questCounts[len];
        }

        bool isEnded = false;
        private void DelayedEndEpisode(bool success)
        {
            if (!isRendering)
            {
                EndEpisode();
                return;
            }

            isEnded = true;

            episodes ++;
            if (shiftSuccesss.Count > 100)
            {
                shiftSuccessCount -= shiftSuccesss[0]? 1:0;
                shiftSuccesss.RemoveAt(0);
            }
            shiftSuccesss.Add(success);
            shiftSuccessCount += success? 1:0;
            int acc = Mathf.RoundToInt(shiftSuccessCount * 100f / shiftSuccesss.Count);

            textEpisodes.text = $"試行回数  {episodes}";
            textAccuracy.text = $"直近ゴール率 {acc}%";

            IEnumerator ie()
            {
                yield return new WaitForSeconds(0.05f);
                EndEpisode();
            }

            StartCoroutine(ie());
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
        }
    }

}
