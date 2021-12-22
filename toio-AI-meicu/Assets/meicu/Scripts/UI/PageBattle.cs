using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


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
        public Text text;
        public Transform trLevel;
        public Transform trStage;
        public UIMeicu meicu;

        [Header("Result UI Components")]
        public GameObject resultObj;
        public UIMeicu meicuResult;
        public Text textResult;
        public Text textResultQuit;
        public Text textTag;

        private int stage = 1;
        private bool textBusy = false;


        internal void SetActive(bool active)
        {
            if (ui.activeSelf == active) return;
            ui.SetActive(active);

            if (active)
            {
                if (MeiPrefs.isLearnCleared)
                    MeiPrefs.SetBattleEnteredAfterLearn();

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

                stage = 1;
                textBusy = false;

                btnStart.gameObject.SetActive(true);
                btnStart.transform.GetComponentInChildren<Text>().text = "スタート";
                resultObj.SetActive(false);
                swHint.isOn = false;

                SetTextIntro();
                UpdateStageText();
                UpdateHint();
                LoadLevel();

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
            if (MeiPrefs.level == Config.nLevels)
                return;

            MeiPrefs.LevelUp();
            LoadLevel();
        }
        void LoadLevel()
        {
            AIController.ins.LoadModelByLevel(MeiPrefs.level);
        }


        private IEnumerator ie_text = null;
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
        void SetTextIntro()
        {
            if (stage > 1)
            {
                SetText("つぎのステージやろう!\n僕はもっと早くなるぞ！");
            }
            else if (MeiPrefs.level == 1)
            {
                SetText("それじゃぁ、僕とたたかってみよう！\n\n最初の僕は【ビギナー】、\nまだまだ弱いけど…");
            }
            else
            {
                int n = new int[]{0, 1, 10, 50, 100, 200, 500, 700, 900, 1000, 1500, 2000}[MeiPrefs.level];
                var content = $"僕は【迷キュー{classNames[MeiPrefs.level]}】だよ\n試行錯誤を<color=red>{n}万回以上</color>したんだ。\n\nどうだい？勝てるかな？";
                if (MeiPrefs.level == 3)
                    content += "\n（ここからリセットボタン使えないから\n間違えないように気をつけてね）";

                SetText(content);
            }

            textTag.text = "迷キュー" + classNames[MeiPrefs.level];

            UpdateStageText();
        }

        void UpdateStageText()
        {
            trLevel.GetComponentInChildren<UIBrain>().SetLevel(MeiPrefs.level);
            trStage.GetComponentInChildren<UIStage>().SetHand(stage, 5);
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

        void ProcPlayerWin()
        {
            // Level Clear
            if (stage == Config.levelSettings[MeiPrefs.level - 1].nStages)
            {
                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.LevelUp);

                if (MeiPrefs.level == Config.nLevels)
                {
                    textResult.text = "おめでとう！すべてクリアだよ！\n今の僕はキミにかなわない…\n僕ももっと「学習」して、もっと強くなったら、また勝負してね！";
                }
                else
                {
                    textResult.text = $"おめでとう！レベル {MeiPrefs.level} クリアだよ！";
                }

                IEnumerator IE()
                {
                    if (MeiPrefs.level == 1)
                    {
                        resultToTitle = true;
                        textResultQuit.text = "「かいせつ」が開放されたよ。\n見てみよう！";
                    }
                    else
                    {
                        textResultQuit.text = "";
                    }
                    yield break;
                }
                StartCoroutine(IE());

                LevelUp();
            }
            // Stage Clear
            else
            {
                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Win);

                stage += 1;
                textResult.text = "おめでとう！きみの勝ち－！！次は負けないぞ！";

                IEnumerator IE()
                {
                    textResultQuit.text = "";
                    yield break;
                }
                StartCoroutine(IE());
            }

            meicuResult.SetState(UIMeicu.State.Regret);
            resultObj.SetActive(true);
            SetText("", force:true);
            btnStart.transform.GetComponentInChildren<Text>().text = "スタート";
        }

        void ProcPlayerLose()
        {
            AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Lose);

            textResult.text = "どうだっ！僕の勝ちだよ！";

            IEnumerator IE()
            {
                textResultQuit.text = "";
                yield break;
            }
            StartCoroutine(IE());

            // Back to Stage 1 or not
            if (Config.levelSettings[MeiPrefs.level - 1].retryOnFail)
            {}
            else
            {
                stage = 1;
            }

            meicuResult.SetState(UIMeicu.State.Laugh);
            resultObj.SetActive(true);
            SetText("", force:true);
            btnStart.transform.GetComponentInChildren<Text>().text = "リトライ";
        }

        void ProcDraw()
        {
            AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Draw);

            textResult.text = "うわっ、まちがえた･･･\n引き分けだね、もういっかいやろう";
            IEnumerator IE()
            {
                textResultQuit.text = "";
                yield break;
            }
            StartCoroutine(IE());

            meicuResult.SetState(UIMeicu.State.Dull);
            resultObj.SetActive(true);
            SetText("", force:true);
            btnStart.transform.GetComponentInChildren<Text>().text = "リトライ";
        }


        #region ======== UI Callbacks ========
        public void OnBtnStart()
        {
            bool keepQuest = MeiPrefs.level < 3 && game.inGame;

            btnStart.gameObject.SetActive(false);

            var levelSetting = Config.levelSettings[MeiPrefs.level - 1];
            var stageSetting = levelSetting.stageSettings[stage-1];

            uiQuest.Reset();
            uiBoard.Reset();

            AIController.ins.intervalBegin = stageSetting.intervelBegin;
            AIController.ins.intervalEnd = stageSetting.intervalEnd;

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
                SetTextIntro();
            }
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
                SetText("スタートにタッチしてゲームスタート！\n\n僕がいても強引にタッチしていいよ。", minDuration:0);
                uiBoard.ShowKomaA(new Vector2Int(4, 4));
                uiBoard.ShowKomaP(new Vector2Int(4, 4));

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
                SetText("キミのキューブを手に持って！\n僕はスタートマスに移動するね！", minDuration:0);
            }
        }

        void OnGameStarted(int countDown)
        {
            if (countDown > 0)
            {
                uiQuest.ShowQuest(game.quest);
                uiBoard.ShowGoal(new Vector2Int(game.quest.goalRow, game.quest.goalCol));
                SetText($"　　　　　　　<size=120>{countDown}</size>", force:true);

                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.StartCount);
            }
            else if (countDown == 0)
            {
                Debug.Log("Game Started");
                SetText("　　　　<size=32>ゲームスタート！</size>\n\nお題の順番に合わせて\nキューブを動かそう！", minDuration:1f, force:true);

                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Start);

                if (MeiPrefs.level < 3)
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
                ProcPlayerWin();
            }
            else if (stateA == Game.PlayerState.Win)
            {
                Debug.Log("PageBattle.OnGameOver: AI Win");
                ProcPlayerLose();
            }
            else
            {
                Debug.Log("PageBattle.OnGameOver: Both Fail");
                ProcDraw();
            }
        }
        void OnGameOverP(Game.PlayerState stateP)
        {
            if (stateP == Game.PlayerState.Fail || stateP == Game.PlayerState.Draw)
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
            if (stateA == Game.PlayerState.Fail || stateA == Game.PlayerState.Draw)
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


        void OnAIHeatmap()
        {
            if (swHint.isOn)
                uiBoard.ShowHeatmap(AIController.ins.Heatmap);
        }
        void OnAIThink(int phase)
        {
            if (phase == 1)
            {
                var content = Random.Range(0f, 1f) < 0.5f? "考え中…" : "次は…どっちだろう？";
                SetText(content, wait:false);
            }
            else if (phase == 2)
            {
                var content = Random.Range(0f, 1f) < 0.5f? "そっちか！" : "わかったぞ！";
                SetText(content, wait:false);
            }
        }
    }

}
