using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace toio.AI.meicu
{

    public class PageTutorial : MonoBehaviour
    {
        public GameObject ui;
        public UIBoard uiBoard;
        public UIQuest uiQuest;
        public Game game;
        public Text text;
        public Button btnNext;

        int phase = 0;
        bool AIMoved = false;


        internal void SetActive(bool active)
        {
            if (ui.activeSelf == active) return;
            ui.SetActive(active);

            phase = 0;
            AIMoved = false;

            if (active)
            {
                uiQuest.Reset();
                uiBoard.Reset();
                game.initedCallback += OnGameInited;
                game.startCallback += OnGameStarted;
                game.overCallbackP += OnGameOverP;
                game.overCallbackA += OnGameOverA;
                game.stepCallbackP += OnGameStepP;
                game.stepCallbackA += OnGameStepA;
                game.InitGame(3);

                RefreshUI();
            }
            else
            {
                game.StopGame();
                game.initedCallback -= OnGameInited;
                game.startCallback -= OnGameStarted;
                game.overCallbackP -= OnGameOverP;
                game.overCallbackA -= OnGameOverA;
                game.stepCallbackP -= OnGameStepP;
                game.stepCallbackA -= OnGameStepA;
            }
        }

        public void OnBtnHome()
        {
                    MeiPrefs.SetTutorialCleared();
            PageManager.SetPage(PageManager.EPage.Title);
        }

        public void OnBtnNext()
        {
            NextPhase();
        }


        #region ======== Game Callbacks ========
        void OnGameInited()
        {
            uiQuest.ShowQuest(game.quest);
            uiBoard.ShowGoal(new Vector2Int(game.quest.goalRow, game.quest.goalCol));
            uiBoard.ShowKomaA(new Vector2Int(4, 4));
            uiBoard.ShowKomaP(new Vector2Int(4, 4));
        }

        void OnGameStarted(int countDown)
        {
            if (countDown == 0)
                RefreshUI();
        }

        void OnGameOverP(Game.PlayerState state)
        {
            RefreshUI();
        }

        void OnGameOverA(Game.PlayerState state)
        {
            RefreshUI();
        }

        void OnGameStepP(Env.Response res)
        {
            var pos = game.GetPosP();
            var traj = game.GetTrajP();
            uiBoard.ShowTrajP(traj);
            uiBoard.ShowKomaP(pos);
            uiQuest.ShowP(traj.Length);

            RefreshUI();
        }

        void OnGameStepA(Env.Response res)
        {
            var pos = game.GetPosA();
            var traj = game.GetTrajA();
            uiBoard.ShowTrajA(traj);
            uiBoard.ShowKomaA(pos);
            uiQuest.ShowA(traj.Length);

            if (phase == 3 && traj.Length == 4 ||
                res == Env.Response.FailWrong ||
                res == Env.Response.FailOut ||
                res == Env.Response.FailPassed
            ){
                AIMoved = true;
                AIController.ins.isPause = true;
                RefreshUI();
            }
        }
        #endregion


        void NextPhase()
        {
            if (phase == 0)
            {
                game.StartGame();
                PlayerController.ins.isPause = true;
                AIController.ins.isPause = true;
                phase = 1;
            }
            else if (phase == 1)
            {
                PlayerController.ins.isPause = true;
                AIController.ins.isPause = true;
                phase = 2;
            }
            else if (phase == 2)
            {
                PlayerController.ins.isPause = true;
                AIController.ins.isPause = false;
                phase = 3;
            }
            else if (phase == 3)
            {
                PlayerController.ins.isPause = true;
                PlayerController.ins.isPause = false;
                phase = 4;
            }
            else if (phase == 4)
            {
                if (game.stateP == Game.PlayerState.Fail)
                {
                    game.RetryP();
                }
                else if (game.stateP == Game.PlayerState.Success)
                {
                    game.StopGame();
                    PageManager.SetPage(PageManager.EPage.Title);
                }
            }

            RefreshUI();
        }

        void RefreshUI()
        {
            if (phase == 0)
            {
                text.text = "中央のマスが「スタート」で、\n旗が「ゴール」です。\n\nお題の色順通りにゴールする\nのが目標です。\n\n同じマスには1回しか入れない。";
            }
            else if (phase == 1)
            {
                if (!game.inGame)
                {
                    text.text = "2つのCubeをスタートに置かれてからゲームが開始します。";
                    btnNext.interactable = false;
                }
                else
                {
                    text.text = "2つのCubeをスタートに置かれてからゲームが開始します。";
                    btnNext.interactable = true;
                }
            }
            else if (phase == 2)
            {
                text.text = "まず私の動きを見な。\n\n※念の為自分のCube（白く点灯している方）を手に持っていてください。";
            }
            else if (phase == 3)
            {
                if (!AIMoved)
                {
                    text.text = "......";
                    btnNext.interactable = false;
                }
                else
                {
                    text.text = "...あと少しだけど、\n君に譲ってあげるよ。";
                    btnNext.interactable = true;
                }
            }
            else if (phase == 4)
            {
                if (game.stateP == Game.PlayerState.InGame)
                {
                    text.text = "行きたいマスにCubeを2秒間タッチさせてください。\n\n";
                    btnNext.interactable = false;
                }
                else if (game.stateP == Game.PlayerState.Success)
                {
                    text.text = "よくやりました~\n\n卒業です。";
                    btnNext.interactable = true;
                    MeiPrefs.SetTutorialCleared();
                }
                else if (game.stateP == Game.PlayerState.Fail)
                {
                    text.text = "残念、やり直しです。";
                    btnNext.interactable = true;
                }
            }
        }
    }

}
