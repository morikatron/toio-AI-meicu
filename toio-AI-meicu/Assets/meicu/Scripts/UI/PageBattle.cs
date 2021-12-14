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

                text.text = "それじゃぁ、僕とたたかってみよう！\n最初は、僕もまだまだ弱いけど…";
                btnStart.gameObject.SetActive(true);
                btnStart.transform.GetComponentInChildren<Text>().text = "スタート";

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
        void StageUp()
        {
            stage += 1;
        }

        void UpdateStageText()
        {
            textStage.text = $"レベル {MeiPrefs.level} - ステージ {stage}";
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
            if (stage == Config.levelSettings[MeiPrefs.level - 1].nStages)
            {
                if (MeiPrefs.level == Config.nLevels)
                {
                    text.text = "おめでとう！すべてクリアだよ！\n今の僕はキミにかなわない…\n僕ももっと「学習」して、もっと強くなったら、また勝負してね！";
                }
                else
                {
                    text.text = $"おめでとう！レベル {MeiPrefs.level} クリアだよ！";
                }
                LevelUp();
                btnStart.gameObject.SetActive(true);
                btnStart.transform.GetComponentInChildren<Text>().text = "つぎ";
            }
            else
            {
                StageUp();
                btnStart.gameObject.SetActive(true);
                btnStart.transform.GetComponentInChildren<Text>().text = "つぎ";
                text.text = "おめでとう！きみの勝ち－！！次は負けないぞ！";
            }
        }

        void ProcPlayerLose()
        {
            text.text = "どうだっ！僕の勝ちだよ！";

            if (Config.levelSettings[MeiPrefs.level - 1].retryOnFail)
            {}
            else
            {
                stage = 1;
            }
            btnStart.gameObject.SetActive(true);
            btnStart.transform.GetComponentInChildren<Text>().text = "リトライ";
        }

        void ProcDraw()
        {
            text.text = "うわっ、まちがえた･･･\n引き分けだね、もういっかいやろう";
            btnStart.gameObject.SetActive(true);
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
                    AudioPlayer.ins.PlaySE(AudioPlayer.ESE.StartConfirm);
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
                text.text = $"{countDown}";

                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.StartCount);
            }
            else if (countDown == 0)
            {
                Debug.Log("Game Started");
                text.text = "ゲームスタート！\n上の順番に合わせてキューブを動かそう！";

                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Start);

                if (MeiPrefs.level < 3 && game.inGame)
                {
                    btnStart.gameObject.SetActive(false);
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
