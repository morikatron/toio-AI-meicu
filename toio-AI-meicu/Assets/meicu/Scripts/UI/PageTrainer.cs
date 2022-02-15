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
        private bool waitBtn = false;
        private bool isSt2Failed = false;
        private bool inTraining = false;
        private int stageIdx = 0;
        private List<Vector2Int> rewardCoords = new List<Vector2Int>();

        private int episodesTurn;
        private int episodesTurnLeft;
        private float loss;
        private List<bool> testLog = new List<bool>();

        private int clearedStages = 2; // TODO move to Prefs in future


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
                inTraining = false;

                BeginPhaseEntry();
            }
            else
            {
                StopAllCoroutines();
            }
        }

        internal void Pause()
        {

        }

        private void BeginPhaseEntry()
        {
            if (phase != Phase.Entry) return;

            IEnumerator IE_Entry()
            {
                waitBtn = false;

                uiBoard.gameObject.SetActive(phase != Phase.Entry);
                uiQuest.gameObject.SetActive(phase != Phase.Entry);
                ui.transform.Find("Entries").gameObject.SetActive(phase == Phase.Entry);
                btnNext.gameObject.SetActive(phase != Phase.Entry);

                text.text = "課題をせんたくしてください。";

                ui.transform.Find("Entries").Find("Btn1").GetComponent<Button>().interactable = clearedStages > 0;
                ui.transform.Find("Entries").Find("Btn2").GetComponent<Button>().interactable = clearedStages > 1;

                yield break;
            }
            StopAllCoroutines();
            StartCoroutine(IE_Entry());
        }

        private void BeginPhaseQuest()
        {
            if (phase != Phase.Quest) return;

            env.Reset();
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
                var quest = env.GenerateQuest(2);
                env.SetQuest(quest);
            }
            else if (stageIdx == 2)
            {
                var quest = env.GenerateQuest(3);
                env.SetQuest(quest);
            }

            IEnumerator IE_Quest()
            {
                waitBtn = false;

                uiBoard.gameObject.SetActive(phase != Phase.Entry);
                uiQuest.gameObject.SetActive(phase != Phase.Entry);
                ui.transform.Find("Entries").gameObject.SetActive(phase == Phase.Entry);
                btnNext.gameObject.SetActive(phase != Phase.Entry);

                if (stageIdx == 0)
                    text.text = "このお題を解けるように\n自分のキューブを学習させてみよう！\n\n最初は長さ1のお題";
                else if (stageIdx == 1)
                    text.text = "もう少し長い問題にチャレンジしてみよう！\n\n今回は試行錯誤を500回するよ。";
                else if (stageIdx == 2)
                    text.text = "長さ3のお題も500回で学習できるかな？";

                uiBoard.ShowGoal(env.quest.goalRow, env.quest.goalCol);
                uiBoard.ShowKomaP(4, 4);
                uiQuest.ShowQuest(env.quest);
                uiQuest.ShowP(0);
                btnNext.interactable = true;
                yield break;
            }
            StopAllCoroutines();
            StartCoroutine(IE_Quest());
        }

        private void BeginPhasePlan()
        {
            if (phase != Phase.Plan) return;

            if (stageIdx == 0)
            {
                this.episodesTurn = 100;
                this.episodesTurnLeft = this.episodesTurn;
            }
            else if (stageIdx == 1)
            {
                this.episodesTurn = 500;
                this.episodesTurnLeft = this.episodesTurn;
            }
            else if (stageIdx == 2)
            {
                this.episodesTurn = 500;
                this.episodesTurnLeft = this.episodesTurn;
            }

            if (isSt2Failed)
                agent.Reset();

            IEnumerator IE_Plan()
            {
                if (stageIdx == 0)
                {
                    btnNext.interactable = true;
                    waitBtn = true;
                    text.text = "キューブは色もゴールも見えないので、\n最初はてきとうに動くけど、";
                    yield return new WaitUntil(() => !waitBtn);

                    waitBtn = true;
                    text.text = "報酬（ほうしゅう）をマスにおいて、\nキューブがたまたまそこに着いたら、\n今の動きは正しいとわかるのだ";
                    yield return new WaitUntil(() => !waitBtn);

                    btnNext.interactable = false;
                    text.text = "ゴールのマスをクリックして、\n報酬をおいてみてください";
                    yield return new WaitUntil(() => rewardCoords.Count > 0);

                    btnNext.interactable = true;
                    text.text = "OKボタン押すと、学習が始まるよ\n\n今回は試行錯誤を100回するよ";
                }
                else if (stageIdx == 1)
                {
                    btnNext.interactable = false;
                    text.text = "マスをクリックして、「報酬」を１つおいてください!";
                    yield return new WaitUntil(() => rewardCoords.Count > 0);
                    btnNext.interactable = true;
                }
                else if (stageIdx == 2)
                {
                    btnNext.interactable = false;
                    if (!isSt2Failed)
                    {
                        text.text = "とりあえず「報酬」１つだけで試してみよう";
                        yield return new WaitUntil(() => rewardCoords.Count > 0);
                    }
                    else
                    {
                        text.text = "さて、「報酬」を２つを置いてみてください";
                        yield return new WaitUntil(() => rewardCoords.Count == 2);
                    }
                    btnNext.interactable = true;
                }
            }
            StopAllCoroutines();
            StartCoroutine(IE_Plan());
        }

        private void BeginPhaseTrain()
        {
            if (phase != Phase.Train) return;

            IEnumerator IE_Train()
            {
                btnNext.interactable = false;
                this.inTraining = true;

                while (this.episodesTurnLeft-- > 0)
                {
                    env.Reset();
                    // UI
                    uiBoard.ShowKomaP(env.row, env.col);
                    uiQuest.ShowP(env.passedSpaceCnt);
                    text.text = $"試行回数： {this.episodesTurn-this.episodesTurnLeft} / {this.episodesTurn}\n" + $"ロス(仮)： {this.loss}";
                    yield return new WaitForSecondsRealtime(0.05f);

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

                        if (!Env.IsResponseFail(res) && this.rewardCoords.Contains(new Vector2Int(row_, col_)))
                        {
                            reward += 1;
                        }
                        agent.Collect(row, col, (int)action, reward, done, row_, col_);

                        yield return new WaitForSecondsRealtime(0.05f);
                        if (done) break;
                    }

                    if (agent.bufferLength > 5)
                        this.loss = agent.Train();
                }

                this.inTraining = false;

                text.text += "\n\n学習終了。OKボタンで試験を受けよう！";
                btnNext.interactable = true;
            }
            StopAllCoroutines();
            StartCoroutine(IE_Train());
        }

        private void BeginPhaseTest()
        {
            if (phase != Phase.Test) return;

            IEnumerator IE_Test()
            {
                btnNext.interactable = false;
                text.text = "試験\n";
                testLog.Clear();

                int successCnt = 0;
                for (int ieps = 0; ieps < 10; ieps ++)
                {
                    env.Reset();
                    // UI
                    uiBoard.ShowKomaP(env.row, env.col);
                    uiQuest.ShowP(env.passedSpaceCnt);
                    yield return new WaitForSecondsRealtime(0.4f);

                    while (true)
                    {
                        // Step
                        var row = env.row; var col = env.col;
                        var action = agent.GetActionTest(env.row, env.col);
                        var res = env.Step(action);
                        bool done = Env.IsResponseFail(res) || res == Env.Response.Goal;

                        // UI
                        uiBoard.ShowKomaP(env.row, env.col);
                        uiQuest.ShowP(env.passedSpaceCnt);
                        if (done)
                        {
                            successCnt += res == Env.Response.Goal? 1: 0;
                            testLog.Add(res == Env.Response.Goal);
                            text.text += res == Env.Response.Goal? "O" : "X";
                        }

                        yield return new WaitForSecondsRealtime(0.4f);
                        if (done) break;
                    }
                }

                text.text += $"\n点数 = {successCnt}/10";

                if (successCnt > 5)
                {
                    text.text += "\n6点以上なので合格! ";
                    clearedStages = Mathf.Max(clearedStages, stageIdx+1);

                    btnNext.interactable = true;
                    waitBtn = true;
                    yield return new WaitUntil(() => !waitBtn);

                    if (stageIdx == 0)
                        text.text = "報酬をゴールにおくことで、\nキューブにゴールへの動きかたを\n学習させることができたね!";
                    else if (stageIdx == 1)
                        text.text = "お題が長くなると、\n学習に必要な試行回数がも多くなったね";
                    else if (stageIdx == 2 && isSt2Failed)
                        text.text = "報酬を2つ置くことで、\nキューブをとおいゴールまでゆうどうできたね!";

                }
                else
                {
                    text.text += "\n6未満なので不合格！";
                    btnNext.interactable = true;
                    waitBtn = true;
                    yield return new WaitUntil(() => !waitBtn);

                    if (stageIdx == 0)
                    {
                        text.text = "報酬をちゃんとゴールに置いたかな？\nリトライしてね";
                    }
                    else if (stageIdx == 1)
                    {
                        text.text = "報酬の位置を変えてリトライしてね";
                    }
                    else if (stageIdx == 2)
                    {
                        if (!isSt2Failed)
                        {
                            text.text = "長さ3のお題に\n報酬1つで試行500回は足りないね...\n\n報酬2つならどうかな？";
                        }
                        else
                        {
                            text.text = "報酬の位置を変えてリトライしてね";
                        }
                    }
                }

                btnNext.interactable = true;
            }
            StopAllCoroutines();
            StartCoroutine(IE_Test());
        }

        private float EpsilonScheduler(int epsLeft, int nEps)
        {
            return (float)epsLeft/nEps * 0.4f + 0.1f;
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

            if (stageIdx == 0 || stageIdx == 1)
            {
                this.PutReward(rowCol.x, rowCol.y, 1);
            }
            else if (stageIdx == 2)
            {
                this.PutReward(rowCol.x, rowCol.y, 2);
            }
        }

        public void OnBtnNext()
        {
            if (waitBtn)
            {
                waitBtn = false;
                return;
            }

            if (phase == Phase.Quest)
            {
                phase = Phase.Plan;
                BeginPhasePlan();
            }
            else if (phase == Phase.Plan)
            {
                phase = Phase.Train;
                BeginPhaseTrain();
            }
            else if (phase == Phase.Train)
            {
                phase = Phase.Test;
                BeginPhaseTest();
            }
            else if (phase == Phase.Test)
            {
                if (stageIdx == 2 && !isSt2Failed)
                {
                    isSt2Failed = true;
                    phase = Phase.Plan;
                    BeginPhasePlan();
                    return;
                }

                phase = Phase.Entry;
                BeginPhaseEntry();
            }
        }

        public void OnBtnEntry(int idx)
        {
            if (phase != Phase.Entry) return;
            this.phase = Phase.Quest;
            this.stageIdx = idx;
            BeginPhaseQuest();
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
            return (Env.Action)SampleFromQ(qs);
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
        static int SampleFromQ(float[] qs, float max_return = 1)
        {
            float scale = 4f / max_return;
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
    }

}
