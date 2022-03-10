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
        public Game game;

        public GameObject ui;
        public UIMeicu meicu;
        public UIBoard uiBoard;
        public UIQuest uiQuest;
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
        private bool isFail = false;
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

            phase = Phase.Entry;
            uiIdx = 0;

            if (active)
            {
                btnBGM.isOn = AudioPlayer.ins.isBGMOn;


                env.Reset();
                uiBoard.Reset();
                uiQuest.Reset();
                uiQuest.HideA();

                game.stepCallbackP += OnGameStepP;
                game.stepCallbackA += OnGameStepA;
                game.overCallbackP += OnGameOverP;

                PlayerController.ins.targetMatCoordBias = new Vector2Int(0, 0);

                // InitPhases();
                // Refresh();
                phase = Phase.Summary;
                SetPhaseAndUpdateUI(Phase.Entry, 0);
            }
            else
            {
                game.StopGame();
                game.stepCallbackP -= OnGameStepP;
                game.stepCallbackA -= OnGameStepA;
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


        // private PhaseIE phaseIE;
        // internal void Refresh()
        // {
        //     StopAllCoroutines();
        //     var ie = phaseIE.GetIE(phaseIE);
        //     StartCoroutine(ie);
        // }
        // internal void NextPhase()
        // {
        //     if (phaseIE.next == null) return;

        //     StopAllCoroutines();
        //     var ie = phaseIE.next.GetIE(phaseIE);
        //     phaseIE = phaseIE.next;
        //     StartCoroutine(ie);
        // }
        // internal void PrevPhase()
        // {
        //     if (phaseIE.prev == null) return;

        //     StopAllCoroutines();
        //     var ie = phaseIE.prev.GetIE(phaseIE);
        //     phaseIE = phaseIE.prev;
        //     StartCoroutine(ie);
        // }

        // public class PhaseIE
        // {
        //     public PhaseIE prev = null;
        //     public PhaseIE next = null;
        //     public delegate IEnumerator IEFactory(PhaseIE from);
        //     public IEFactory ieFactory = null;
        //     public IEnumerator GetIE(PhaseIE from)
        //     {
        //         if (ieFactory == null) yield break;
        //         yield return ieFactory(from);
        //     }

        //     public static PhaseIE CreateTextOnly(TMP_Text text, string content)
        //     {
        //         var p = new PhaseIE();
        //         IEnumerator ie (PhaseIE from)
        //         {
        //             text.text = content;
        //             yield break;
        //         }
        //         p.ieFactory = new IEFactory(ie);
        //         return p;
        //     }
        //     public static void Connect(PhaseIE prev, PhaseIE next)
        //     {
        //         prev.next = next;
        //         next.prev = prev;
        //     }
        // }

        // private void InitPhases()
        // {
        //     var s0intro = PhaseIE.CreateTextOnly(text, "hello");
        //     var phase2 = PhaseIE.CreateTextOnly(text, "bye");
        //     PhaseIE.Connect(phase1, phase2);

        //     var phase3 = 

        //     phaseIE = phase1;
        // }

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
                            text.text = "え？「学習」ってどのようにするのかって？";
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
                            text.text = "どこに「ごほうび♥」を置くのか、何回学習するのか、よく考えて挑戦してみよう！";
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
                            text.text = "ようこそ！\nここでは、「ごほうび♥」を使ってAIを育ててみるよ！";
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "「バトル」と同じように、スタートとゴール、そして「どの色を通過するか」という問題が出されているね。";
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "キミはスグに「こう進めばいい」と分かったと思うけど、AIは最初は色も場所も分かってないから、「どっちに進めば正解なのか」も全く分からないんだったよね。";
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "そんなAIにどうやって「学習」させるのか、それは……。";
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "「ごほうび♥」を使うんだよ！";
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "道に「ごほうび♥」をおいておくと、AIはごほうびをたどって「こっちが正しい道だぞ！」とおぼえていくんだ。";
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "そうやって繰り返して覚えていくのが「学習する」ということになるんだ。どう？分かったかな？";
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
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "この問題を見て！さっきよりも長くなっているでしょ？さっきの問題をクリアするには、400回学習必要だったけど…";
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "この問題の場合（通過する色のマスが3つある場合）、だいたい1000回は学習しないと、うまくゴールまでの道を覚えられないんだ…";
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "つまり「学習にかかる時間」がとっても長くなっちゃうんだ…";
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
                            text.text = "「ごほうび♥」を<color=red>2個</color>使ってみるよ！";
                            yield break;
                        }
                        if (uiIdx == cnt++)
                        {
                            text.text = "「ごほうび♥」を2個使うことで、400回まで回数をへらすことができるんだ！";
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
                            text.text = "ここでは、「ごほうび♥」の置き方、「学習」の回数はキミ次第！";
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
                                text.text = "今回のゴールはスタートのすぐそばだから「ごほうび♥」もゴールに置けばよさそうだね！";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = "マウスでゴール（はたが立っているマス）をクリックしてみてね。";
                                btnNext.interactable = false;
                                yield return new WaitUntil(() => uiBoard.RewardCount > 0);
                                btnNext.interactable = true;
                                text.text = $"よし！それでは学習開始！{episodesTurn}回試行錯誤（しこうさくご）してみるよ！";
                                yield break;
                            }
                            Debug.LogWarning("Invalid uiIdx");
                        }
                        else
                        {
                            text.text = "上の白いボタンを右に動かすと試行回数を変えられるよ。回数を増やしてから、もう一回学習させよう。";
                            btnNext.interactable = true;
                        }
                    }
                    else if (stageIdx == 1)
                    {
                        if (!isRetry)
                        {
                            if (uiIdx == cnt++)
                            {
                                btnNext.interactable = false;
                                text.text = $"まず、前の問題と同じように、最終的にたどり着きたいゴールに「ごほうび」を置いてみよう。";
                                yield return new WaitUntil(() =>
                                    uiBoard.RewardCount == 1 &&
                                    uiBoard.rewardList[0] == new Vector3Int(env.quest.goalRow, env.quest.goalCol, 0));
                                text.text = $"ここまでくればごほうびがもらえる、ってAIに覚えさせるんだ。";
                                btnNext.interactable = true;
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = $"ゴールがすぐ横にある場合は、何も学習しなくても1/4のかくりつでゴールにたどり着けるよね。";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = $"でも、ゴールが遠くなればなるほど、そこまでぐうぜん到達できる確率は減るんだ。";
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
                                text.text = $"でも、これってとっても時間が掛かっちゃうよね…";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = $"「長い1つの問題」を「短い2つの問題」に分けるように考えて...";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                btnNext.interactable = false;
                                text.text = $"ゴールまでの間にもう一つ「ごほうび」を置いてみよう！";
                                yield return new WaitUntil(() => uiBoard.RewardCount > 1);
                                text.text = "よし！今回も「試行回数」は400回でやってみるよ！";
                                btnNext.interactable = true;
                                yield break;
                            }
                            Debug.LogWarning("Invalid uiIdx");
                        }
                        else
                        {
                            if (uiIdx == cnt++)
                            {
                                btnNext.interactable = false;
                                text.text = "「ごほうび♥」の場所を決めてね。";
                                yield return new WaitUntil(() => uiBoard.RewardCount > 0);
                                text.text = "よし！今回も「試行回数」は400回でやってみるよ！";
                                btnNext.interactable = true;
                                yield break;
                            }
                            Debug.LogWarning("Invalid uiIdx");
                        }
                    }
                    else if (stageIdx == 2)
                    {
                        btnNext.interactable = false;
                        text.text = "「ごほうび♥」の場所を決めてね。";
                        yield return new WaitUntil(() => uiBoard.RewardCount > 0);
                        text.text = "よし！それでは学習開始！";
                        btnNext.interactable = true;
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

                        if (isBattleWin)
                        {
                            text.text = "おめでとう！キミの勝ちだ！";
                        }
                        else
                        {
                            text.text = "ざんねん！ボクの勝ちー！";
                        }
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
                                btnNext.gameObject.SetActive(false);
                                btnBack.gameObject.SetActive(false);
                                btnRetry.gameObject.SetActive(true);
                                yield break;
                            }
                            Debug.LogWarning("Invalid uiIdx");
                        }
                        else if (!isSt0Failed && isTestPassed)
                        {
                            if (uiIdx == cnt++)
                            {
                                text.text = $"なんとぐうぜんに学習できた！けど回数を増やすとあんていして学習できるので試してみよう！";
                                btnNext.gameObject.SetActive(false);
                                btnBack.gameObject.SetActive(false);
                                btnRetry.gameObject.SetActive(true);
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
                                text.text = "うまく学習できたね！";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = "AIを育てるには、「ごほうび♥」をあげたり、「試行回数」を変えたり、うまく育つようにいろんな事を考えて試す必要があるんだ。";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = "問題が難しくなればなるほど、育てるのも難しくなったり、時間が掛かったりする…";
                                yield break;
                            }
                            if (uiIdx == cnt++)
                            {
                                text.text = "だから「はやく・うまくAIを育てる」ためには、色んな工夫が必要なんだ。";
                                yield break;
                            }
                            // if (uiIdx == cnt++)
                            // {
                            //     text.text = "今は「ごほうびをあげる」「回数を変えてみる」だけだったけど、本当は失敗したら怒ったり（マイナスのほうしゅうと言います）など...";
                            //     yield break;
                            // }
                            if (uiIdx == cnt++)
                            {
                                text.text = "AIの学習には色々な調節するもの（パラメータと言います）があるんだ。これは難しいので、またの機会に説明するね！";
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
                                text.text = "「ごほうび♥」の場所が悪かったみたい。\n「ごほうび♥」の場所を変えて、もう一度やってみよう！";
                                btnNext.gameObject.SetActive(false);
                                btnBack.gameObject.SetActive(false);
                                btnRetry.gameObject.SetActive(true);
                                yield break;
                            }
                            Debug.LogWarning("Invalid uiIdx");
                        }
                    }
                    else if (stageIdx == 2)
                    {
                        if (isBattleWin)
                        {
                            if (uiIdx == cnt++)
                            {
                                text.text = "同じ問題でも、「ごほうび♥」や「試行回数」を変えると結果が変わるよ！";
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
            StopAllCoroutines();
            StartCoroutine(ie());
        }

        private List<float> trainEpsReturns = new List<float>();
        private List<bool> trainEpsGoals = new List<bool>();
        IEnumerator IE_Train()
        {
            float stepTime = Mathf.Max(0.01f, 5f/this.episodesTurn);

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

                agent.e = QAgent.EpsilonScheduler(1f, 0.5f, episodesTurnLeft, episodesTurn);

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
                text.text = $"実験 {ieps+1} 回目\n";
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
                    AudioPlayer.ins.PlaySE(AudioPlayer.ESE.StepConfirmed);

                    // UI
                    traj.Add(new Vector2Int(env.row, env.col));
                    uiBoard.ShowKomaP(env.row, env.col);
                    uiBoard.ShowTrajP(traj.ToArray());
                    uiQuest.ShowP(env.passedSpaceCnt);

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
                text.text = "おめでとう！キミのAIも正しい道を学習できてきたね！";
            }
            // Fail
            else
            {
                // Control cubes: Perform
                PlayerController.ins.PerformRegret();

                // UI
                text.text += $"\n\n{successCnt}回しかたどり着けなかったね…残念…";
            }
        }

        private bool isBattleWin = false;
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
                game.StartGame(0);
                isGameOverP = false;

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
                // yield return new WaitForSecondsRealtime(1);
            }

            this.isBattleWin = successCnt > 0;
        }

        private void OnEnterEntry()
        {
            isRetry = false;

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
        }
        private void OnEnterPlan()
        {
            // Stage Settings
            if (stageIdx == 0)
            {
                this.agent.lr = 0.1f;
                this.maxRewards = 1;
                this.episodesTurn = 50;
                this.episodesTurnLeft = this.episodesTurn;
            }
            else if (stageIdx == 1)
            {
                this.agent.lr = 0.2f;
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

            // Reset board and quest
            uiQuest.ShowP(0);
            uiQuest.HideA();
            uiBoard.ShowKomaP(4, 4);
            uiBoard.HideTrajP();
            uiBoard.HideKomaA();
            uiBoard.HideTrajA();

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

            btnNext.interactable = true;
            btnBack.interactable = false;
        }
        private void OnEnterSummary()
        {
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

        private bool isGameStepP = false;
        private void OnGameStepP(Env.Response res)
        {
            isGameStepP = true;
            uiQuest.ShowP(game.envP.passedSpaceCnt);
            uiBoard.ShowKomaP(game.envP.row, game.envP.col);
            uiBoard.ShowTrajP(game.GetTrajP());
        }
        private void OnGameStepA(Env.Response res)
        {
            uiQuest.ShowA(game.envA.passedSpaceCnt);
            uiBoard.ShowKomaA(game.envA.row, game.envA.col);
            uiBoard.ShowTrajA(game.GetTrajA());
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
                uiBoard.PutReward(rowCol.x, rowCol.y, type, this.maxRewards);
            }
            else if (stageIdx == 1)
            {
                uiBoard.PutReward(rowCol.x, rowCol.y, type, this.maxRewards);
            }
            else if (stageIdx == 2)
            {
                uiBoard.PutReward(rowCol.x, rowCol.y, type, this.maxRewards);
            }
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
                if (uiIdx < max-1)
                    uiIdx ++;
            }
            else if (phase == Phase.Intro)
            {
                int max = 1;
                if (Prefs.trainerStage == 0) max = 7;
                if (Prefs.trainerStage == 1) max = 9;
                if (Prefs.trainerStage == 2) max = 3;
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
                int max = 4;
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
            if (stageIdx == 0 && !isSt0Failed)
            {
                isSt0Failed = true;
            }
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
                OnBtnOK();
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

        public void OnSliderSteps()
        {
            int v = (int)uiPhasePlan.Find("SliderSteps").GetComponent<Slider>().value * 100;
            this.episodesTurnLeft = v;
            this.episodesTurn = v;
            UpdatePhasePlan();
        }
        #endregion


        private void UpdatePhasePlan()
        {
            uiPhasePlan.Find("TextSteps").GetComponent<Text>().text = $"{this.episodesTurn}";
            uiPhasePlan.Find("TextRewards").GetComponent<Text>().text = $"{uiBoard.RewardCount}個 (最大{this.maxRewards}個)";
            uiPhasePlan.Find("SliderSteps").GetComponent<Slider>().value = episodesTurn/100;
        }

        private void UpdatePhaseTrain()
        {
            int goalPercent = this.trainEpsGoals.Where(x=>x).Count();
            float returnAvg = this.trainEpsReturns.Count>0? this.trainEpsReturns.Sum() / this.trainEpsReturns.Count : 0;

            uiPhaseTrain.Find("TextSteps").GetComponent<Text>().text = $"{this.episodesTurn-this.episodesTurnLeft, 4}/{this.episodesTurn, -4}";
            uiPhaseTrain.Find("TextAccuracy").GetComponent<Text>().text = $"{goalPercent}%";
            uiPhaseTrain.Find("TextReturn").GetComponent<Text>().text = $"{returnAvg:F1}";
        }

        private void UpdatePhaseTest()
        {
            for (int i = 0; i < this.testGoals.Count; i++)
                uiPhaseTest.Find("Slots").Find($"Slot ({i})").GetComponentInChildren<Text>().text = this.testGoals[i]? "O" : "X";
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
