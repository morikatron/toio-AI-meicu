using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace toio.AI.meicu
{

    internal class QAgent
    {
        public float[,,] Q;
        public float e = 0.8f;
        public float lr = 0.2f;
        public float gamma = 0.95f;

        public QAgent()
        {
            this.Q = new float[9, 9, 4];
            Reset();
        }

        public void Reset()
        {
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                    for (int a = 0; a < 4; a++)
                        this.Q[r, c, a] = UnityEngine.Random.Range(0f, 0.1f);

            this.rowBuffer.Clear();
            this.colBuffer.Clear();
            this.actionBuffer.Clear();
            this.rewardBuffer.Clear();
            this.doneBuffer.Clear();
            this.row_Buffer.Clear();
            this.col_Buffer.Clear();
        }

        public Env.Action GetBestAction(int row, int col)
        {
            var qs = Enumerable.Range(0, 4).Select(x => this.Q[row, col, x]).ToArray();
            var i = Array.IndexOf(qs, qs.Max());
            return (Env.Action)i;
        }

        public Env.Action GetActionTraining(int row, int col)
        {
            if (UnityEngine.Random.Range(0f, 1f) > e)
            {
                return GetBestAction(row, col);
            }
            else
            {
                return (Env.Action)(UnityEngine.Random.Range(0, 4));
            }
        }

        public Env.Action GetActionTest(int row, int col)
        {
            var qs = Enumerable.Range(0, 4).Select(x => this.Q[row, col, x]).ToArray();
            return (Env.Action)SampleFromQ(qs, scale:8);
        }

        private List<int> rowBuffer = new List<int>();
        private List<int> colBuffer = new List<int>();
        private List<int> actionBuffer = new List<int>();
        private List<float> rewardBuffer = new List<float>();
        private List<bool> doneBuffer = new List<bool>();
        private List<int> row_Buffer = new List<int>();
        private List<int> col_Buffer = new List<int>();

        public int bufferLength => rowBuffer.Count;

        /// <summary>
        /// Input row_=-1, col_=-1 for failure
        /// </summary>
        public int Collect(int row, int col, int action, float reward, bool done, int row_, int col_)
        {
            this.rowBuffer.Add(row);
            this.colBuffer.Add(col);
            this.actionBuffer.Add(action);
            this.rewardBuffer.Add(reward);
            this.doneBuffer.Add(done);
            this.row_Buffer.Add(row_);
            this.col_Buffer.Add(col_);
            return this.rowBuffer.Count;
        }

        private float[,,] QUpdate = new float[9, 9, 4];
        public float Train()
        {
            // Clear QUpdate
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                    for (int a = 0; a < 4; a++)
                        this.QUpdate[r, c, a] = 0;

            // Calc. gradient
            float lossSum = 0;
            float returns = 0;
            int nsteps = this.rowBuffer.Count;
            for (int t = nsteps-1; t >=0; t--)
            {
                var row = this.rowBuffer[t];
                var col = this.colBuffer[t];
                var action = this.actionBuffer[t];
                var reward = this.rewardBuffer[t];
                var done = this.doneBuffer[t];
                var row_ = this.row_Buffer[t];
                var col_ = this.col_Buffer[t];

                var qs = Enumerable.Range(0, 4).Select(x => this.Q[row, col, x]).ToArray();
                var q_s = row_ == -1 || done? new float[]{0, 0, 0, 0} : Enumerable.Range(0, 4).Select(x => this.Q[row_, col_, x]).ToArray();
                returns = (done || t == nsteps-1)? reward + this.gamma * q_s.Max() : reward + this.gamma * returns;
                var dq = returns - qs[action];
                this.QUpdate[row, col, action] += dq / nsteps;
                lossSum += Mathf.Abs(dq) / nsteps;
            }

            // Update
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                    for (int a = 0; a < 4; a++)
                        this.Q[r, c, a] += this.QUpdate[r, c, a] * this.lr;

            Debug.Log($"[{this.Q[4, 4, 0]} {this.Q[4, 4, 1]} {this.Q[4, 4, 2]} {this.Q[4, 4, 3]}]");

            // Clear buffer
            this.rowBuffer.Clear();
            this.colBuffer.Clear();
            this.actionBuffer.Clear();
            this.rewardBuffer.Clear();
            this.doneBuffer.Clear();
            this.row_Buffer.Clear();
            this.col_Buffer.Clear();

            return lossSum;
        }

        // Simulate Stochastic Policy from Q values using softmax
        static int SampleFromQ(float[] qs, float scale = 4)
        {
            var qs_scaled = Array.ConvertAll(qs, q => q * scale);
            var softmax = Softmax(qs_scaled);
            var i = SampleIdx(softmax);
            return i;
        }
        static float[] Softmax(float[] logits)
        {
            var exps = Array.ConvertAll(logits, q => Mathf.Exp(q));
            float sum = 0;
            foreach (var e in exps) sum += e;
            return Array.ConvertAll(exps, e=>e/sum);
        }
        static int SampleIdx(float[] probs)
        {
            var p = UnityEngine.Random.Range(0f, 1f);
            float cumu = 0;
            for (var i = 0; i < probs.Length; i++)
            {
                cumu += probs[i];
                if (p < cumu)
                {
                    return i;
                }
            }
            return probs.Length - 1;
        }

        public static float EpsilonScheduler(float start, float end, int epsLeft, int nEps)
        {
            return (start - end) * epsLeft/nEps + end;
        }

    }

}
