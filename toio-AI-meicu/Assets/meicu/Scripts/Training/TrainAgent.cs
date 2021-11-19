using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


namespace toio.AI.meicu.Training
{

    public class TrainAgent : Agent
    {
        public UIBoard uiBoard;
        public UIQuest uIQuest;

        public EnvParas paras;

        [Header("For Test")]
        public float[] accuracies;

        private Env env = new Env();
        private int[] questCounts;
        private int[] successCounts;


        bool isRendering { get { return SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null;} }


        public void Start()
        {
            uIQuest?.Reset();
            uIQuest?.HideP();
            uIQuest?.HideA();

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
            // Get Environment Parameters
            paras.Fetch();
            var questSize = Random.Range(paras.questMinScale, paras.questMaxScale+1);
            int startRow = 4; int startCol = 4;
            if (paras.randomStart)
            {
                startRow = Random.Range(0, 9);
                startCol = startRow % 2 == 0 ? Random.Range(0, 5) * 2 : Random.Range(0, 4) * 2 + 1;
            }

            // Reset Environment
            env.Reset(startRow, startCol);

            // Generate Quest
            MeiQuest quest = env.GenerateQuest(questSize);
            env.SetQuest(quest);

            // UI if not training in Headless mode
            if (isRendering)
            {
                uiBoard.Reset();
                uiBoard.ShowKomaA(env.row, env.col);
                uiBoard.ShowGoal(quest.goalRow, quest.goalCol);

                uIQuest.ShowQuest(quest);
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
            var a = actions.DiscreteActions[0];

            // Take Action
            Env.Response res = env.Step((Env.Action)a);

            // UI
            if (isRendering)
            {
                uiBoard.ShowKomaA(env.row, env.col);
            }

            // Episode control
            if (res == Env.Response.Goal)
            {
                SetReward(1);
                CountResult(true);
                EndEpisode();
            }
            else if (
                res == Env.Response.FailOut ||
                res == Env.Response.FailPassed ||
                res == Env.Response.FailWrong ||
                res == Env.Response.FailEarlyGoal)
            {
                CountResult(false);
                EndEpisode();
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

        public override void Heuristic(in ActionBuffers actionsOut)
        {
        }
    }

}
