using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace toio.AI.meicu
{

    public class PageBattle : MonoBehaviour
    {
        public UIBoard uiBoard;
        public UIQuest uiQuest;
        public Game game;

        [Header("UI Components")]
        public GameObject ui;
        public Button btnStart;
        public UISwitch swHint;
        public UISwitch btnBGM;
        public TMP_Text text;
        public Transform trLevel;
        public Transform trStage;
        public UIMeicu meicu;

        [Header("Result UI Components")]
        public GameObject resultObj;
        public UIMeicu meicuResult;
        public Text textResult;
        public Text textResultQuit;
        public Text textTag;

        public enum BattleState { NotStarted, InGame, PWin, AWin, Draw }
        private BattleState state = BattleState.NotStarted;
        private int stage = 1;
        private bool keepQuest = false;


        internal void SetActive(bool active)
        {
            if (ui.activeSelf == active) return;
            ui.SetActive(active);

            if (active)
            {
                Prefs.SetBattleAccessed();
                if (Prefs.isLearnCleared)
                    Prefs.SetBattleEnteredAfterLearn();

                btnBGM.isOn = AudioPlayer.ins.isBGMOn;

                uiQuest.Reset();
                uiBoard.Reset();
                AIController.ins.heatmapCallback += OnAIHeatmap;
                AIController.ins.thinkCallback += OnAIThink;
                game.initedCallback += OnGameInited;
                game.readyCallback += OnGameReady;
                game.startCallback += OnGameStarted;
                game.overCallback += OnGameOver;
                game.overCallbackP += OnGameOverP;
                game.overCallbackA += OnGameOverA;
                game.stepCallbackP += OnGameStepP;
                game.stepCallbackA += OnGameStepA;

                // Init vars
                state = BattleState.NotStarted;
                stage = 1;
                textBusy = false;
                keepQuest = false;

                // Init UIs
                btnStart.gameObject.SetActive(true);
                btnStart.transform.GetComponentInChildren<Text>().text = "スタート";
                resultObj.SetActive(false);
                swHint.isOn = false;

                SetQuestIndicator(false);

                meicu.Reset();
                SetIntroText();
                UpdateStageText();
                UpdateHint();
                LoadLevel();

                // Un-Pause controllers
                AIController.ins.isPause = false;
                PlayerController.ins.isPause = false;
            }
            else
            {
                StopAllCoroutines();
                game.StopGame();
                AIController.ins.heatmapCallback -= OnAIHeatmap;
                AIController.ins.thinkCallback -= OnAIThink;
                game.initedCallback -= OnGameInited;
                game.readyCallback -= OnGameReady;
                game.startCallback -= OnGameStarted;
                game.overCallback -= OnGameOver;
                game.overCallbackP -= OnGameOverP;
                game.overCallbackA -= OnGameOverA;
                game.stepCallbackP -= OnGameStepP;
                game.stepCallbackA -= OnGameStepA;
            }
        }

        internal void Pause()
        {
            AIController.ins.isPause = true;
            PlayerController.ins.isPause = true;
        }

        internal void Resume()
        {
            AIController.ins.isPause = false;
            PlayerController.ins.isPause = false;
        }


        void LevelUp()
        {
            stage = 1;
            if (Prefs.level == Config.nLevels)
                return;

            Prefs.LevelUp();
            LoadLevel();
        }
        void LoadLevel()
        {
            AIController.ins.LoadModelByLevel(Prefs.level);
        }


        #region ======== UI functions ========

        private bool textBusy = false;
        private IEnumerator ie_text = null;
        // Coordinate setting text
        void SetText(string content, float minDuration = 0.8f, bool wait = true, bool force = false)
        {
            if (!wait && textBusy)
                return;

            IEnumerator IE()
            {
                if (textBusy)
                {
                    yield return new WaitUntil(()=>!textBusy);
                }
                textBusy = true;
                text.text = content;
                yield return new WaitForSecondsRealtime(minDuration);
                textBusy = false;
                ie_text = null;
            }

            if (force)
            {
                if (ie_text != null)
                {
                    StopCoroutine(ie_text);
                    ie_text = null;
                }
                textBusy = false;
            }
            ie_text = IE();
            StartCoroutine(ie_text);
        }

        string[] classNames = new string[]{"", "ビギナー", "ジュニア", "シニア", "プロ", "サブリーダー", "リーダー", "サブトレーナー", "トレーナー", "スター", "スーパースター", "マスター"};
        // Set text before game started
        void SetIntroText()
        {
            // First level/stage entered
            if (state == BattleState.NotStarted && Prefs.level == 1 && stage == 1)
            {
                SetText($"それじゃぁ、ボクとたたかってみよう！\nボクに5回勝つとレベルクリアだよ！\n\n最初のボクは【{classNames[1]}】、\nまだまだ弱いけど…");
            }
            // New level enterd.
            else if ((state == BattleState.NotStarted || state == BattleState.PWin) && stage == 1)
            {
                int n = new int[]{0, 1, 10, 50, 100, 200, 500, 700, 900, 1000, 1500, 2000}[Prefs.level];
                int p = new int[]{0, 80, 99, 60, 80, 99, 60, 80, 90, 60, 85, 70}[Prefs.level];
                var content = $"ボクは【迷キュー・{classNames[Prefs.level]}】だよ\n試行錯誤を<color=red>{n}万回以上</color>したんだ。\nこのレベルでのゴール率は<color=red>{p}%</color>だよ。\n\nどうだい？勝てるかな？";
                if (Prefs.level == 3)
                    content += "\n（ここからリセットボタン使えないから\n間違えないように気をつけてね）";
                else if (Prefs.level == 4)
                    content += "\n（ここから負けた時には、\nちがうお題が出るんだよ）";
                else if (Prefs.level == 11)
                    content += "\n<size=21>※このレベルは5連勝しないとクリアできない</size>";
                SetText(content);
            }
            // New Stage enterd
            if (state == BattleState.PWin && stage > 1)
            {
                SetText("次のステージに行こう！\nボクはもっと早くなるぞ！");
            }
            // P Lose
            else if (state == BattleState.AWin)
            {
                SetText("もう一回やってみよう！\n次も負けないぞ！");
            }
            // Draw
            else if (state == BattleState.Draw)
            {
                SetText("もう一回やってみよう！\n次は勝つぞ！");
            }

        }

        void UpdateStageText()
        {
            trLevel.GetComponentInChildren<Text>().text = $"レベル  {Prefs.level}";
            ui.transform.Find("LevelView").GetComponent<UILevelView>().ShowLevel(Prefs.level);
            trStage.GetComponentInChildren<UIStage>().SetHand(stage, 5);
            textTag.text = "迷キュー・" + classNames[Prefs.level];
        }

        void UpdateHint()
        {
            if (swHint.isOn)
            {
                ui.transform.Find("ImgThink").gameObject.SetActive(true);
                uiBoard.ShowHeatmap(AIController.ins.Heatmap);
            }
            else
            {
                ui.transform.Find("ImgThink").gameObject.SetActive(false);
                uiBoard.HideHeatmap();
            }
        }

        void SetQuestIndicator(bool active, int questSize = 1)
        {
            var q = ui.transform.Find("Indicators").Find("Quest") as RectTransform;

            if (active)
                q.sizeDelta = new Vector2(questSize * 2 * 38 + 134, q.sizeDelta.y);

            q.gameObject.SetActive(active);
        }

        #endregion


        #region ======== Hanlde game result ========

        void ProcPlayerWin()
        {
            // Level Clear
            if (stage == Config.levelSettings[Prefs.level - 1].nStages)
            {
                // Play SE
                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.LevelUp);

                // Set Result text
                // Lv.1 cleared
                if (Prefs.level == 1)
                {
                    textResult.text = $"おめでとう！レベル {Prefs.level} クリアだよ！";
                    textResultQuit.text = "　　「かいせつ」が開放されたよ。\n　　見てみてね！";
                    // btnStart will jump to PageTitle
                    resultToTitle = true;
                }
                // Lv.2 cleared
                else if (Prefs.level == 2)
                {
                    textResult.text = $"おめでとう！レベル {Prefs.level} クリアだよ！";
                    textResultQuit.text = "　　「キミだけのAIを育てよう」が\n　　開放されたよ。見てみてね！";
                    // btnStart will jump to PageTitle
                    resultToTitle = true;
                }
                // All Lv cleared
                else if (Prefs.level == Config.nLevels)
                {
                    textResult.text = "すごい！！\nキミは、ついに迷キューマスターのボクを倒したんだ！\n\nこれからは、キミが迷キューマスターだ！！";
                    textResultQuit.text = "おめでとうございます。\nあなたは、全ステージクリアしました！\n\n最後まで遊んでいただいて\nありがとうございました。";
                }
                else
                {
                    textResult.text = $"おめでとう！レベル {Prefs.level} クリアだよ！";
                    textResultQuit.text = "";
                }

                // Lv up
                LevelUp();
            }
            // Stage Clear
            else
            {
                // Play SE
                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Win);

                // Set Result text
                textResult.text = "おめでとう！きみの勝ち－！！次は負けないぞ！";
                textResultQuit.text = "";

                // Stage up
                stage += 1;
            }

            // meicu emotion
            meicuResult.SetFace(UIMeicu.Face.Regret);

            // show result
            resultObj.SetActive(true);

            // clear text
            SetText("", force:true);

            // update btnStart
            btnStart.transform.GetComponentInChildren<Text>().text = "次へ";
        }

        void ProcPlayerLose()
        {
            // Play SE
            AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Lose);

            // Set Result text
            textResult.text = "どうだっ！ボクの勝ちだよ！";
            textResultQuit.text = "";

            // Stage down or not
            var failBehaviour = Config.levelSettings[Prefs.level - 1].failBehaviour;
            if (failBehaviour == Config.FailBehaviour.KeepQuest)
            {
                keepQuest = true;
            }
            else if (failBehaviour == Config.FailBehaviour.KeepStage)
            {}
            else
            {
                stage = 1;
            }

            // meicu emotion
            meicuResult.SetFace(UIMeicu.Face.Laugh);

            // show result
            resultObj.SetActive(true);

            // clear text
            SetText("", force:true);

            // update btnStart
            btnStart.transform.GetComponentInChildren<Text>().text = "もう一回";
        }

        void ProcDraw()
        {
            // Play SE
            AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Draw);

            // Set Result text
            textResult.text = "うわっ、まちがえた･･･\n引き分けだね、もういっかいやろう";
            textResultQuit.text = "";

            // Keep stage and quest
            keepQuest = true;

            // meicu emotion
            meicuResult.SetFace(UIMeicu.Face.Dull);

            // show result
            resultObj.SetActive(true);

            // clear text
            SetText("", force:true);

            // update btnStart
            btnStart.transform.GetComponentInChildren<Text>().text = "もう一回";
        }

        #endregion


        #region ======== UI Callbacks ========
        public void OnBtnStart()
        {
            bool keepQuest = this.keepQuest || game.inGame; // inGame means Reset
            this.keepQuest = false;

            state = BattleState.InGame;

            btnStart.gameObject.SetActive(false);

            var levelSetting = Config.levelSettings[Prefs.level - 1];
            var stageSetting = levelSetting.stageSettings[stage-1];

            uiQuest.Reset();
            uiBoard.Reset();

            AIController.ins.setting = stageSetting;

            game.InitGame(levelSetting.questSize, keepQuest);
            game.WaitReady();

            UpdateStageText();
            UpdateHint();
        }

        public void OnBtnHint()
        {
            UpdateHint();
        }

        bool resultToTitle = false;
        public void OnBtnResult()
        {
            if (resultToTitle)
            {
                resultToTitle = false;
                PageManager.SetPage(PageManager.EPage.Title);
            }
            else
            {
                resultObj.SetActive(false);
                btnStart.gameObject.SetActive(true);
                meicu.Reset();
                SetIntroText();
                UpdateStageText();
            }
        }

        public void OnBtnBGM()
        {
            AudioPlayer.ins.isBGMOn = btnBGM.isOn;
        }
        #endregion


        #region ======== Game Callbacks ========
        void OnGameInited()
        {
        }

        void OnGameReady(bool ready)
        {
            if (ready)
            {
                Debug.Log("PageBattle.OnGameReady: Ready");
                SetText("スタートにタッチしてゲームスタート！\n\nボクがいても強引にタッチしていいよ。\nタッチすると「問題」が表示されるから見ててね！", minDuration:0);
                uiBoard.ShowKomaA(new Vector2Int(4, 4));
                uiBoard.ShowKomaP(new Vector2Int(4, 4));
                SetQuestIndicator(true, game.quest.Length);

                IEnumerator Wait2Start()
                {
                    yield return new WaitUntil(()=>PlayerController.ins.IsAtCenter);
                    Debug.Log("PageBattle.OnGameReady: Touched, Call game.StartGame");
                    AudioPlayer.ins.PlaySE(AudioPlayer.ESE.StartConfirmed);
                    yield return new WaitForSecondsRealtime(0.3f);
                    game.StartGame();
                }
                StartCoroutine(Wait2Start());
            }
            else
            {
                SetText("キミのキューブを手に持って！\nボクはスタートマスに移動するね！", minDuration:0);
            }
        }

        void OnGameStarted(int countDown)
        {
            // Countdown
            if (countDown > 0)
            {
                // Show quest
                uiQuest.ShowQuest(game.quest);
                uiBoard.ShowGoal(new Vector2Int(game.quest.goalRow, game.quest.goalCol));

                // Countdown with text
                SetText($"　　　　　　　<size=120>{countDown}</size>", force:true);

                // Play SE
                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.StartCount);
            }
            // Start
            else if (countDown == 0)
            {
                Debug.Log("Game Started");
                SetText("　　　　<size=32>ゲームスタート！</size>\n\nお題の順番に合わせて\nキューブを動かそう！", minDuration:1f, force:true);

                SetQuestIndicator(false);

                // Play SE
                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Start);

                // Lv.1~2 Player can restart during game.
                if (Prefs.level < 3)
                {
                    btnStart.gameObject.SetActive(true);
                    btnStart.transform.GetComponentInChildren<Text>().text = "リセット";
                }
            }
        }

        void OnGameOver(Game.PlayerState stateP, Game.PlayerState stateA)
        {
            if (stateP == Game.PlayerState.Win)
            {
                Debug.Log("PageBattle.OnGameOver: Player Win");
                state = BattleState.PWin;
                ProcPlayerWin();
            }
            else if (stateA == Game.PlayerState.Win)
            {
                Debug.Log("PageBattle.OnGameOver: AI Win");
                state = BattleState.AWin;
                ProcPlayerLose();
            }
            else
            {
                Debug.Log("PageBattle.OnGameOver: Draw");
                state = BattleState.Draw;
                ProcDraw();
            }
        }
        void OnGameOverP(Game.PlayerState stateP)
        {
            if (stateP == Game.PlayerState.Fail)
            {
                PlayerController.ins.PerformRegret();
                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Wrong);
                uiBoard.ShowFailP();
            }
            else if (stateP == Game.PlayerState.Draw)
            {
                PlayerController.ins.PerformRegret();
            }
            else if (stateP == Game.PlayerState.LoseFail || stateP == Game.PlayerState.LoseNotFail)
            {
                PlayerController.ins.PerformSad();
            }
            else if (stateP == Game.PlayerState.Win)
            {
                PlayerController.ins.PerformHappy();
            }
        }
        void OnGameOverA(Game.PlayerState stateA)
        {
            if (stateA == Game.PlayerState.Fail)
            {
                AIController.ins.PerformRegret();
                uiBoard.ShowFailA();
                meicu.PerformFail();
            }
            else if (stateA == Game.PlayerState.Draw)
            {
                AIController.ins.PerformRegret();
            }
            else if (stateA == Game.PlayerState.LoseFail || stateA == Game.PlayerState.LoseNotFail)
            {
                AIController.ins.PerformSad();
            }
            else if (stateA == Game.PlayerState.Win)
            {
                AIController.ins.PerformHappy();
            }
        }

        void OnGameStepP(Env.Response res)
        {
            var pos = game.GetPosP();
            Debug.Log($"Player steps at {pos.x}, {pos.y}");

            var traj = game.GetTrajP();
            uiBoard.ShowTrajP(traj);
            uiBoard.ShowKomaP(pos);

            uiQuest.ShowP(traj.Length - (Env.IsResponseFail(res)?1:0));

            if (Env.IsResponseFail(res) && game.stateA == Game.PlayerState.InGame)
            {
                SetText("あっ、まちがえたね");
            }
        }

        void OnGameStepA(Env.Response res)
        {
            var pos = game.GetPosA();
            Debug.Log($"AI steps at {pos.x}, {pos.y}");

            var traj = game.GetTrajA();
            uiBoard.ShowTrajA(traj);
            uiBoard.ShowKomaA(pos);

            uiQuest.ShowA(traj.Length - (Env.IsResponseFail(res)?1:0));

            if (Env.IsResponseFail(res) && game.stateP == Game.PlayerState.InGame)
            {
                SetText("あっ、しまった");
            }
        }
        #endregion


        #region ======== AI controller Callbacks ========
        void OnAIHeatmap()
        {
            if (swHint.isOn)
                uiBoard.ShowHeatmap(AIController.ins.Heatmap);
        }
        void OnAIThink(int phase)
        {
            if (phase == 1)
            {
                meicu.PerformThinkBegin();
                AIController.ins.PerformThink();
                var content = Random.Range(0f, 1f) < 0.5f? "考え中…" : "次は…どっちだろう？";
                SetText(content, wait:false);
            }
            else if (phase == 2)
            {
                meicu.PerformThinkEnd();
                var content = Random.Range(0f, 1f) < 0.5f? "そっちか！" : "わかったぞ！";
                SetText(content, wait:false);
            }
        }
        #endregion
    }

}
