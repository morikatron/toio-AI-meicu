using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using static System.Math;


namespace toio.AI.meicu.Training
{
    public class Paras : MonoBehaviour
    {
        EnvironmentParameters remote;

        public int questMaxScale = 3;
        public int questMinScale = 3;

        public float colorReward = 0.01f;


        void Start()
        {
            this.remote = Academy.Instance.EnvironmentParameters;
        }

        public void Fetch(){
            float tmp;
            tmp = GetRemotePara("questMaxScale");
            if (tmp != -1) this.questMaxScale = (int) tmp;

            tmp = GetRemotePara("questMinScale");
            if (tmp != -1) this.questMinScale = (int) tmp;

            tmp = GetRemotePara("colorReward");
            if (tmp != -1) this.colorReward = tmp;
        }
        protected float GetRemotePara(string name)
        {
            return this.remote.GetWithDefault(name, -1);
        }
    }

}
