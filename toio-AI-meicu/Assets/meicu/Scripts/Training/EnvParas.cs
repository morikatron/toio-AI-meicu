using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using static System.Math;


namespace toio.AI.meicu.Training
{
    public class EnvParas : MonoBehaviour
    {
        EnvironmentParameters remote;

        public int questMaxScale = 8;
        public int questMinScale = 3;
        public float colorReward = 0.01f;
        public bool randomStart = false;


        void Start()
        {
            this.remote = Academy.Instance.EnvironmentParameters;
        }

        public void Fetch(){
            float tmp;
            tmp = GetRemotePara("questMaxScale");
            if (tmp != -1) this.questMaxScale = (int) tmp;
            this.questMaxScale = Mathf.Clamp(this.questMaxScale, questMinScale, 8);

            tmp = GetRemotePara("questMinScale");
            if (tmp != -1) this.questMinScale = (int) tmp;
            this.questMinScale = Mathf.Clamp(this.questMinScale, 1, questMaxScale);

            tmp = GetRemotePara("colorReward");
            if (tmp != -1) this.colorReward = tmp;

            tmp = GetRemotePara("randomStart");
            if (tmp != -1) this.randomStart = tmp > 0;
        }
        protected float GetRemotePara(string name)
        {
            return this.remote.GetWithDefault(name, -1);
        }
    }

}
