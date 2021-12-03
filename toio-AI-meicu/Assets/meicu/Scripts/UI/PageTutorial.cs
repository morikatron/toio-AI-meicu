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
        public Text textPhase;
        public Button btnNext;

        int phase = 0;
        int dialog = 0;


        internal void SetActive(bool active)
        {
            if (ui.activeSelf == active) return;
            ui.SetActive(active);

            phase = 0;
            dialog = 0;

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

                PlayerController.ins.isPause = true;
                AIController.ins.isPause = true;

                InitGame();

                MeiPrefs.SetTutorialCleared();  // TODO
                Refresh();
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

                PlayerController.ins.isPause = false;
                AIController.ins.isPause = false;
            }
        }

        internal void Pause()
        {
            PlayerController.ins.isPause = true;
        }

        internal void Resume()
        {
            if (phase == 1 && (dialog == 3 || dialog == 4))
                PlayerController.ins.isPause = false;
            Refresh();
        }

        public void OnBtnNext()
        {
            NextPhase();
        }

        public void OnBtnDialogNext()
        {
            NextDialog();
        }


        void InitGame()
        {
            Env.Space[] colors = {Env.Space.G, Env.Space.G, Env.Space.B};
            MeiQuest quest = new MeiQuest(4, 4, colors, 3, 6, null);
            game.InitGame(quest);
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
                Refresh();
        }

        void OnGameOverP(Game.PlayerState state)
        {
            if (state == Game.PlayerState.Success)
                dialog = 6;
            else if (state == Game.PlayerState.Fail)
            {
                dialog = 5;
                PlayerController.ins.isPause = true;
            }
            Refresh();
        }

        void OnGameOverA(Game.PlayerState state)
        {
        }

        void OnGameStepP(Env.Response res)
        {
            var pos = game.GetPosP();
            var traj = game.GetTrajP();
            uiBoard.ShowTrajP(traj);
            uiBoard.ShowKomaP(pos);
            uiQuest.ShowP(traj.Length);

            if (dialog == 3)
            {
                if (traj.Length == 4)
                {
                    if (pos == new Vector2Int(2,6) || pos == new Vector2Int(3,7))
                        dialog = 4;
                    else
                    {
                        dialog = 5;
                        PlayerController.ins.isPause = true;
                    }
                }
                Refresh();
            }
        }

        void OnGameStepA(Env.Response res)
        {
            var pos = game.GetPosA();
            var traj = game.GetTrajA();
            uiBoard.ShowTrajA(traj);
            uiBoard.ShowKomaA(pos);
            uiQuest.ShowA(traj.Length);
        }
        #endregion


        void NextPhase()
        {
            if (phase == 0)
            {
                game.StartGame();
                phase = 1;
                dialog = 0;
            }
            else if (phase == 1)
            {
                PageManager.SetPage(PageManager.EPage.Title);
            }

            Refresh();
        }

        void NextDialog()
        {
            if (phase == 0)
            {
                dialog ++;
            }
            else if (phase == 1)
            {
                if (dialog < 2)
                {
                    dialog ++;
                }
                else if (dialog == 2)
                {
                    dialog = 3;
                    PlayerController.ins.isPause = false;
                }
                else if (dialog == 5)
                {
                    dialog = 3;
                    PlayerController.ins.isPause = false;
                    game.RetryP();
                    uiBoard.ShowKomaP(4, 4);
                    uiBoard.HideTrajP();
                    uiQuest.ShowP(0);
                }
            }

            Refresh();
        }

        void Refresh()
        {
            StopAllCoroutines();
            StartCoroutine(IE_Refresh());
        }

        IEnumerator IE_Refresh()
        {
            btnNext.interactable = false;

            if (phase == 0)
            {
                textPhase.text = "1. ゲームの紹介";

                if (dialog == 0)
                {
                    text.text = "このゲームは、どちらが早くゴールにたどりつけるかのゲームだよ！";
                }
                else if (dialog == 1)
                {
                    text.text = "真ん中のマスが「スタート」、はたのマスが「ゴール」で…";    // TODO はた?
                    // TODO 明示
                }
                else if (dialog == 2)
                {
                    text.text = "指定された色を順番に通ってゴールするのがルールだよ！";
                    // TODO 明示
                }
                else if (dialog == 3)
                {
                    text.text = "スタートからゴールまでの道は、1つとはかぎらないよ。";
                    // TODO 明示
                }
                else if (dialog == 4)
                {
                    text.text = "また、同じマスは2度通ってはダメだから注意してね！";
                    // TODO 明示
                    btnNext.interactable = true;
                }
            }
            else if (phase == 1)
            {
                textPhase.text = "2. あそび方の紹介";

                if (dialog == 0)
                {
                    text.text = "それでは、まずは僕が動きながら説明するね。オレンジ色のキューブが僕だよ。";
                }
                else if (dialog == 1)
                {
                    text.text = "青色がキミのキューブだけど、今はいったん手に持っておいてね。";
                    yield return new WaitUntil(()=>!Device.cubes[0].isGrounded);
                    yield return new WaitForSecondsRealtime(0.5f);
                    AIController.ins.Move2Center();
                }
                else if (dialog == 2)
                {
                    text.text = "まずは（みどり）";
                    AIController.ins.RequestMove((Env.Action)1);    // right
                    yield return new WaitUntil(()=>!AIController.ins.isMoving);
                    yield return new WaitForSecondsRealtime(1f);

                    text.text = "つぎは（しろ）...";
                    AIController.ins.RequestMove((Env.Action)1);    // right
                    yield return new WaitUntil(()=>!AIController.ins.isMoving);
                    yield return new WaitForSecondsRealtime(1f);

                    text.text = "つぎは（みどり）...";
                    AIController.ins.RequestMove((Env.Action)1);    // right
                    yield return new WaitUntil(()=>!AIController.ins.isMoving);
                    yield return new WaitForSecondsRealtime(1f);

                    text.text = "また（しろ）...";
                    AIController.ins.RequestMove((Env.Action)0);    // up
                    yield return new WaitUntil(()=>!AIController.ins.isMoving);
                    yield return new WaitForSecondsRealtime(1f);

                    text.text = "つぎの（あお）でゴールだけど、僕はここで待ってるから、キミも同じようにキューブを動かしてみて！";
                    yield return new WaitForSecondsRealtime(0.1f);

                }
                else if (dialog == 3)
                {
                    text.text = "あなたのキューブでマットをタッチしてみよう！\n マットには1秒以上タッチしてください。成功すると音が鳴ります。";
                }
                else if (dialog == 4)
                {
                    text.text = "よし！じゃぁゴールしてみて！";
                }
                else if (dialog == 5)
                {
                    text.text = "あれれ？僕の動きをよく見て。もう一度最初からやってみるね。";
                }
                else if (dialog == 6)
                {
                    text.text = "ゴール！\n問題の色の順番通りにゴールまでの道を探す、というルールが分かったかな？\n「バトル」では僕も本気でたたかうからね！";
                    MeiPrefs.SetTutorialCleared();
                    btnNext.interactable = true;
                }
            }
        }
    }

}
