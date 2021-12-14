using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace toio.AI.meicu
{

    public class PageBattle : MonoBehaviour
    {
        public GameObject ui;
        public UIBoard uiBoard;
        public UIQuest uiQuest;
        public Game game;
        public Button btnStart;
        public Text text;
        public Text textStage;
        public GameObject resultObj;
        public Text textResult;
        public Text textResultQuit;
        public Text textTag;

        private bool isHint = false;
        private int stage = 1;


        internal void SetActive(bool active)
        {
            if (ui.activeSelf == active) return;
            ui.SetActive(active);

            if (active)
            {
                uiQuest.Reset();
                uiBoard.Reset();
                AIController.ins.heatmapCallback += OnAIHeatmap;
                AIController.ins.thinkCallback += OnAIThink;
                game.initedCallback += OnGameInited;
                game.readyCallback += OnGameReady;
                game.startCallback += OnGameStarted;
                game.overCallback += OnGameOver;
                game.stepCallbackP += OnGameStepP;
                game.stepCallbackA += OnGameStepA;

                stage = 1;

                btnStart.gameObject.SetActive(true);
                btnStart.transform.GetComponentInChildren<Text>().text = "スタート";
                resultObj.SetActive(false);

                UpdateTextIntro();
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


        string[] classNames = new string[]{"", "ビギナー", "ジュニア", "シニア", "プロ", "サブリーダー", "リーダー", "サブトレーナー", "トレーナー", "スター", "スーパースター", "マスター"};
        void UpdateTextIntro()
        {
            if (stage > 1)
            {
                text.text = "つぎのステージやろう!\n僕はもっと早くなるぞ！";
            }
            else if (MeiPrefs.level == 1)
            {
                text.text = "それじゃぁ、僕とたたかってみよう！\n\n最初の僕は【ビギナー】、\nまだまだ弱いけど…";
            }
            else
            {
                int n = new int[]{0, 1, 10, 50, 100, 200, 500, 700, 900, 1000, 1500, 2000}[MeiPrefs.level];
                text.text = $"僕は迷キュー\n【{classNames[MeiPrefs.level]}】だよ。\n試行錯誤を{n}万回以上したんだ。\nどうだい？勝てるかな？";

                if (MeiPrefs.level == 3)
                    text.text += "\n（ここからリセットボタン使えないから\n間違えないように気をつけてね）";
            }

            textTag.text = classNames[MeiPrefs.level];

            UpdateStageText();
        }

        void UpdateStageText()
        {
            textStage.text = $"レベル {MeiPrefs.level}/{Config.nLevels} ・ ステージ {stage}/5";
        }
        void UpdateHint()
        {
            if (this.isHint)
            {
                ui.transform.Find("BtnHint").GetComponent<Image>().color = Color.white;
                ui.transform.Find("ImgThink").gameObject.SetActive(true);
                uiBoard.ShowHeatmap(AIController.ins.Heatmap);
            }
            else
            {
                ui.transform.Find("BtnHint").GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f);
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
                        textResultQuit.text = "「かいせつ」が開放されたよ。\n見てみよう！\n\n（タップして戻る）";
                    }
                    else
                    {
                        textResultQuit.text = "（タップして戻る）";
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
                    textResultQuit.text = "（タップして戻る）";
                    yield break;
                }
                StartCoroutine(IE());
            }

            resultObj.SetActive(true);
            text.text = "";
            btnStart.transform.GetComponentInChildren<Text>().text = "スタート";
        }

        void ProcPlayerLose()
        {
            AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Lose);

            textResult.text = "どうだっ！僕の勝ちだよ！";

            IEnumerator IE()
            {
                textResultQuit.text = "（タップして戻る）";
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

            resultObj.SetActive(true);
            text.text = "";
            btnStart.transform.GetComponentInChildren<Text>().text = "リトライ";
        }

        void ProcDraw()
        {
            AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Draw);

            textResult.text = "うわっ、まちがえた･･･\n引き分けだね、もういっかいやろう";
            IEnumerator IE()
            {
                textResultQuit.text = "（タップして戻る）";
                yield break;
            }
            StartCoroutine(IE());

            resultObj.SetActive(true);
            text.text = "";
            btnStart.transform.GetComponentInChildren<Text>().text = "リトライ";
        }

        #region ======== UI Callbacks ========
        public void OnBtnStart()
        {
            bool keepQuest = MeiPrefs.level < 3 && game.inGame;

            btnStart.gameObject.SetActive(false);

            var lv = Config.levelSettings[MeiPrefs.level - 1];
            var st = lv.stageSettings[stage-1];

            uiQuest.Reset();
            uiBoard.Reset();

            AIController.ins.intervalBegin = st.intervelBegin;
            AIController.ins.intervalEnd = st.intervalEnd;

            game.InitGame(lv.questSize, keepQuest);
            game.WaitReady();

            UpdateStageText();
            UpdateHint();
        }

        public void OnBtnHint()
        {
            this.isHint = !this.isHint;
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
                UpdateTextIntro();
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
                text.text = "スタート位置にタッチするとゲームが始まるよ。\n\n僕がいても強引にタッチしていいよ。";
                uiBoard.ShowKomaA(new Vector2Int(4, 4));
                uiBoard.ShowKomaP(new Vector2Int(4, 4));

                IEnumerator Wait2Start()
                {
                    yield return new WaitUntil(() => Device.ID2SpaceCoord(Device.cubes[0].x, Device.cubes[0].y) == new Vector2Int(4, 4) && Device.cubes[0].isGrounded); // TODO move to PlayerCon
                    Debug.Log("PageBattle.OnGameReady: Touched, Call game.StartGame");
                    AudioPlayer.ins.PlaySE(AudioPlayer.ESE.StartConfirmed);
                    yield return new WaitForSecondsRealtime(0.3f);
                    game.StartGame();
                }
                StartCoroutine(Wait2Start());
            }
            else
            {
                text.text = "キミのキューブを手に持って！\n僕はスタートマスに移動するね！";
            }
        }

        void OnGameStarted(int countDown)
        {
            if (countDown > 0)
            {
                uiQuest.ShowQuest(game.quest);
                uiBoard.ShowGoal(new Vector2Int(game.quest.goalRow, game.quest.goalCol));
                text.text = $"　　　　　　　　　{countDown}";

                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.StartCount);
            }
            else if (countDown == 0)
            {
                Debug.Log("Game Started");
                text.text = "　　　　　ゲームスタート！\n\n上の順番に合わせてキューブを動かそう！";

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
            if (stateP == Game.PlayerState.Success)
            {
                Debug.Log("Player Win");
                ProcPlayerWin();
            }
            else if (stateA == Game.PlayerState.Success)
            {
                Debug.Log("AI Win");
                ProcPlayerLose();
            }
            else
            {
                Debug.Log("Both Fail");
                ProcDraw();
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
                text.text = "あっ、まちがえたね";
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
                text.text = "あっ、しまった";
            }
        }
        #endregion


        void OnAIHeatmap()
        {
            if (isHint)
                uiBoard.ShowHeatmap(AIController.ins.Heatmap);
        }
        void OnAIThink(int phase)
        {
            if (phase == 1)
            {
                text.text = Random.Range(0f, 1f) < 0.5f? "考え中…" : "次は…どっち？";
            }
            else if (phase == 2)
            {
                text.text = Random.Range(0f, 1f) < 0.5f? "そっちか！" : "わかったぞ！";
            }
        }
    }

}
