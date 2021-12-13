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
                AIController.ins.heatmapCallback += OnHeatmapCallback;
                game.initedCallback += OnGameInited;
                game.readyCallback += OnGameReady;
                game.startCallback += OnGameStarted;
                game.overCallback += OnGameOver;
                game.stepCallbackP += OnGameStepP;
                game.stepCallbackA += OnGameStepA;

                stage = 1;

                text.text = "";
                btnStart.gameObject.SetActive(true);
                btnStart.transform.GetComponentInChildren<Text>().text = "Start";

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
                AIController.ins.heatmapCallback -= OnHeatmapCallback;
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
            textStage.text = $"Lv.{MeiPrefs.level} - Stage {stage}";
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
                LevelUp();
                btnStart.gameObject.SetActive(true);
                btnStart.transform.GetComponentInChildren<Text>().text = "Next Lv.";
                text.text = "君の勝ちだ！\n\n次のレベルに行こう！";
            }
            else
            {
                StageUp();
                btnStart.gameObject.SetActive(true);
                btnStart.transform.GetComponentInChildren<Text>().text = "Next Stage";
                text.text = "君の勝ちだ！\n\n次のステージに行こう！";
            }
        }

        void ProcPlayerLose()
        {
            text.text = "私の勝ちだ！\n\nやり直しな";

            if (Config.levelSettings[MeiPrefs.level - 1].retryOnFail)
            {}
            else
            {
                stage = 1;
            }
            btnStart.gameObject.SetActive(true);
            btnStart.transform.GetComponentInChildren<Text>().text = "Retry";
        }

        void ProcDraw()
        {
            text.text = "引き分けだな、もうっかいやろう";
            btnStart.gameObject.SetActive(true);
            btnStart.transform.GetComponentInChildren<Text>().text = "Retry";
        }

        #region ======== UI Callbacks ========
        public void OnBtnStart()
        {
            btnStart.gameObject.SetActive(false);

            var lv = Config.levelSettings[MeiPrefs.level - 1];
            var st = lv.stageSettings[stage-1];

            uiQuest.Reset();
            uiBoard.Reset();

            AIController.ins.intervalBegin = st.intervelBegin;
            AIController.ins.intervalEnd = st.intervalEnd;

            game.InitGame(lv.questSize);
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
                text.text = "スタート位置にタッチするとゲーム始まるよ。";
                uiBoard.ShowKomaA(new Vector2Int(4, 4));
                uiBoard.ShowKomaP(new Vector2Int(4, 4));

                IEnumerator Wait2Start()
                {
                    yield return new WaitUntil(() => Device.ID2SpaceCoord(Device.cubes[0].x, Device.cubes[0].y) == new Vector2Int(4, 4) && Device.cubes[0].isGrounded); // TODO move to PlayerCon
                    AudioPlayer.ins.PlaySE(AudioPlayer.ESE.StartConfirm);
                    yield return new WaitForSecondsRealtime(0.3f);
                    game.StartGame();
                }
                StartCoroutine(Wait2Start());
            }
            else
            {
                text.text = "自分のキューブを手に持ってくださいね。";
            }
        }

        void OnGameStarted(int countDown)
        {
            if (countDown > 0)
            {
                uiQuest.ShowQuest(game.quest);
                uiBoard.ShowGoal(new Vector2Int(game.quest.goalRow, game.quest.goalCol));
                text.text = $"ゲーム開始まで...\n\n{countDown}";

                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.StartCount);
            }
            else if (countDown == 0)
            {
                Debug.Log("Game Started");
                text.text = "ゲーム開始!";

                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Start);
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
        }

        void OnGameStepA(Env.Response res)
        {
            var pos = game.GetPosA();
            Debug.Log($"AI steps at {pos.x}, {pos.y}");

            var traj = game.GetTrajA();
            uiBoard.ShowTrajA(traj);
            uiBoard.ShowKomaA(pos);

            uiQuest.ShowA(traj.Length - (Env.IsResponseFail(res)?1:0));
        }
        #endregion


        void OnHeatmapCallback()
        {
            if (isHint)
                uiBoard.ShowHeatmap(AIController.ins.Heatmap);
        }
    }

}
