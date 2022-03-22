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
        public Game game;

        public GameObject ui;
        public UIMeicu meicu;
        public UIBoard uiBoard;
        public UIQuest uiQuest;
        public Transform uiResult;
        public Transform uiIndicators;
        public Text textCaption;
        public TMP_Text text;

        [Header("UI Phases")]
        public Transform uiPhaseEntry;
        public Transform uiPhaseIntro;
        public Transform uiPhasePlan;
        public Transform uiPhaseTrain;
        public Transform uiPhaseTest;
        public Transform uiPhaseBattle;

        [Header("UI Buttons")]
        public Button btnNext;
        public Button btnBack;
        public Button btnOK;
        public Button btnRetry;
        public Button btnNew;
        public UISwitch btnBGM;
        public Button btnEntry0;
        public Button btnEntry1;
        public Button btnEntry2;

        private Env env = new Env();
        private QAgent agent;
        private Phase phase;
        private int uiIdx = 0;
        private int stageIdx = 0;
        private bool isSt0Failed = false;
        private bool isRetry = false;

        private int episodesTurn;
        private int episodesTurnLeft;
        private int maxRewards = 1;
        private float loss;



        void OnEnable()
        {
            uiBoard.onSpaceClicked += OnSpaceClicked;
            agent = agent = new QAgent();
        }

        internal void SetActive(bool active)
        {
            if (ui.activeSelf == active) return;
            uiResult.gameObject.SetActive(false);
            ui.SetActive(active);

            phase = Phase.Entry;
            uiIdx = 0;

            if (active)
            {
                Prefs.SetTrainerAccessed();
                btnBGM.isOn = AudioPlayer.ins.isBGMOn;

                env.Reset();
                uiBoard.Reset();
                uiQuest.Reset();
                uiQuest.HideA();

                game.startCallback += OnGameStarted;
                game.stepCallbackP += OnGameStepP;
                game.stepCallbackA += OnGameStepA;
                game.overCallbackP += OnGameOverP;

                PlayerController.ins.targetMatCoordBias = new Vector2Int(0, 0);

                AIController.ins.thinkCallback += OnAIThink;

                phase = Phase.Summary;
                SetPhaseAndUpdateUI(Phase.Entry, 0);

                ClearIEText();
            }
            else
            {
                game.StopGame();
                game.startCallback -= OnGameStarted;
                game.stepCallbackP -= OnGameStepP;
                game.stepCallbackA -= OnGameStepA;
                game.overCallbackP -= OnGameOverP;

                PlayerController.ins.targetMatCoordBias = new Vector2Int(10, -10);

                AIController.ins.thinkCallback -= OnAIThink;
                AIController.ins.StopMotion();
                AIController.ins.StopAllCoroutines();
                PlayerController.ins.Stop();

                ClearIEText();
                StopAllCoroutines();
            }

        }

        internal void Pause()
        {

        }

        internal void SetPhaseAndUpdateUI(Phase phase, int idx)
        {
            var prevPhase = this.phase;
            var prevIdx = this.uiIdx;
            this.phase = phase;
            this.uiIdx = idx;
            UpdateUI(prevPhase, prevIdx);
        }
        internal void UpdateUI(Phase prevPhase, int prevIdx)
        {
            IEnumerator ie()
            {
                btnBack.interactable = uiIdx > 0;

                int cnt = 0;
                if (phase == Phase.Entry)
                {
                    if (prevPhase != phase) OnEnterEntry();

                    if (Prefs.trainerStage == 0)
                    {
                        if (uiIdx == cnt++)
                        {
                            text.text = "こんにちは！\nここでは「キミだけのAI」を育ててみる事ができるよ！";
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "「AIを育てる」には、キミと同じようにAIも「学習」する必要があるんだ。";
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "え？\n「学習」ってどのようにするのかって？";
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "任せて！ボクがこれからひとつひとつ説明していくよ！";
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "キミだけのAIを育てて、ボクと勝負だ！";
                            btnNext.interactable = true;
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text += "まずは最初、「ごほうびを使ってAIを育てよう」を選択してみよう！";
                            btnEntry0.interactable = true;
                            btnNext.interactable = false;
                            yield break;
                        }
                        Debug.LogWarning("Invalid uiIdx");
                    }
                    else if (Prefs.trainerStage == 1)
                    {
                        if (uiIdx == cnt++)
                        {
                            text.text = "「長い問題にチャレンジしてみよう」が選べるようになったよ！";
                            btnNext.interactable = true;
                            btnEntry0.interactable = true;
                            btnEntry1.interactable = true;
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "どこに「ごほうび」を置くのか、何回学習するのか、よく考えて挑戦してみよう！";
                            btnNext.interactable = false;
                            yield break;
                        }
                        Debug.LogWarning("Invalid uiIdx");
                    }
                    else if (Prefs.trainerStage == 2)
                    {
                        if (uiIdx == cnt++)
                        {
                            text.text = "いよいよボクと対戦だよ！";
                            btnNext.interactable = true;
                            btnEntry0.interactable = true;
                            btnEntry1.interactable = true;
                            btnEntry2.interactable = true;
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "まずキミがAIを学習させてから、ボクと勝負するよ！";
                            btnNext.interactable = false;
                            yield break;
                        }
                        Debug.LogWarning("Invalid uiIdx");
                    }
                }
                else if (phase == Phase.Intro)
                {
                    if (prevPhase != phase) OnEnterIntro();

                    if (stageIdx == 0)
                    {
                        if (uiIdx == cnt++)
                        {
                            text.text = "ようこそ！\nここでは、「ごほうび」を使ってAIを育ててみるよ！";
                            uiIndicators.Find("Quest").gameObject.SetActive(false);
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "「バトル」と同じように、スタートとゴール、そして「どの色を通過するか」という問題が出されているね。";
                            uiIndicators.Find("Quest").gameObject.SetActive(true);
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "キミはスグに「こう進めばいい」と分かったと思うけど、AIは最初は色も場所も分かってないから、「どっちに進めば正解なのか」も全く分からないんだったよね。";
                            uiIndicators.Find("Quest").gameObject.SetActive(false);
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "そんなAIにどうやって「学習」させるのか、それは……。";
                            uiIndicators.Find("Reward").gameObject.SetActive(false);
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "<color=#b00>「ごほうび」</color>を使うんだよ！";
                            uiIndicators.Find("Reward").gameObject.SetActive(true);
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "道に「ごほうび」をおいておくと、AIはごほうびをたどって「こっちが正しい道だぞ！」とおぼえていくんだ。";
                            uiIndicators.Find("Reward").gameObject.SetActive(false);
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "そうやって繰り返して覚えていくのが「学習する」ということになるんだ。\nどう？分かったかな？";
                            yield break;
                        }
                        Debug.LogWarning("Invalid uiIdx");
                    }
                    else if (stageIdx == 1)
                    {
                        if (uiIdx == cnt++)
                        {
                            text.text = "よくきたね！\nここでは「長い問題」に挑戦するよ！";
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "問題が長くなれば…さっきよりもたくさんの試行回数が必要になるんだ。";
                            uiIndicators.Find("Quest2").gameObject.SetActive(false);
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "この問題を見て！さっきよりも長くなっているでしょ？さっきの問題をクリアするには、400回学習必要だったけど…";
                            uiIndicators.Find("Quest2").gameObject.SetActive(true);
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "この問題の場合（通過する色のマスが3つある場合）、だいたい1000回は学習しないと、うまくゴールまでの道を覚えられないんだ…";
                            uiIndicators.Find("Quest2").gameObject.SetActive(true);
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "つまり「学習にかかる時間」がとっても長くなっちゃうんだ…";
                            uiIndicators.Find("Quest2").gameObject.SetActive(false);
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "それって、とても面倒だよね。";
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "そこで！必殺技！";
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "「ごほうび」を<color=red>2個</color>使ってみるよ！";
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "「ごほうび」を2個使うことで、400回まで回数をへらすことができるんだ！";
                            yield break;
                        }
                        Debug.LogWarning("Invalid uiIdx");
                    }
                    else if (stageIdx == 2)
                    {
                        if (uiIdx == cnt++)
                        {
                            text.text = "いよいよボクとバトルだ！";
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "ここでは、「ごほうび」の置き方、\n「学習」の回数はキミ次第！";
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "自由に学習させて、学習が終わったら「バトル」ボタンを押してみてね！";
                            yield break;
                        }
                        Debug.LogWarning("Invalid uiIdx");
                    }
                }
                else if (phase == Phase.Plan)
                {
                    if (prevPhase != phase) OnEnterPlan();

                    if (stageIdx == 0)
                    {
                        if (!isSt0Failed)
                        {
                            if (uiIdx == cnt++)
                            {
                                text.text = "じゃあ、今度は実際にやってみよう！";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = "今回のゴールはスタートのすぐそばだから「ごほうび」もゴールに置けばよさそうだね！";
                                btnNext.interactable = true;
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                while (true)
                                {
                                    if (uiBoard.RewardCount == 1 && uiBoard.HasReward(env.quest.goalRow, env.quest.goalCol, 0))
                                    {
                                        text.text = $"よし！それでは学習開始！\n{episodesTurn}回試行錯誤してみるよ！";
                                        btnNext.interactable = true;
                                    }
                                    else
                                    {
                                        text.text = "マウスでゴール（はたが立っているマス）をクリックしてみてね。\n間違えたらごほうびマークをクリックすると直せるよ";
                                        btnNext.interactable = false;
                                    }
                                    yield return new WaitForSecondsRealtime(0.3f);
                                }
                            }
                            Debug.LogWarning("Invalid uiIdx");
                        }
                        else
                        {
                            text.text = "上の白いボタンを右に動かすと試行回数を変えられるよ。回数を増やしてから、もう一回学習させよう。";
                            uiIndicators.Find("Slider").gameObject.SetActive(true);
                            btnNext.interactable = true;
                        }
                    }
                    else if (stageIdx == 1)
                    {
                        if (!isRetry)
                        {
                            if (uiIdx == cnt++)
                            {
                                while (true)
                                {
                                    if (uiBoard.RewardCount == 1 && uiBoard.HasReward(env.quest.goalRow, env.quest.goalCol, 0))
                                    {
                                        text.text = $"ここまでくればごほうびがもらえる、ってAIに覚えさせるんだ。";
                                        btnNext.interactable = true;
                                    }
                                    else
                                    {
                                        text.text = $"まず、前の問題と同じように、最終的にたどり着きたいゴールに「ごほうび」を置いてみよう。";
                                        btnNext.interactable = false;
                                    }
                                    yield return new WaitForSecondsRealtime(0.3f);
                                }
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = $"ゴールがすぐ横にある場合は、何も学習しなくても1/4のかくりつでゴールにたどり着けるよね。";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = $"でも、ゴールが遠くなればなるほど、そこまでぐうぜんたどり着ける確率は減るんだ。";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = $"これが「問題が長くなると、学習に時間がかかる」ことの理由。";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = $"ゴール（ごほうび）にたどり着くまで、何度も何度も試さなきゃならないんだ。";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = $"でも、これってとっても時間がかかっちゃうよね…";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = $"「長い1つの問題」を「短い2つの問題」に分けるように考えて...";
                                btnNext.interactable = true;
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                while (true)
                                {
                                    if (uiBoard.RewardCount == 2 && uiBoard.HasReward(env.quest.goalRow, env.quest.goalCol, 0))
                                    {
                                        text.text = "よし！今回も「試行回数」は400回でやってみるよ！";
                                        btnNext.interactable = true;
                                    }
                                    else
                                    {
                                        text.text = $"ゴールまでの間にもう一つ「ごほうび」を置いてみよう！";
                                        btnNext.interactable = false;
                                    }
                                    yield return new WaitForSecondsRealtime(0.3f);
                                }
                            }
                            Debug.LogWarning("Invalid uiIdx");
                        }
                        else
                        {
                            if (uiIdx == cnt++)
                            {
                                var lastRewards = uiBoard.rewardList;
                                while (true)
                                {
                                    if (uiBoard.RewardCount == 0)
                                    {
                                        text.text = $"「ごほうび」の場所を決めてね。\nここでは最大{this.maxRewards}個まで置けるよ";
                                        btnNext.interactable = false;
                                    }
                                    else if (uiBoard.rewardList.Count == lastRewards.Count && lastRewards.TrueForAll(r=>uiBoard.rewardList.Contains(r)))
                                    {
                                        text.text = $"ちがう「ごほうび」の置き方に変えてみよう";
                                        btnNext.interactable = false;
                                    }
                                    else
                                    {
                                        text.text = "よし！今回も「試行回数」は400回でやってみるよ！";
                                        btnNext.interactable = true;
                                    }
                                    yield return new WaitForSecondsRealtime(0.3f);
                                }
                            }
                            Debug.LogWarning("Invalid uiIdx");
                        }
                    }
                    else if (stageIdx == 2)
                    {
                        var lastRewards = uiBoard.rewardList;
                        while (true)
                        {
                            if (uiBoard.RewardCount == 0)
                            {
                                text.text = $"「ごほうび」の場所を決めてね。\nここでは最大{this.maxRewards}個まで置けるよ";
                                btnNext.interactable = false;
                            }
                            else if (isRetry && uiBoard.rewardList.Count == lastRewards.Count && lastRewards.TrueForAll(r=>uiBoard.rewardList.Contains(r)))
                            {
                                text.text = $"「ちがう「ごほうび」の置き方に変えてみよう";
                                btnNext.interactable = false;
                            }
                            else
                            {
                                text.text = "よし！それでは学習開始！";
                                btnNext.interactable = true;
                            }
                            yield return new WaitForSecondsRealtime(0.3f);
                        }
                    }
                }
                else if (phase == Phase.Train)
                {
                    if (prevPhase != phase) OnEnterTrain();

                    if (uiIdx == cnt++)
                    {
                        btnNext.interactable = false;
                        yield return IE_Train();
                        btnNext.interactable = true;
                        yield break;
                    }
                    Debug.LogWarning("Invalid uiIdx");
                }
                else if (phase == Phase.Test)
                {
                    if (prevPhase != phase) OnEnterTest();

                    if (stageIdx == 0 && !isSt0Failed)
                    {
                        if (uiIdx == cnt++)
                        {
                            text.text = "それでは、どれくらい学習できたのか、\n実際にキューブを動かしながら\n実験してみよう！";
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "これから5回実験するから\n3回以上、ゴールに着いたら合格だよ！";
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "実験は、マットの上で実際にキューブを動かして行うよ。";
                            yield break;
                        }
                    }
                    if (uiIdx == cnt++)
                    {
                        btnBack.interactable = false;
                        btnNext.interactable = false;
                        yield return IE_Test();
                        btnNext.interactable = true;
                        yield break;
                    }
                    Debug.LogWarning("Invalid uiIdx");
                }
                else if (phase == Phase.Battle)
                {
                    if (prevPhase != phase) OnEnterBattle();
                    if (uiIdx == cnt++)
                    {
                        text.text = "それでは、ボクとバトルしよう！";
                        yield break;
                    }
                    if (uiIdx == cnt++)
                    {
                        text.text = "どちらが先に、間違えずにゴールにたどりつけるかな？";
                        yield break;
                    }
                    if (uiIdx == cnt++)
                    {
                        text.text = "バトルも、マットの上で実際にキューブを動かして行うよ。";
                        yield break;
                    }
                    if (uiIdx == cnt++)
                    {
                        btnBack.interactable = false;
                        btnNext.interactable = false;
                        yield return IE_Battle();
                        btnNext.interactable = true;

                        if (game.stateP == Game.PlayerState.Win)
                            text.text = "おめでとう！キミの勝ちだ！";
                        else if (game.stateP == Game.PlayerState.Draw)
                            text.text = "ざんねん！ひきわけだね！";
                        else
                            text.text = "ざんねん！ボクの勝ちだ！";
                        yield break;
                    }
                    Debug.LogWarning("Invalid uiIdx");
                }
                else if (phase == Phase.Summary)
                {
                    if (prevPhase != phase) OnEnterSummary();
                    if (stageIdx == 0)
                    {
                        if (!isSt0Failed && !isTestPassed)
                        {
                            if (uiIdx == cnt++)
                            {
                                text.text = $"「{episodesTurn}回」じゃ試行回数が足りなかったみたい。";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = $"今度は、回数を増やしてやってみよう！";
                                btnNext.gameObject.SetActive(true);
                                btnBack.gameObject.SetActive(false);
                                yield break;
                            }
                            Debug.LogWarning("Invalid uiIdx");
                        }
                        else if (!isSt0Failed && isTestPassed)
                        {
                            if (uiIdx == cnt++)
                            {
                                text.text = $"「なんとぐうぜんに学習できた！";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = $"けど回数を増やすとあんていして学習できるので試してみよう！";
                                btnNext.gameObject.SetActive(true);
                                btnBack.gameObject.SetActive(false);
                                yield break;
                            }
                            Debug.LogWarning("Invalid uiIdx");
                        }
                        else if (isSt0Failed && isTestPassed)
                        {
                            if (uiIdx == cnt++)
                            {
                                text.text = $"最初の「50回」だとうまくいかなかったけど、「{episodesTurn}回」にしたらうまくいったね！";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = $"つまり、問題の長さ（難しさ）によって、必要な「学習の量」が変わるんだ。";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = $"キミだって、簡単な問題はすぐわかるけど、難しい問題はいっぱい考えなきゃいけないでしょ？";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = $"つぎはもっと問題が長くなるよ！\nぜひ次の課題にも挑戦してみてね！";
                                btnNext.gameObject.SetActive(false);
                                btnBack.gameObject.SetActive(false);
                                btnOK.gameObject.SetActive(true);
                                btnRetry.gameObject.SetActive(true);
                                yield break;
                            }
                            Debug.LogWarning("Invalid uiIdx");
                        }
                        else if (isSt0Failed && !isTestPassed)
                        {
                            if (uiIdx == cnt++)
                            {
                                text.text = "報酬をちゃんとゴールに置いたかな？\nリトライしよう";
                                btnNext.gameObject.SetActive(false);
                                btnBack.gameObject.SetActive(false);
                                btnRetry.gameObject.SetActive(true);
                                yield break;
                            }
                            Debug.LogWarning("Invalid uiIdx");
                        }
                    }
                    else if (stageIdx == 1)
                    {
                        if (isTestPassed)
                        {
                            if (uiIdx == cnt++)
                            {
                                text.text = "AIを育てるには、「ごほうび」をあげたり、「試行回数」を変えたり、うまく育つようにいろんな事を考えて試す必要があるんだ。";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = "問題が難しくなればなるほど、育てるのも難しくなったり、時間がかかったりする…";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = "だから「はやく・うまくAIを育てる」ためには、色んな工夫が必要なんだ。";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = "次は、いよいよキミが育てたAIと、ボクが対決するよ！";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = "キミのAIと勝負だ！";
                                btnNext.gameObject.SetActive(false);
                                btnBack.gameObject.SetActive(false);
                                btnOK.gameObject.SetActive(true);
                                btnRetry.gameObject.SetActive(true);
                                yield break;
                            }
                            Debug.LogWarning("Invalid uiIdx");
                        }
                        else
                        {
                            if (uiIdx == cnt++)
                            {
                                text.text = "「ごほうび」の場所が悪かったみたい。\n「ごほうび」の場所を変えて、もう一度やってみよう！";
                                btnNext.gameObject.SetActive(false);
                                btnBack.gameObject.SetActive(false);
                                btnOK.gameObject.SetActive(true);
                                btnRetry.gameObject.SetActive(true);
                                yield break;
                            }
                            Debug.LogWarning("Invalid uiIdx");
                        }
                    }
                    else if (stageIdx == 2)
                    {
                        if (game.stateP == Game.PlayerState.Win)
                        {
                            if (uiIdx == cnt++)
                            {
                                text.text = "同じ問題でも、「ごほうび」や「試行回数」を変えると結果が変わるよ！";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = "できるだけ少ない回数（早い時間）で、ボクに勝てるように工夫してみてね！";
                                btnNext.gameObject.SetActive(false);
                                btnBack.gameObject.SetActive(false);
                                btnOK.gameObject.SetActive(true);
                                btnNew.gameObject.SetActive(true);
                                btnRetry.gameObject.SetActive(true);
                                yield break;
                            }
                            Debug.LogWarning("Invalid uiIdx");
                        }
                        else
                        {
                            if (uiIdx == cnt++)
                            {
                                text.text = "もう一度学習をやり直してみる？";
                                btnNext.gameObject.SetActive(false);
                                btnBack.gameObject.SetActive(false);
                                btnOK.gameObject.SetActive(true);
                                btnRetry.gameObject.SetActive(true);
                                yield break;
                            }
                            Debug.LogWarning("Invalid uiIdx");
                        }
                    }
                }
            }
            ClearIEText();
            StopAllCoroutines();
            StartCoroutine(ie());
        }

        private List<float> trainEpsReturns = new List<float>();
        private List<bool> trainEpsGoals = new List<bool>();
        IEnumerator IE_Train()
        {
            float stepTime = 0.05f;
            if (this.episodesTurn >= 200)
                stepTime = Mathf.Max(0.01f, 5f/this.episodesTurn);

            while (this.episodesTurnLeft-- > 0)
            {
                env.Reset();
                // UI
                List<Vector2Int> traj = new List<Vector2Int>();
                uiBoard.ShowKomaP(env.row, env.col);
                uiBoard.HideTrajP();
                uiBoard.ResetRewardGot();
                uiQuest.ShowP(env.passedSpaceCnt);
                UpdatePhaseTrain();

                yield return new WaitForSecondsRealtime(stepTime);

                agent.e = QAgent.EpsilonScheduler(stageIdx==2?0.5f:1f, 0.5f, episodesTurnLeft, episodesTurn);

                float episodeReturn = 0;
                bool episodeGoal = false;
                while (true)
                {
                    // Step
                    var row = env.row; var col = env.col;
                    float progress = (float)env.passedSpaceCnt / (env.quest.Length * 2 - 1);
                    var action = agent.GetActionCurious(env.row, env.col, progress);
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

                    // UI
                    episodeReturn += reward;
                    episodeGoal = res == Env.Response.Goal;

                    yield return new WaitForSecondsRealtime(stepTime);
                    if (done) break;
                }

                // if (agent.bufferLength > 5)
                    this.loss = agent.Train();

                // UI
                trainEpsReturns.Add(episodeReturn);
                trainEpsGoals.Add(episodeGoal);
                while (trainEpsReturns.Count > 100) trainEpsReturns.RemoveAt(0);
                while (trainEpsGoals.Count > 100) trainEpsGoals.RemoveAt(0);
            }
            uiBoard.ResetRewardGot();

            btnNext.interactable = true;
            if (stageIdx == 0 && isSt0Failed)
            {
                text.text = "かんりょうー！";
                yield return WaitButton();
                text.text = "やっぱり、試行回数が多い分、ちょっと時間がかかったね…";
            }
            else
            {
                text.text = "かんりょうー！";
            }
        }

        private bool isTestPassed = false;
        private List<bool> testGoals = new List<bool>();
        IEnumerator IE_Test()
        {
            const int episodes = 5;
            const float delayStart = 0.5f;
            const float delayStep = 0.4f;
            const float delayEnd = 0.5f;

            int successCnt = 0;
            for (int ieps = 0; ieps < episodes; ieps ++)
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
                text.text = $"<size=40>　　　実験 {ieps+1} 回目</size>\n";
                yield return new WaitForSecondsRealtime(delayStart);

                bool episodeGoal = false;
                while (true)
                {
                    // Step
                    var row = env.row; var col = env.col;
                    var action = agent.GetActionStochastic(row, col);
                    var res = env.Step(action);
                    bool done = Env.IsResponseFail(res) || res == Env.Response.Goal;

                    // Control cubes
                    PlayerController.ins.RequestMove(env.row, env.col, spd:50);
                    yield return new WaitUntil(() => !PlayerController.ins.isMoving);

                    // UI
                    traj.Add(new Vector2Int(env.row, env.col));
                    uiBoard.ShowKomaP(env.row, env.col);
                    uiBoard.ShowTrajP(traj.ToArray());
                    uiQuest.ShowP(env.passedSpaceCnt);
                    // Audio
                    if (!done) AudioPlayer.ins.PlaySE(AudioPlayer.ESE.StepConfirmed);
                    else AudioPlayer.ins.PlaySE(res == Env.Response.Goal? AudioPlayer.ESE.Correct : AudioPlayer.ESE.Wrong, 0.66f);

                    // Analytics
                    if (done)
                    {
                        successCnt += res == Env.Response.Goal? 1: 0;
                        episodeGoal = res == Env.Response.Goal;
                        break;
                    }

                    yield return new WaitForSecondsRealtime(delayStep);
                }

                // UI
                this.testGoals.Add(episodeGoal);
                UpdatePhaseTest();

                if (ieps < episodes-1)
                    yield return new WaitForSecondsRealtime(delayEnd);
            }

            this.isTestPassed = successCnt > 2;

            if (isTestPassed)
            {
                Prefs.trainerStage = Mathf.Max(Prefs.trainerStage, stageIdx+1);

                // Control cubes: Perform
                PlayerController.ins.PerformHappy();

                // UI
                text.text += $"\n{successCnt}回たどり着けたので…合格ー！";
                yield return WaitButton();
                text.text = "おめでとう！";
                meicu.PerformThinkEnd();
            }
            // Fail
            else
            {
                // Control cubes: Perform
                PlayerController.ins.PerformRegret();

                // UI
                if (successCnt > 0)
                    text.text += $"\n{successCnt}回しかたどり着けなかったね…残念…";
                else
                    text.text += "1回もたどりつかなかったね…残念…";
                meicu.PerformFail();
            }
        }

        IEnumerator IE_Battle()
        {
            btnNext.interactable = false;
            text.text = "バトルスタート！";

            int successCnt = 0;
            game.StopGame();
            AIController.ins.setting = Config.trainerStageSetting;

            // for (int ieps=0; ieps < 5; ieps++)
            {
                // Control Cubes
                AIController.ins.RequestMove(4, 4, 80, 0);
                PlayerController.ins.RequestMove(4, 4, 80, 0);
                yield return new WaitUntil(() => AIController.ins.IsAtCenter);
                yield return new WaitUntil(() => PlayerController.ins.IsAtCenter);

                // UI
                uiBoard.ShowKomaP(4, 4);
                uiBoard.ShowKomaA(4, 4);
                uiBoard.HideTrajP();
                uiBoard.HideTrajA();
                uiQuest.ShowP(0);
                uiQuest.ShowA(0);

                // Start Game
                game.InitGame(env.quest);
                game.StartGame();
                isGameOverP = false;

                yield return new WaitUntil(() => game.inGame);

                // Control loop for player cube
                while (true)
                {
                    // Get action with trained agent
                    var row = game.envP.row; var col = game.envP.col;
                    var action = agent.GetActionStochastic(row, col);

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
                    successCnt += 1;
                // yield return new WaitForSecondsRealtime(1);
            }
        }

        private void OnEnterEntry()
        {
            isRetry = false;

            isSt0Failed = false;

            // Init UI
            textCaption.text = "キミだけのAIを育てよう";
            uiPhaseEntry.gameObject.SetActive(true);
            uiPhaseIntro.gameObject.SetActive(false);
            uiPhasePlan.gameObject.SetActive(false);
            uiPhaseTrain.gameObject.SetActive(false);
            uiPhaseTest.gameObject.SetActive(false);
            uiPhaseBattle.gameObject.SetActive(false);

            uiBoard.gameObject.SetActive(false);
            uiQuest.gameObject.SetActive(false);

            btnNext.gameObject.SetActive(true); btnNext.interactable = true;
            btnBack.gameObject.SetActive(true); btnBack.interactable = false;
            btnRetry.gameObject.SetActive(false);
            btnNew.gameObject.SetActive(false);
            btnOK.gameObject.SetActive(false);
            btnEntry0.interactable = false;
            btnEntry1.interactable = false;
            btnEntry2.interactable = false;

            meicu.Reset();

            for (int i = 0; i < uiIndicators.childCount; i++)
                uiIndicators.GetChild(i).gameObject.SetActive(false);
        }
        private void OnEnterIntro()
        {
            // Init training agent and environment
            env.Reset();
            GenerateQuest();

            // Update UI
            textCaption.text = "AIを育てよう";
            uiPhaseEntry.gameObject.SetActive(false);
            uiPhaseIntro.gameObject.SetActive(true);
            uiPhasePlan.gameObject.SetActive(false);
            uiPhaseTrain.gameObject.SetActive(false);
            uiPhaseTest.gameObject.SetActive(false);
            uiPhaseBattle.gameObject.SetActive(false);

            btnNext.gameObject.SetActive(true); btnNext.interactable = true;
            btnBack.gameObject.SetActive(true); btnBack.interactable = false;
            btnRetry.gameObject.SetActive(false);
            btnNew.gameObject.SetActive(false);
            btnOK.gameObject.SetActive(false);

            uiQuest.gameObject.SetActive(true);
            uiQuest.Reset();
            uiQuest.HideA();
            uiQuest.ShowP(0);
            uiQuest.ShowQuest(env.quest);

            uiBoard.gameObject.SetActive(true);
            uiBoard.Reset();
            uiBoard.ShowGoal(env.quest.goalRow, env.quest.goalCol);
            uiBoard.ShowKomaP(4, 4);

            meicu.Reset();
        }
        private void OnEnterPlan()
        {
            // Stage Settings
            if (stageIdx == 0)
            {
                this.agent.lr = 0.1f;
                this.maxRewards = 1;
                this.episodesTurn = 50;
                if (stageIdx == 0 && isSt0Failed) this.episodesTurn = 100;
                this.episodesTurnLeft = this.episodesTurn;
            }
            else if (stageIdx == 1)
            {
                this.agent.lr = 0.3f;
                this.maxRewards = 2;
                this.episodesTurn = 400;
                this.episodesTurnLeft = this.episodesTurn;
            }
            else if (stageIdx == 2)
            {
                this.agent.lr = 0.3f;
                this.maxRewards = 4;
                this.episodesTurn = 100;
                this.episodesTurnLeft = this.episodesTurn;
            }

            // Update UI
            uiPhaseEntry.gameObject.SetActive(false);
            uiPhaseIntro.gameObject.SetActive(false);
            uiPhasePlan.gameObject.SetActive(true);
            uiPhaseTrain.gameObject.SetActive(false);
            uiPhaseTest.gameObject.SetActive(false);
            uiPhaseBattle.gameObject.SetActive(false);

            btnNext.gameObject.SetActive(true); btnNext.interactable = true;
            btnBack.gameObject.SetActive(true); btnBack.interactable = false;
            btnRetry.gameObject.SetActive(false);
            btnOK.gameObject.SetActive(false);
            btnNew.gameObject.SetActive(false);

            UpdatePhasePlan();
            uiPhasePlan.Find("SliderSteps").gameObject.SetActive(stageIdx == 0 && isSt0Failed || stageIdx == 2);
            UpdateSliderByValue(this.episodesTurn);

            // Reset board and quest
            uiQuest.ShowP(0);
            uiQuest.HideA();
            uiBoard.ShowKomaP(4, 4);
            uiBoard.HideTrajP();
            uiBoard.HideKomaA();
            uiBoard.HideTrajA();

            meicu.Reset();

            // Reset Agent
            agent.Reset();
        }
        private void OnEnterTrain()
        {
            uiPhaseEntry.gameObject.SetActive(false);
            uiPhaseIntro.gameObject.SetActive(false);
            uiPhasePlan.gameObject.SetActive(false);
            uiPhaseTrain.gameObject.SetActive(true);
            uiPhaseTest.gameObject.SetActive(false);
            uiPhaseBattle.gameObject.SetActive(false);

            btnNext.interactable = true;
            btnBack.interactable = false;
            btnRetry.gameObject.SetActive(false);
            btnOK.gameObject.SetActive(false);
            btnNew.gameObject.SetActive(false);

            uiIndicators.Find("Slider").gameObject.SetActive(false);

            meicu.Reset();

            trainEpsGoals.Clear();
            trainEpsReturns.Clear();
            UpdatePhaseTrain();
        }
        private void OnEnterTest()
        {
            // Update UI
            uiPhaseEntry.gameObject.SetActive(false);
            uiPhaseIntro.gameObject.SetActive(false);
            uiPhasePlan.gameObject.SetActive(false);
            uiPhaseTrain.gameObject.SetActive(false);
            uiPhaseTest.gameObject.SetActive(true);
            uiPhaseBattle.gameObject.SetActive(false);

            uiQuest.ShowP(0);
            uiBoard.HideTrajP();
            uiBoard.ShowKomaP(4, 4);
            btnNext.interactable = true;
            btnBack.interactable = false;

            meicu.Reset();

            this.testGoals.Clear();
            UpdatePhaseTest();
        }
        private void OnEnterBattle()
        {
            uiPhaseEntry.gameObject.SetActive(false);
            uiPhaseIntro.gameObject.SetActive(false);
            uiPhasePlan.gameObject.SetActive(false);
            uiPhaseTrain.gameObject.SetActive(false);
            uiPhaseTest.gameObject.SetActive(false);
            uiPhaseBattle.gameObject.SetActive(true);

            meicu.Reset();

            btnNext.interactable = true;
            btnBack.interactable = false;
        }
        private void OnEnterSummary()
        {
            meicu.Reset();

            btnNext.interactable = true;
            btnBack.interactable = false;
        }


        private void GenerateQuest()
        {
            // Generate Quest by Stage Settings
            int questSize = 2;
            if (stageIdx == 0)
                questSize = 2;
            else if (stageIdx == 1)
                questSize = 3;
            else if (stageIdx == 2)
                questSize = 4;

            Quest quest = env.GenerateQuest(questSize);
            while (quest.goalRow >= 3 && quest.goalRow <= 5 && quest.goalCol >= 3 && quest.goalCol <=5)
                quest = env.GenerateQuest(questSize);
            env.SetQuest(quest);
        }


        #region ========== Game Callbacks ==========

        private void OnGameStarted(int countDown)
        {
            // Countdown
            if (countDown > 0)
            {
                text.text = $"　　　　　　　<size=120>{countDown}</size>";
                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.StartCount);
            }
            // Start
            else if (countDown == 0)
            {
                text.text = "　　　　<size=32>ゲームスタート！</size>";
                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Start);
            }
        }

        private bool isGameStepP = false;
        private void OnGameStepP(Env.Response res)
        {
            isGameStepP = true;
            var traj = game.GetTrajP();
            uiBoard.ShowKomaP(game.envP.row, game.envP.col);
            uiBoard.ShowTrajP(traj);
            uiQuest.ShowP(traj.Length - (Env.IsResponseFail(res)?1:0));

            if (Env.IsResponseFail(res) && game.stateA == Game.PlayerState.InGame)
            {
                StartIEText("あっ、まちがえたね");
            }
        }
        private void OnGameStepA(Env.Response res)
        {
            var traj = game.GetTrajA();
            uiBoard.ShowKomaA(game.envA.row, game.envA.col);
            uiBoard.ShowTrajA(traj);
            uiQuest.ShowA(traj.Length - (Env.IsResponseFail(res)?1:0));

            if (Env.IsResponseFail(res) && game.stateP == Game.PlayerState.InGame)
            {
                StartIEText("あっ、しまった");
            }
        }
        private bool isGameOverP = false;
        private void OnGameOverP(Game.PlayerState state)
        {
            if (state == Game.PlayerState.Win)
            {
                PlayerController.ins.PerformHappy();
                AIController.ins.PerformSad();
                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Win);

                uiResult.GetComponentInChildren<UIMeicu>().SetFace(UIMeicu.Face.Regret);
                uiResult.Find("Text").GetComponent<Text>().text = "おめでとう！キミの勝ちだ！";
                uiResult.gameObject.SetActive(true);
            }
            else if (state == Game.PlayerState.Fail)
            {
                PlayerController.ins.PerformRegret();
                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Wrong);
            }
            else if (state == Game.PlayerState.LoseFail || state == Game.PlayerState.LoseNotFail)
            {
                PlayerController.ins.PerformSad();
                AIController.ins.PerformHappy();
                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Lose);

                uiResult.GetComponentInChildren<UIMeicu>().SetFace(UIMeicu.Face.Laugh);
                uiResult.Find("Text").GetComponent<Text>().text = "ざんねん！ボクの勝ちだ！";
                uiResult.gameObject.SetActive(true);
            }
            else if (state == Game.PlayerState.Draw)
            {
                PlayerController.ins.PerformRegret();
                AIController.ins.PerformRegret();
                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Draw);

                uiResult.GetComponentInChildren<UIMeicu>().SetFace(UIMeicu.Face.Dull);
                uiResult.Find("Text").GetComponent<Text>().text = "ざんねん！ひきわけだね！";
                uiResult.gameObject.SetActive(true);
            }
            isGameOverP = true;
        }

        void OnAIThink(int phase)
        {
            if (phase == 1)
            {
                meicu.PerformThinkBegin();
                AIController.ins.PerformThink();
                var content = Random.Range(0f, 1f) < 0.5f? "考え中…" : "次は…どっちだろう？";
                StartIEText(content, wait:false);
            }
            else if (phase == 2)
            {
                meicu.PerformThinkEnd();
                var content = Random.Range(0f, 1f) < 0.5f? "そっちか！" : "わかったぞ！";
                StartIEText(content, wait:false);
            }
        }

        #endregion


        #region ========== UI Callbacks ==========


        private void OnSpaceClicked(Vector2Int rowCol, UIBoard.RewardPositionType type)
        {
            if (phase != Phase.Plan) return;

            (bool isRewardExisted, bool isRewardAdded) = uiBoard.PutReward(rowCol.x, rowCol.y, type, this.maxRewards);
            if (isRewardExisted)
                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.TurnOff);
            else if (isRewardAdded)
                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.TurnOn);

            UpdatePhasePlan();
        }

        public void OnBtnNext()
        {
            var prevPhase = phase;
            var prevIdx = uiIdx;

            if (isWaitButton)
            {
                isWaitButton = false;
                return;
            }

            if (phase == Phase.Entry)
            {
                int max = 1;
                if (Prefs.trainerStage == 0) max = 6;
                if (Prefs.trainerStage == 1) max = 2;
                if (Prefs.trainerStage == 2) max = 2;
                if (Prefs.trainerStage == 3) max = 2;
                if (uiIdx < max-1)
                    uiIdx ++;
            }
            else if (phase == Phase.Intro)
            {
                int max = 1;
                if (stageIdx == 0) max = 7;
                if (stageIdx == 1) max = 9;
                if (stageIdx == 2) max = 3;
                uiIdx ++;
                if (uiIdx >= max)
                {
                    phase = Phase.Plan;
                    uiIdx = 0;
                }
            }
            else if (phase == Phase.Plan)
            {
                int max = 1;
                if (stageIdx == 0 && !isSt0Failed) max = 3;
                if (stageIdx == 0 && isSt0Failed) max = 1;
                if (stageIdx == 1 && !isRetry) max = 8;
                if (stageIdx == 1 && isRetry) max = 1;
                if (stageIdx == 2) max = 1;
                uiIdx ++;
                if (uiIdx >= max)
                {
                    phase = Phase.Train;
                    uiIdx = 0;
                }
            }
            else if (phase == Phase.Train)
            {
                if (stageIdx == 0 || stageIdx == 1)
                {
                    uiIdx = 0;
                    phase = Phase.Test;
                }
                else if (stageIdx == 2)
                {
                    uiIdx = 0;
                    phase = Phase.Battle;
                }
            }
            else if (phase == Phase.Test)
            {
                int max = 1;
                if (stageIdx == 0 && !isSt0Failed) max = 4;
                else max = 1;

                uiIdx ++;
                if (uiIdx >= max)
                {
                    uiIdx = 0;
                    phase = Phase.Summary;
                }
            }
            else if (phase == Phase.Battle)
            {
                int max = 4;
                uiIdx ++;
                if (uiIdx >= max)
                {
                    uiIdx = 0;
                    phase = Phase.Summary;
                }
            }
            else if (phase == Phase.Summary)
            {
                uiIdx ++;
                if (stageIdx == 0 && !isSt0Failed && uiIdx >= 2)
                {
                    isSt0Failed = true;
                    uiIdx = 0;
                    phase = Phase.Plan;
                }
            }

            UpdateUI(prevPhase, prevIdx);
        }

        public void OnBtnBack()
        {
            if (uiIdx > 0)
            {
                SetPhaseAndUpdateUI(phase, uiIdx-1);
            }
        }

        public void OnBtnRetry()
        {
            // if (stageIdx == 0 && !isSt0Failed)
            // {
            //     isSt0Failed = true;
            // }
            isRetry = true;
            SetPhaseAndUpdateUI(Phase.Plan, 0);
        }
        public void OnBtnNew()
        {
            SetPhaseAndUpdateUI(Phase.Intro, 0);
        }
        public void OnBtnOK()
        {
            SetPhaseAndUpdateUI(Phase.Entry, 0);
        }

        public void OnBtnHome()
        {
            if (phase == Phase.Entry)
                PageManager.OnBtnHome();
            else
            {
                SetPhaseAndUpdateUI(Phase.Entry, 0);
                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Cancel);
            }
        }

        public void OnBtnEntry(int idx)
        {
            if (phase != Phase.Entry) return;
            this.stageIdx = idx;
            SetPhaseAndUpdateUI(Phase.Intro, 0);
        }

        public void OnBtnBGM()
        {
            AudioPlayer.ins.isBGMOn = btnBGM.isOn;
        }

        public void OnBtnResult()
        {
            uiResult.gameObject.SetActive(false);
        }

        int[] sliderStepsList = new int[]{100, 200, 400, 600, 800, 1200, 1600, 2400, 3200};
        public void OnSliderSteps()
        {
            uiIndicators.Find("Slider").gameObject.SetActive(false);

            int v = (int)uiPhasePlan.Find("SliderSteps").GetComponent<Slider>().value;
            v = sliderStepsList[v];
            this.episodesTurnLeft = v;
            this.episodesTurn = v;
            UpdatePhasePlan();
        }
        #endregion


        private bool isIETextBusy = false;
        private IEnumerator ie_text = null;
        // Coordinate setting text
        void StartIEText(string content, float minDuration = 0.8f, bool wait = true, bool force = false)
        {
            if (!wait && isIETextBusy)
                return;

            IEnumerator IE()
            {
                if (isIETextBusy)
                {
                    yield return new WaitUntil(()=>!isIETextBusy);
                }
                isIETextBusy = true;
                text.text = content;
                yield return new WaitForSecondsRealtime(minDuration);
                isIETextBusy = false;
                ie_text = null;
            }

            if (force)
            {
                if (ie_text != null)
                {
                    StopCoroutine(ie_text);
                    ie_text = null;
                }
                isIETextBusy = false;
            }
            ie_text = IE();
            StartCoroutine(ie_text);
        }
        private void ClearIEText()
        {
            if (ie_text != null) StopCoroutine(ie_text);
            ie_text = null;
            isIETextBusy = false;
        }

        private void UpdateSliderByValue(int value)
        {
            int i = 0;
            while (i < sliderStepsList.Length && sliderStepsList[i] <= value) i++;
            uiPhasePlan.Find("SliderSteps").GetComponent<Slider>().value = Mathf.Max(0, i - 1);
        }

        private void UpdatePhasePlan()
        {
            uiPhasePlan.Find("TextSteps").GetComponent<Text>().text = $"{this.episodesTurn}";
            uiPhasePlan.Find("TextRewards").GetComponent<Text>().text = $"{uiBoard.RewardCount}個 (最大{this.maxRewards}個)";
        }

        private void UpdatePhaseTrain()
        {
            int goalPercent = this.trainEpsGoals.Where(x=>x).Count();
            float returnAvg = this.trainEpsReturns.Count>0? this.trainEpsReturns.Sum() / this.trainEpsReturns.Count : 0;

            uiPhaseTrain.Find("TextSteps").GetComponent<Text>().text = $"{this.episodesTurn-this.episodesTurnLeft}/{this.episodesTurn}";
            uiPhaseTrain.Find("TextAccuracy").GetComponent<Text>().text = $"{goalPercent}%";
            // uiPhaseTrain.Find("TextReturn").GetComponent<Text>().text = $"{returnAvg:F1}";
        }

        private void UpdatePhaseTest()
        {
            for (int i = 0; i < this.testGoals.Count; i++)
                uiPhaseTest.Find("Slots").Find($"Slot ({i})").GetComponentInChildren<Text>().text =
                    this.testGoals[i]? "<color=#349DD1>O</color>" : "<color=#D23E2F>X</color>";
            for (int i = this.testGoals.Count; i < 5; i++)
                uiPhaseTest.Find("Slots").Find($"Slot ({i})").GetComponentInChildren<Text>().text = "";
        }

        private bool isWaitButton = false;
        private IEnumerator WaitButton()
        {
            btnNext.interactable = true;
            isWaitButton = true;
            yield return new WaitUntil(() => !isWaitButton);
        }

        internal enum Phase {
            Entry, Intro, Plan, Train, Test, Battle, Summary
        }

    }

}
