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
                game.startCallback += OnGameStarted;
                game.overCallbacik += OnGameOver;
                game.stepCallbackP += OnGameStepP;
                game.stepCallbackA += OnGameStepA;

                stage = 1;
                btnStart.transform.GetComponentInChildren<Text>().text = "Start";
                UpdateStageText();
                UpdateHint();
                LoadLevel();
            }
            else
            {
                game.StopGame();
                AIController.ins.heatmapCallback -= OnHeatmapCallback;
                game.initedCallback -= OnGameInited;
                game.startCallback -= OnGameStarted;
                game.overCallbacik -= OnGameOver;
                game.stepCallbackP -= OnGameStepP;
                game.stepCallbackA -= OnGameStepA;
            }
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
                btnStart.transform.GetComponentInChildren<Text>().text = "Next Lv.";
                text.text = "君の勝ちだ！\n\n次のレベルに行こう！";
            }
            else
            {
                StageUp();
                btnStart.transform.GetComponentInChildren<Text>().text = "Next Stage";
                text.text = "君の勝ちだ！\n\n次のステージに行こう！";
            }
            UpdateStageText();
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
            btnStart.transform.GetComponentInChildren<Text>().text = "Restart";
            UpdateStageText();
        }

        void ProcDraw()
        {
            text.text = "引き分けだな、もうっかいやろう";
            btnStart.transform.GetComponentInChildren<Text>().text = "Again";
        }

        #region ======== UI Callbacks ========
        public void OnBtnStart()
        {
            var lv = Config.levelSettings[MeiPrefs.level - 1];
            var st = lv.stageSettings[stage-1];

            uiQuest.Reset();
            uiBoard.Reset();

            AIController.ins.intervalBegin = st.intervelBegin;
            AIController.ins.intervalEnd = st.intervalEnd;

            game.InitGame(lv.questSize);
            game.StartGame();

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

        void OnGameStarted(int countDown)
        {
            if (countDown > 10)
            {
                text.text = "ゲーム開始中...\n\nキューブをスタートに置いてください。";
            }
            else if (countDown > 0)
            {
                uiBoard.ShowKomaA(new Vector2Int(4, 4));
                uiBoard.ShowKomaP(new Vector2Int(4, 4));
                text.text = $"ゲーム開始まで...\n\n{countDown}";
            }
            else if (countDown == 0)
            {
                Debug.Log("Game Started");
                uiQuest.ShowQuest(game.quest);
                uiBoard.ShowGoal(new Vector2Int(game.quest.goalRow, game.quest.goalCol));
                text.text = "ゲーム開始!";
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

            uiQuest.ShowP(traj.Length);
        }

        void OnGameStepA(Env.Response res)
        {
            var pos = game.GetPosA();
            Debug.Log($"AI steps at {pos.x}, {pos.y}");

            var traj = game.GetTrajA();
            uiBoard.ShowTrajA(traj);
            uiBoard.ShowKomaA(pos);

            uiQuest.ShowA(traj.Length);
        }
        #endregion


        void OnHeatmapCallback()
        {
            if (isHint)
                uiBoard.ShowHeatmap(AIController.ins.Heatmap);
        }
    }

}
