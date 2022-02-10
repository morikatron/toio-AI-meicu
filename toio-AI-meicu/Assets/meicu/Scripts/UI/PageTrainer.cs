using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace toio.AI.meicu
{
    public class PageTrainer : MonoBehaviour
    {
        public GameObject ui;
        public UIBoard uiBoard;
        public UIQuest uiQuest;
        public TMP_Text text;
        public Button btnNext;
        public UISwitch btnBGM;
        public UIMeicu meicu;

        private Env env = new Env();
        private QAgent agent;
        private Phase phase;
        private int stageIdx = 0;
        private List<Vector2Int> rewardCoords = new List<Vector2Int>();

        private int episodesTurn;
        private int episodesTurnLeft;


        void OnEnable()
        {
            uiBoard.onSpaceClicked += OnSpaceClicked;
        }

        internal void SetActive(bool active)
        {
            if (ui.activeSelf == active) return;
            ui.SetActive(active);

            phase = Phase.Entry;

            if (active)
            {
                btnBGM.isOn = AudioPlayer.ins.isBGMOn;

                env.Reset();
                uiBoard.Reset();
                uiQuest.Reset();

                Refresh();
            }
            else
            {
            }
        }

        internal void Pause()
        {

        }

        private IEnumerator ie_Refresh;
        internal void Refresh()
        {
            if (ie_Refresh != null) StopCoroutine(ie_Refresh);
            ie_Refresh = IE_Refresh();
            StartCoroutine(ie_Refresh);
        }

        private IEnumerator IE_Refresh()
        {
            uiBoard.gameObject.SetActive(phase != Phase.Entry);
            uiQuest.gameObject.SetActive(phase != Phase.Entry);
            ui.transform.Find("Entries").gameObject.SetActive(phase == Phase.Entry);
            btnNext.gameObject.SetActive(phase != Phase.Entry);

            btnNext.interactable = false;

            if (phase == Phase.Entry)
            {
                text.text = "課題をせんたくしてください。";

                // TODO Finger?
            }
            else if (phase == Phase.Quest)
            {
                text.text = "この課題を解けるように\n自分のキューブを学習させてみよう！";

                uiBoard.ShowGoal(env.quest.goalRow, env.quest.goalCol);
                uiBoard.ShowKomaP(4, 4);
                uiQuest.ShowQuest(env.quest);
                uiQuest.ShowP(0);
            }
            else if (phase == Phase.Plan)
            {
                if (stageIdx == 0)
                {
                    text.text = "マスをクリックして、「報酬」を１つ配置してください!";
                    yield return new WaitUntil(() => rewardCoords.Count>0);
                }
                else if (stageIdx == 1)
                {
                    text.text = "マスをクリックして、「報酬」を１つ配置してください!";
                    yield return new WaitUntil(() => rewardCoords.Count>0);
                }
            }
            else if (phase == Phase.Train)
            {

            }

            yield return new WaitForSecondsRealtime(0.5f);
            btnNext.interactable = true;
        }

        private IEnumerator IE_Train()
        {
            if (phase != Phase.Train) yield break;

            while (this.episodesTurnLeft-- > 0)
            {
                env.Reset();

                agent.e = EpsilonScheduler(episodesTurnLeft, episodesTurn);

                while (true)
                {
                    // Step
                    var row = env.row; var col = env.col;
                    var action = agent.GetActionTraining(env.row, env.col);
                    var res = env.Step(action);
                    var row_ = env.row; var col_ = env.col;
                    float reward = 0;
                    bool done = false;

                    // UI
                    uiBoard.ShowKomaP(row_, col_);
                    uiQuest.ShowP(env.passedSpaceCnt);

                    // Collect
                    if (Env.IsResponseFail(res))
                    {
                        row_ = -1; col_ = -1; done = true;
                    }
                    if (res == Env.Response.Goal)
                    {
                        done = true;
                    }

                    if (this.rewardCoords.Contains(new Vector2Int(row_, col_)))
                    {
                        reward += 1;
                    }
                    agent.Collect(row, col, (int)action, reward, done, row_, col_);

                    yield return new WaitForSecondsRealtime(0.1f);
                    if (done) break;
                }

                // if (episodesTurnLeft % 5 == 0 && agent.bufferLength > 5)
                agent.Train();
            }
        }

        private float EpsilonScheduler(int epsLeft, int nEps)
        {
            if (stageIdx == 0)
            {
                return (float)epsLeft/nEps * 0.4f + 0.1f;
            }
            return 0.1f;
        }


        private void InitQuest()
        {
            uiBoard.Reset();
            uiQuest.Reset();
            rewardCoords.Clear();

            agent = new QAgent();

            if (stageIdx == 0)
            {
                var quest = env.GenerateQuest(1);
                env.SetQuest(quest);
            }
            else if (stageIdx == 1)
            {
                var quest = env.GenerateQuest(1);
                env.SetQuest(quest);
            }
        }
        private void InitPlan()
        {
            if (phase != Phase.Plan) return;

            if (stageIdx == 0)
            {
                this.episodesTurn = 100;
                this.episodesTurnLeft = this.episodesTurn;
            }
        }

        private void PutReward(int row, int col, int maxCount)
        {
            Vector2Int coords = new Vector2Int(row, col);
            if (this.rewardCoords.Contains(coords)) return;

            this.rewardCoords.Add(coords);
            while (this.rewardCoords.Count > maxCount)
                this.rewardCoords.RemoveAt(0);
            uiBoard.PutReward(row, col, maxCount);
        }


        #region ========== Callbacks ==========

        private void OnSpaceClicked(Vector2Int rowCol)
        {
            if (phase != Phase.Plan) return;

            if (stageIdx == 0)
            {
                this.PutReward(rowCol.x, rowCol.y, 1);
            }
        }

        public void OnBtnNext()
        {
            if (phase == Phase.Quest)
            {
                phase = Phase.Plan;
                InitPlan();
                Refresh();
            }
            else if (phase == Phase.Plan)
            {
                phase = Phase.Train;
                StartCoroutine(IE_Train());
                Refresh();
            }
        }

        public void OnBtnEntry(int idx)
        {
            if (phase != Phase.Entry) return;
            this.phase = Phase.Quest;
            this.stageIdx = idx;
            InitQuest();
            Refresh();
        }
        #endregion


        internal enum Phase {
            Entry, Quest, Plan, Train, Test
        }

    }

    internal class QAgent
    {
        public float[,,] Q;
        public float e = 0.5f;
        public float lr = 0.1f;
        public float gamma = 0.95f;

        public QAgent()
        {
            this.Q = new float[9, 9, 4];
            // Clear QUpdate
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                    for (int a = 0; a < 4; a++)
                        this.Q[r, c, a] = UnityEngine.Random.Range(-0.3f, 0.3f);
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
            var i = SampleIdx(qs);
            return (Env.Action)i;
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
            for (int t = 0; t < this.rowBuffer.Count; t++)
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
                var dq = reward + this.gamma * q_s.Max() - qs[action];
                this.QUpdate[row, col, action] += dq / this.rowBuffer.Count;
                lossSum += dq / this.rowBuffer.Count;
            }

            // Update
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                    for (int a = 0; a < 4; a++)
                        this.Q[r, c, a] += this.QUpdate[r, c, a] * this.lr;

            Debug.Log($"[{this.Q[4, 4, 0]} {this.Q[4, 4, 1]} {this.Q[4, 4, 2]} {this.Q[4, 4, 3]}]");
            Debug.Log(lossSum);

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
    }

}
