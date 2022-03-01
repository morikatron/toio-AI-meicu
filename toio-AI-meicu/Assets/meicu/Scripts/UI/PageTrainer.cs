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
        private int stageIdx = 0;
        private bool isSt0Failed = false;
        private bool isFail = false;
        private bool isRetry = false;

        private int episodesTurn;
        private int episodesTurnLeft;
        private float loss;


        void OnEnable()
        {
            uiBoard.onSpaceClicked += OnSpaceClicked;
            agent = agent = new QAgent();
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

                game.stepCallbackP += OnGameStepP;
                game.overCallbackP += OnGameOverP;

                PlayerController.ins.targetMatCoordBias = new Vector2Int(0, 0);

                BeginPhaseEntry();
            }
            else
            {
                game.StopGame();
                game.stepCallbackP -= OnGameStepP;
                game.overCallbackP -= OnGameOverP;

                PlayerController.ins.targetMatCoordBias = new Vector2Int(10, -10);

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

            isRetry = false;

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
                if (Prefs.trainerStage == 0)
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
                    yield return WaitButton();
                    text.text += "まずは最初、「ごほうびを使ってAIを育てよう」を選択してみよう！";
                }
                else if (Prefs.trainerStage == 1)
                {
                    text.text = "「長い問題にチャレンジしてみよう」が選べるようになったよ！";
                    yield return WaitButton();
                    text.text += "どこに「ごほうび」を置くのか、何回学習するのか、よく考えて挑戦してみよう！";
                }
                else if (Prefs.trainerStage == 2)
                {
                    text.text = "いよいよボクと対戦だよ！";
                    yield return WaitButton();
                    text.text = "まずキミがAIを学習させてから、ボクと勝負するよ！";
                }

                entryBtn0.interactable = true;
                entryBtn1.interactable = Prefs.trainerStage > 0;
                entryBtn2.interactable = Prefs.trainerStage > 1;
                btnNext.gameObject.SetActive(false);

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

            // Generate Quest by Stage Settings
            int questSize = 2;
            if (stageIdx == 0)
                questSize = 2;
            else if (stageIdx == 1)
                questSize = 3;
            else if (stageIdx == 2)
                questSize = 4;
            Quest quest = env.GenerateQuest(questSize);
            env.SetQuest(quest);

            // Update UI
            textCaption.text = "AIを育てよう";
            btnNext.gameObject.SetActive(true);
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
                    text.text = "そうやって繰り返して覚えていくのが「学習する」ということになるんだ。どう？分かったかな？";
                }
                else if (stageIdx == 1)
                {
                    text.text = "よくきたね！\nここでは「長い問題」に挑戦するよ！";
                    yield return WaitButton();
                    text.text = "問題が長くなれば…さっきよりもたくさんの学習回数が必要になるんだ。";
                    yield return WaitButton();
                    text.text = "この問題を見て！さっきよりも長くなっているでしょ？さっきの問題をクリアするには、400回学習必要だったけど…";
                    yield return WaitButton();
                    text.text = "この問題の場合（通過する色のマスが3つある場合）、だいたい1000回は学習しないと、うまくゴールまでの道を覚えられないんだ…";
                    yield return WaitButton();
                    text.text = "つまり「学習にかかる時間」がとっても長くなっちゃうんだ…";
                    yield return WaitButton();
                    text.text = "それって、とても面倒だよね。";

                    yield return WaitButton();
                    text.text = "そこで！必殺技！";
                    yield return WaitButton();
                    text.text = "「ごほうび」を<color=red>2個</color>使ってみるよ！";
                    yield return WaitButton();
                    text.text = "「ごほうび」を2個使うことで、400回まで回数をへらすことができるんだ！";
                }
                else if (stageIdx == 2)
                {
                    text.text = "いよいよボクとバトルだ！";
                    yield return WaitButton();
                    text.text = "ここでは、「ごほうび」の置き方、「学習」の回数はキミ次第！";
                    yield return WaitButton();
                    text.text = "自由に学習させて、学習が終わったら「バトル」ボタンを押してみてね！";
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
                this.episodesTurn = 50;
                this.episodesTurnLeft = this.episodesTurn;
            }
            else if (stageIdx == 1)
            {
                this.episodesTurn = 400;
                this.episodesTurnLeft = this.episodesTurn;
            }
            else if (stageIdx == 2)
            {
                this.episodesTurn = 100;
                this.episodesTurnLeft = this.episodesTurn;
            }

            // Update UI
            btnNext.interactable = true;
            btnNext.GetComponentInChildren<Text>().text = "O K";
            ui.transform.Find("TextSteps").gameObject.SetActive(true);
            ui.transform.Find("TextSteps").GetComponent<Text>().text = $"試行回数  {this.episodesTurn}";

            if (stageIdx == 0 && isSt0Failed || stageIdx == 2)
            {
                ui.transform.Find("PhasePlan").gameObject.SetActive(true);
                ui.transform.Find("PhasePlan").Find("SliderSteps").GetComponent<Slider>().value = episodesTurn/100;
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
                        text.text = "じゃあ、今度は実際にやってみよう！";
                        yield return WaitButton();
                        text.text = "今回のゴールはスタートのすぐそばだから「ごほうび」もゴールに置けばよさそうだね！";
                        yield return WaitButton();

                        text.text = "マウスでゴール（はたが立っているマス）をクリックしてみてね。";
                        btnNext.interactable = false;
                        yield return new WaitUntil(() => uiBoard.RewardCount > 0);
                        btnNext.interactable = true;

                        text.text = $"よし！それでは学習開始！{episodesTurn}回試行錯誤（しこうさくご）してみるよ！";
                    }
                    else
                    {
                        text.text = "スライダーで試行回数を増やして再学習しよう。";
                        btnNext.interactable = true;
                    }
                }
                else if (stageIdx == 1)
                {
                    btnNext.interactable = false;

                    if (!isRetry)
                    {
                        text.text = "まず、たどり着きたいゴールに「ごほうび」を1つ置いてみて。";
                        yield return new WaitUntil(() => uiBoard.RewardCount > 0);
                        text.text = "オッケー！つぎはどこに置くのがいいかな？キミが考えて置いてみて！";
                        yield return new WaitUntil(() => uiBoard.RewardCount > 1);
                    }
                    else
                    {
                        text.text = "「ごほうび」の場所を決めてね。";
                        yield return new WaitUntil(() => uiBoard.RewardCount > 0);
                    }
                    btnNext.interactable = true;

                    text.text = "よし！今回も「学習回数」は400回でやってみるよ！\n\n準備はいいかな？";
                }
                else if (stageIdx == 2)
                {
                    btnNext.interactable = false;
                    text.text = "準備はいいかな？";
                    yield return new WaitUntil(() => uiBoard.RewardCount > 0);
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

                float stepTime = 0.01f;

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

                btnNext.interactable = true;
                if (stageIdx == 0 && isSt0Failed)
                {
                    text.text = "かんりょうー！";
                    yield return WaitButton();
                    text.text = "やっぱり、学習回数が多い分、ちょっと時間がかかったね…";
                }
                else
                {
                    text.text = "かんりょうー！";
                }
            }
            StopAllCoroutines();
            StartCoroutine(IE_Train());
        }

        private void BeginPhaseTest()
        {
            if (phase != Phase.Test) return;

            // Update UI
            uiQuest.ShowP(0);
            uiBoard.HideTrajP();
            uiBoard.ShowKomaP(4, 4);


            IEnumerator IE_Test()
            {
                btnNext.interactable = true;

                text.text = "それでは、どれくらい学習できたか、テストで試してみよう！";
                yield return WaitButton();

                if (stageIdx == 0 && !isSt0Failed)
                {
                    text.text = "テストは、「5回のうち何回ゴールにたどりつけるか」で判定するよ。";
                    yield return WaitButton();
                    text.text = "3回以上が合格だよ！";
                    yield return WaitButton();
                    text.text = "テストは、マットの上で実際にキューブを動かして行うよ。";
                    yield return WaitButton();
                }

                btnNext.interactable = false;

                int successCnt = 0;
                string textLog = "";
                for (int ieps = 0; ieps < 5; ieps ++)
                {
                    env.Reset();

                    // Control cubes
                    PlayerController.ins.RequestMove(4, 4, 80, 0);
                    yield return new WaitUntil(() => PlayerController.ins.IsAtCenter);

                    // UI
                    List<Vector2Int> traj = new List<Vector2Int>();
                    uiBoard.ShowKomaP(env.row, env.col);
                    uiBoard.HideTrajP();
                    uiQuest.ShowP(env.passedSpaceCnt);
                    text.text = $"試験 {ieps+1} 回目\n" + textLog;
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
                            textLog += res == Env.Response.Goal? "O" : "X";
                            text.text = $"試験 {ieps+1} 回目\n" + textLog;
                        }

                        yield return new WaitForSecondsRealtime(0.4f);
                        if (done) break;
                    }

                    if (ieps < 4)
                        yield return new WaitForSecondsRealtime(0.5f);
                }

                if (successCnt > 2)
                {
                    isFail = false;
                    Prefs.trainerStage = Mathf.Max(Prefs.trainerStage, stageIdx+1);

                    // Control cubes: Perform
                    PlayerController.ins.PerformHappy();

                    // UI
                    btnNext.interactable = true;

                    text.text += $"\n{successCnt}回たどり着けたので…合格ー！";
                    yield return WaitButton();
                    text.text = "おめでとう！キミのAIも正しい道を学習できてきたね！";

                    // UI conclusion
                    if (stageIdx == 0)
                    {
                        if (isSt0Failed)
                        {
                            yield return WaitButton();
                            text.text = $"「50回」だとうまくいかなかったけど、「{episodesTurn}回」にしたらうまくいったね！";
                            yield return WaitButton();
                            text.text = $"つまり、問題の長さ（難しさ）によって、必要な「学習の量」が変わるんだ。";
                            yield return WaitButton();
                            text.text = $"キミだって、簡単な問題はすぐわかるけど、難しい問題はいっぱい考えなきゃいけないでしょ？";
                            yield return WaitButton();
                            text.text = $"つぎはもっと問題が長くなるよ！\nぜひ次の課題にも挑戦してみてね！";
                        }
                    }
                    else if (stageIdx == 1)
                    {
                        yield return WaitButton();
                        text.text = "うまく学習できたね！";
                        yield return WaitButton();
                        text.text = "AIを育てるには、「ごほうび」をあげたり、「学習回数」を変えたり、うまく育つようにいろんな事を考えて試す必要があるんだ。";
                        yield return WaitButton();
                        text.text = "問題が難しくなればなるほど、育てるのも難しくなったり、時間が掛かったりする…";
                        yield return WaitButton();
                        text.text = "だから「はやく・うまくAIを育てる」ためには、色んな工夫が必要なんだ。";
                        yield return WaitButton();
                        text.text = "今は「ごほうびをあげる」「回数を変えてみる」だけだったけど、本当は失敗したら怒ったり（マイナスのほうしゅうと言います）など...";
                        yield return WaitButton();
                        text.text = "AIの学習には色々な調節するもの（パラメータと言います）があるんだ。これは難しいので、またの機会に説明するね！";
                        yield return WaitButton();
                        text.text = "次は、いよいよキミが育てたAIと、ボクが対決するよ！";
                        yield return WaitButton();
                        text.text = "キミのAIと勝負だ！";
                    }
                }
                // Fail
                else
                {
                    isFail = true;

                    // Control cubes: Perform
                    PlayerController.ins.PerformRegret();

                    // UI
                    text.text += $"\n\n{successCnt}回しかたどり着けなかったね…残念…";
                    btnNext.interactable = true;
                    isWaitButton = true;
                    yield return new WaitUntil(() => !isWaitButton);

                    if (stageIdx == 0)
                    {
                        if (!isSt0Failed)
                        {
                            text.text = $"「{episodesTurn}回」じゃ学習回数が足りなかったみたい。";
                            yield return WaitButton();
                            text.text = "今度は、回数を増やしてやってみよう！";
                        }
                        else
                            text.text = "報酬をちゃんとゴールに置いたかな？\nリトライしよう";
                    }
                    else if (stageIdx == 1)
                    {
                        text.text = "「ごほうび」の場所が悪かったみたい。\n「ごほうび」の場所を変えて、もう一度やってみよう！";
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
                btnNext.interactable = true;
                text.text = "それでは、ボクとバトルしよう！";
                yield return WaitButton();
                text.text = "どちらが先に、間違えずにゴールにたどりつけるかな？";
                yield return WaitButton();
                text.text = "バトルも、マットの上で実際にキューブを動かして行うよ。";
                yield return WaitButton();
                text.text = "バトルスタート！";

                btnNext.interactable = false;

                // for (int ieps=0; ieps < 5; ieps++)
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
                        // text.text += "O";
                        successCnt += 1;
                    }
                    else if (game.stateP == Game.PlayerState.Draw)
                    {
                        // text.text += "-";
                    }
                    else
                    {
                        // text.text += 'X';
                    }
                    yield return new WaitForSecondsRealtime(2);
                }

                btnNext.interactable = true;
                if (successCnt > 0)
                {
                    isFail = false;
                    text.text = "おめでとう！キミの勝ちだ！";
                    yield return WaitButton();
                    text.text = "同じ問題でも、「ごほうび」や「学習回数」を変えると結果が変わるよ！";
                    yield return WaitButton();
                    text.text = "できるだけ少ない回数（早い時間）で、ボクに勝てるように工夫してみてね！";
                    // yield return WaitButton();
                    // text.text = "次の問題に挑戦する？";
                }
                else
                {
                    isFail = true;
                    text.text = "ざんねん！ボクの勝ちー！";
                    yield return WaitButton();
                    text.text = "もう一度学習をやり直してみる？";
                    btnNext.GetComponentInChildren<Text>().text = "リトライ";
                }
            }
            StopAllCoroutines();
            StartCoroutine(IE_Battle());
        }

        private float EpsilonScheduler(int epsLeft, int nEps)
        {
            return (float)epsLeft/nEps * 0.4f + 0.2f;
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


        #region ========== UI Callbacks ==========

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
                uiBoard.PutReward(rowCol.x, rowCol.y, type, 4);
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
                    isRetry = true;
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

        public void OnBtnHome()
        {
            if (phase == Phase.Entry)
                PageManager.OnBtnHome();
            else
            {
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

        public void OnSliderSteps()
        {
            int v = (int)ui.transform.Find("PhasePlan").Find("SliderSteps").GetComponent<Slider>().value * 100;
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
    }

}
