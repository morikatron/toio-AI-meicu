using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


namespace toio.AI.meicu
{
    public class GameAgent : Agent
    {
        internal Action<Env.Action> actCallback;

        private Env env;


        public void Restart()
        {
            EndEpisode();
        }

        public override void OnEpisodeBegin()
        {
        }

        public void RequestAct(Env env)
        {
            this.env = env;
            RequestDecision();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            if (this.env == null) return;

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

            this.env = null;
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var a = (Env.Action) actions.DiscreteActions[0];
            this.actCallback?.Invoke(a);

            // if (m_additionalOutputNames.Length > 0)
            // {
                // var feature = additionalOuput[m_additionalOutputNames[0]];
            //     string info = "[";
            //     foreach (var p in feature)
            //         info += p + ", ";
            //     info += "]";
            //     Debug.Log(info);
            // }
        }

        [HideInInspector]
        public Env.Action manualAction;
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            actionsOut.DiscreteActions.Array[0] = (int) manualAction;
            // Debug.Log("Heu: " + manualAction);
        }
    }

}
