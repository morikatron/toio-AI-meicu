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
        public Text textCaption;
        public TMP_Text text;
        public Button btnNext;
        public UISwitch btnBGM;
        public UIMeicu meicu;
        public Game game;

        private Env env = new Env();
        private QAgent agent;
        private Phase phase;
        private bool inTraining = false;
        private int stageIdx = 0;
        private bool isSt0Failed = false;
        private bool isFail = false;

        private int episodesTurn;
        private int episodesTurnLeft;
        private float loss;

        private int clearedStages = 2; // TODO move to Prefs in future


        void OnEnable()
        {
            uiBoard.onSpaceClicked += OnSpaceClicked;
        }

        internal void SetActive(bool active)
        {
            if (ui.activeSelf == active) return;

            phase = Phase.Entry;

            if (active)
            {
                btnBGM.isOn = AudioPlayer.ins.isBGMOn;

                env.Reset();
                uiBoard.Reset();
                uiQuest.Reset();
                uiQuest.HideA();
                inTraining = false;

                game.stepCallbackP += OnGameStepP;
                game.overCallbackP += OnGameOverP;

                BeginPhaseEntry();
            }
            else
            {
                game.StopGame();
                game.stepCallbackP -= OnGameStepP;
                game.overCallbackP -= OnGameOverP;

                AIController.ins.StopMotion();
                AIController.ins.StopAllCoroutines();
                PlayerController.ins.Stop();

                StopAllCoroutines();
            }

            ui.SetActive(active);
        }

        internal void Pause()
        {

        }

        private void BeginPhaseEntry()
        {
            if (phase != Phase.Entry) return;

            // Init UI
            var entriesTr = ui.transform.Find("PhaseEntry");
            textCaption.text = "キミだけのAIを育てよう";
            ui.transform.Find("PhasePlan").gameObject.SetActive(false);
            ui.transform.Find("TextSteps").gameObject.SetActive(false);
            uiBoard.gameObject.SetActive(false);
            uiQuest.gameObject.SetActive(false);
            entriesTr.gameObject.SetActive(true);
            btnNext.gameObject.SetActive(true);
            btnNext.interactable = true;

            var entryBtn0 = entriesTr.Find("Btn0").GetComponent<Button>();
            var entryBtn1 = entriesTr.Find("Btn1").GetComponent<Button>();
            var entryBtn2 = entriesTr.Find("Btn2").GetComponent<Button>();
            entryBtn0.interactable = false;
            entryBtn1.interactable = false;
            entryBtn2.interactable = false;

            IEnumerator IE_Entry()
            {
                text.text = "こんにちは！\nここでは「キミだけのAI」を育ててみる事ができるよ！";
                yield return WaitButton();

                text.text = "「AIを育てる」には、キミと同じようにAIも「学習」する必要があるんだ。";
                yield return WaitButton();

                text.text = "え？「学習」ってどのようにするのかって？";
                yield return WaitButton();

                text.text = "任せて！ボクがこれからひとつひとつ説明していくよ！";
                yield return WaitButton();

                text.text = "キミだけのAIを育てて、ボクと勝負だ！";
                if (clearedStages == 0)
                    text.text += "\n\nまずは最初、「ごほうびを使ってAIを育てよう」を選択してみよう！";
                entryBtn0.interactable = true;
                entryBtn1.interactable = clearedStages > 0;
                entryBtn2.interactable = clearedStages > 1;

                yield break;
            }
            StopAllCoroutines();
            StartCoroutine(IE_Entry());
        }

        private void BeginPhaseQuest()
        {
            if (phase != Phase.Quest) return;

            // Init training agent and environment
            env.Reset();
            agent = new QAgent();

            // Generate Quest by Stage Settings
            int questSize = 2;
            if (stageIdx == 0)
                questSize = 2;
            else if (stageIdx == 1)
                questSize = 3;
            else if (stageIdx == 2)
                questSize = 6;
            Quest quest = env.GenerateQuest(questSize);
            env.SetQuest(quest);

            // Update UI
            textCaption.text = "AIを育てよう";
            uiBoard.gameObject.SetActive(true);
            uiQuest.gameObject.SetActive(true);
            btnNext.gameObject.SetActive(true);
            ui.transform.Find("PhaseEntry").gameObject.SetActive(false);

            uiQuest.Reset();
            uiQuest.HideA();
            uiQuest.ShowP(0);
            uiQuest.ShowQuest(env.quest);
            uiBoard.Reset();
            uiBoard.ShowGoal(env.quest.goalRow, env.quest.goalCol);
            uiBoard.ShowKomaP(4, 4);


            IEnumerator IE_Quest()
            {
                if (stageIdx == 0)
                {
                    text.text = "ようこそ！\nここでは、「ごほうび」を使ってAIを育ててみるよ！";
                    yield return WaitButton();
                    text.text = "「バトル」と同じように、スタートとゴール、そして「どの色を通過するか」という問題が出されているね。";
                    yield return WaitButton();
                    text.text = "キミはスグに「こう進めばいい」と分かったと思うけど、AIは最初は色も場所も分かってないから、「どっちに進めば正解なのか」も全く分からないんだったよね。";
                    yield return WaitButton();
                    text.text = "そんなAIにどうやって「学習」させるのか、それは……。";
                    yield return WaitButton();
                    text.text = "「ごほうび」を使うんだよ！";
                    yield return WaitButton();
                    text.text = "道に「ごほうび」をおいておくと、AIはごほうびをたどって「こっちが正しい道だぞ！」とおぼえていくんだ。";
                    yield return WaitButton();
                    text.text = "そうやって繰り返して覚えていくのが「学習する」ということになるんだ。";
                    yield return WaitButton();
                    text.text = "「ごほうび」を使うんだよ！";
                    yield return WaitButton();
                    text.text = "じゃあ、今度は実際にやってみよう！";
                }
                else if (stageIdx == 1)
                {
                    // TODO
                    text.text = "もう少し長い問題にチャレンジしてみよう！\n\n今回は試行錯誤を500回するよ。";
                }
                else if (stageIdx == 2)
                {
                    // TODO
                    text.text = "長さ3のお題も500回で学習できるかな？";
                }

                yield break;
            }
            StopAllCoroutines();
            StartCoroutine(IE_Quest());
        }

        private void BeginPhasePlan()
        {
            if (phase != Phase.Plan) return;

            // Stage Settings
            if (stageIdx == 0)
            {
                this.episodesTurn = 100;
                this.episodesTurnLeft = this.episodesTurn;
            }
            else if (stageIdx == 1)
            {
                this.episodesTurn = 300;
                this.episodesTurnLeft = this.episodesTurn;
            }
            else if (stageIdx == 2)
            {
                this.episodesTurn = 1000;
                this.episodesTurnLeft = this.episodesTurn;
            }
                btnNext.GetComponentInChildren<Text>().text = "O K";
            ui.transform.Find("TextSteps").GetComponent<Text>().text = $"試行回数  {this.episodesTurnLeft}";

            if (isSt0Failed)
            {
                ui.transform.Find("SliderSteps").gameObject.SetActive(true);
                ui.transform.Find("SliderSteps").GetComponent<Slider>().value = 1;
            }

            uiQuest.ShowP(0);
            uiQuest.HideA();
            uiBoard.ShowKomaP(4, 4);
            uiBoard.HideTrajP();
            agent.Reset();

            IEnumerator IE_Plan()
            {
                if (stageIdx == 0)
                {
                    if (!isSt0Failed)
                    {
                        btnNext.interactable = true;
                        isWaitButton = true;
                        text.text = "キューブは色もゴールも見えないので、\n最初はてきとうに動くけど、";
                        yield return new WaitUntil(() => !isWaitButton);

                        isWaitButton = true;
                        text.text = "報酬（ほうしゅう）をマスにおいて、\nキューブがたまたまそこに着いたら、\n今の動きは正しいとわかるのだ";
                        yield return new WaitUntil(() => !isWaitButton);

                        btnNext.interactable = false;
                        text.text = "ゴールのマスをクリックして、\n報酬をおいてみてください";
                        yield return new WaitUntil(() => uiBoard.RewardCount > 0);

                        text.text = $"OKボタン押すと、学習が始まるよ\n\nまず試行錯誤を{episodesTurnLeft}回するよ";
                        btnNext.interactable = true;
                    }
                    else
                    {
                        text.text = "100回じゃたりないね。スライダーで試行回数を増やして再学習しよう。";
                        btnNext.interactable = true;
                    }
                }
                else if (stageIdx == 1)
                {
                    btnNext.interactable = false;
                    text.text = "「報酬」を２つ置いてみてください";
                    yield return new WaitUntil(() => uiBoard.RewardCount > 0);
                    btnNext.interactable = true;
                }
                else if (stageIdx == 2)
                {
                    btnNext.interactable = false;
                    text.text = "「報酬」を置いてみてください";
                    yield return new WaitUntil(() => uiBoard.RewardCount == 2);
                    btnNext.interactable = true;
                }
            }
            StopAllCoroutines();
            StartCoroutine(IE_Plan());
        }

        private void BeginPhaseTrain()
        {
            if (phase != Phase.Train) return;

            ui.transform.Find("PhasePlan").gameObject.SetActive(false);

            IEnumerator IE_Train()
            {
                btnNext.interactable = false;
                this.inTraining = true;

                float stepTime = 0.01f;
                // if (stageIdx == 0) stepTime = 0.04f;

                while (this.episodesTurnLeft-- > 0)
                {
                    env.Reset();
                    // UI
                    List<Vector2Int> traj = new List<Vector2Int>();
                    uiBoard.ShowKomaP(env.row, env.col);
                    uiBoard.HideTrajP();
                    uiBoard.ResetRewardGot();
                    uiQuest.ShowP(env.passedSpaceCnt);
                    text.text = $"試行回数： {this.episodesTurn-this.episodesTurnLeft} / {this.episodesTurn}\n" + $"ロス(仮)： {this.loss}";
                    yield return new WaitForSecondsRealtime(stepTime);

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
                        traj.Add(new Vector2Int(row_, col_));
                        uiBoard.ShowKomaP(row_, col_);
                        uiBoard.ShowTrajP(traj.ToArray());
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

                        if (!Env.IsResponseFail(res)){
                            reward += uiBoard.GetReward(row, col, action);
                        }
                        agent.Collect(row, col, (int)action, reward, done, row_, col_);

                        yield return new WaitForSecondsRealtime(stepTime);
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

                int successCnt = 0;
                for (int ieps = 0; ieps < 5; ieps ++)
                {
                    env.Reset();

                    // Control cubes
                    PlayerController.ins.Move2Center();
                    yield return new WaitUntil(() => PlayerController.ins.IsAtCenter);

                    // UI
                    List<Vector2Int> traj = new List<Vector2Int>();
                    uiBoard.ShowKomaP(env.row, env.col);
                    uiBoard.HideTrajP();
                    uiQuest.ShowP(env.passedSpaceCnt);
                    yield return new WaitForSecondsRealtime(0.4f);

                    while (true)
                    {
                        // Step
                        var row = env.row; var col = env.col;
                        var action = agent.GetActionTest(row, col);
                        var res = env.Step(action);
                        bool done = Env.IsResponseFail(res) || res == Env.Response.Goal;

                        // Control cubes
                        PlayerController.ins.RequestMove(env.row, env.col, spd:50);
                        yield return new WaitUntil(() => !PlayerController.ins.isMoving);
                        AudioPlayer.ins.PlaySE(AudioPlayer.ESE.StepConfirmed);

                        // UI
                        traj.Add(new Vector2Int(env.row, env.col));
                        uiBoard.ShowKomaP(env.row, env.col);
                        uiBoard.ShowTrajP(traj.ToArray());
                        uiQuest.ShowP(env.passedSpaceCnt);

                        if (done)
                        {
                            successCnt += res == Env.Response.Goal? 1: 0;
                            text.text += res == Env.Response.Goal? "O" : "X";
                        }

                        yield return new WaitForSecondsRealtime(0.4f);
                        if (done) break;
                    }

                    if (ieps < 4)
                        yield return new WaitForSecondsRealtime(0.5f);
                }

                text.text += $"\n点数 = {successCnt}/5";

                if (successCnt > 2)
                {
                    isFail = false;

                    // Control cubes: Perform
                    PlayerController.ins.PerformHappy();

                    // UI
                    text.text += "\n3点以上なので合格! ";
                    clearedStages = Mathf.Max(clearedStages, stageIdx+1);

                    btnNext.interactable = true;
                    isWaitButton = true;
                    yield return new WaitUntil(() => !isWaitButton);

                    if (stageIdx == 0)
                        text.text = "報酬をゴールにおくことで、\nキューブにゴールへの動きかたを\n学習させることができたね!";
                    else if (stageIdx == 1)
                        text.text = "お題が長くなると、\n学習に必要な試行回数がも多くなったね";
                }
                else
                {
                    isFail = true;

                    // Control cubes: Perform
                    PlayerController.ins.PerformRegret();

                    // UI
                    text.text += "\n3未満なので不合格！";
                    btnNext.interactable = true;
                    isWaitButton = true;
                    yield return new WaitUntil(() => !isWaitButton);

                    if (stageIdx == 0)
                    {
                        if (!isSt0Failed)
                            text.text = "100じゃたりないみたいね。\nリトライしよう";
                        else
                            text.text = "報酬をちゃんとゴールに置いたかな？\nリトライしよう";
                    }
                    else if (stageIdx == 1)
                    {
                        text.text = "報酬の位置を変えてリトライしよう";
                    }
                    btnNext.GetComponentInChildren<Text>().text = "リトライ";
                }

                btnNext.interactable = true;
            }
            StopAllCoroutines();
            StartCoroutine(IE_Test());
        }

        private void BeginPhaseBattle()
        {
            if (phase != Phase.Battle) return;

            text.text = "迷キューとバトル\n\n";
            int successCnt = 0;

            game.StopGame();
            AIController.ins.setting = Config.trainerStageSetting;

            IEnumerator IE_Battle()
            {
                btnNext.interactable = false;

                for (int ieps=0; ieps < 5; ieps++)
                {
                    // Control Cubes
                    AIController.ins.RequestMove(4, 4, 80, 0);
                    PlayerController.ins.RequestMove(4, 4, 80, 0);
                    yield return new WaitUntil(() => AIController.ins.IsAtCenter);
                    yield return new WaitUntil(() => PlayerController.ins.IsAtCenter);

                    // UI
                    // List<Vector2Int> traj = new List<Vector2Int>();
                    uiBoard.ShowKomaP(4, 4);
                    uiBoard.HideTrajP();
                    uiQuest.ShowP(0);

                    // Start Game
                    game.InitGame(env.quest);
                    game.StartGame(0);
                    isGameOverP = false;

                    // Control loop for player cube
                    while (true)
                    {
                        // Get action with trained agent
                        var row = game.envP.row; var col = game.envP.col;
                        var action = agent.GetActionTest(row, col);

                        // Delay
                        yield return new WaitForSecondsRealtime(0.5f);

                        if (game.stateP == Game.PlayerState.InGame)
                        {
                            // Move cube
                            (var tarRow, var tarCol) = Env.Translate(action, row, col);
                            PlayerController.ins.RequestMove(tarRow, tarCol, spd:80);  // TODO speed depends on probs

                            // Wait step complete or game over
                            isGameStepP = false;
                            yield return new WaitUntil(() => isGameStepP || isGameOverP);
                        }

                        if (game.stateP == Game.PlayerState.InGame)
                            continue;
                        if (game.stateP == Game.PlayerState.Fail)
                            yield return new WaitUntil(() => game.stateP == Game.PlayerState.LoseFail);
                        break;
                    }

                    // Count
                    if (game.stateP == Game.PlayerState.Win)
                    {
                        text.text += "O";
                        successCnt += 1;
                    }
                    else if (game.stateP == Game.PlayerState.Draw)
                    {
                        text.text += "-";
                    }
                    else
                    {
                        text.text += 'X';
                    }
                    yield return new WaitForSecondsRealtime(2);
                }

                if (successCnt > 2)
                {
                    isFail = false;
                    text.text += "\nキミの勝ち！";
                }
                else
                {
                    isFail = true;
                    text.text += "\nボクの勝ち！";
                    btnNext.GetComponentInChildren<Text>().text = "リトライ";
                }

                btnNext.interactable = true;
            }
            StopAllCoroutines();
            StartCoroutine(IE_Battle());
        }

        private float EpsilonScheduler(int epsLeft, int nEps)
        {
            return (float)epsLeft/nEps * 0.4f + 0.3f;
        }


        #region ========== Game Callbacks ==========

        private bool isGameStepP = false;
        private void OnGameStepP(Env.Response res)
        {
            isGameStepP = true;
        }
        private bool isGameOverP = false;
        private void OnGameOverP(Game.PlayerState state)
        {
            isGameOverP = true;
            if (state == Game.PlayerState.Win)
            {
                PlayerController.ins.PerformHappy();
                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Win);
            }
            else if (state == Game.PlayerState.Fail)
            {
                PlayerController.ins.PerformRegret();
                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Wrong);
            }
            else if (state == Game.PlayerState.LoseFail || state == Game.PlayerState.LoseNotFail || state == Game.PlayerState.Draw)
            {
                PlayerController.ins.PerformSad();
                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Lose);
            }
        }

        #endregion


        #region ========== Callbacks ==========

        private void OnSpaceClicked(Vector2Int rowCol, UIBoard.RewardPositionType type)
        {
            if (phase != Phase.Plan) return;

            if (stageIdx == 0)
            {
                uiBoard.PutReward(rowCol.x, rowCol.y, type, 1);
            }
            else if (stageIdx == 1)
            {
                uiBoard.PutReward(rowCol.x, rowCol.y, type, 2);
            }
            else if (stageIdx == 2)
            {
                uiBoard.PutReward(rowCol.x, rowCol.y, type, 8);
            }
        }

        public void OnBtnNext()
        {
            if (isWaitButton)
            {
                isWaitButton = false;
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
                if (stageIdx == 0 || stageIdx == 1)
                {
                    phase = Phase.Test;
                    BeginPhaseTest();
                }
                else if (stageIdx == 2)
                {
                    phase = Phase.Battle;
                    BeginPhaseBattle();
                }
            }
            else if (phase == Phase.Test)
            {
                if (stageIdx == 0 && !isSt0Failed)
                {
                    isSt0Failed = true;
                    phase = Phase.Plan;
                    BeginPhasePlan();
                    return;
                }
                else if (isFail)
                {
                    phase = Phase.Plan;
                    BeginPhasePlan();
                }
                else
                {
                    phase = Phase.Entry;
                    BeginPhaseEntry();
                }
            }
            else if (phase == Phase.Battle)
            {
                if (isFail)
                {
                    phase = Phase.Plan;
                    BeginPhasePlan();
                }
                else
                {
                    phase = Phase.Entry;
                    BeginPhaseEntry();
                }
            }
        }

        public void OnBtnEntry(int idx)
        {
            if (phase != Phase.Entry) return;
            this.phase = Phase.Quest;
            this.stageIdx = idx;
            BeginPhaseQuest();
        }

        public void OnSliderSteps()
        {
            int v = (int)ui.transform.Find("SliderSteps").GetComponent<Slider>().value * 100;
            ui.transform.Find("TextSteps").GetComponent<Text>().text = $"試行回数  {v}";
            this.episodesTurnLeft = v;
            this.episodesTurn = v;
        }
        #endregion


        private bool isWaitButton = false;
        private IEnumerator WaitButton()
        {
            isWaitButton = true;
            yield return new WaitUntil(() => !isWaitButton);
        }

        internal enum Phase {
            Entry, Quest, Plan, Train, Test, Battle
        }

    }


    internal class QAgent
    {
        public float[,,] Q;
        public float e = 0.8f;
        public float lr = 0.25f;
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
    }

}
