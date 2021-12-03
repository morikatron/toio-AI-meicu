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


        internal void SetActive(bool active)
        {
            if (ui.activeSelf == active) return;
            ui.SetActive(active);

            phase = 0;

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

                Refresh();
                MeiPrefs.SetTutorialCleared();  // TODO
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
            if (phase == 8 || phase == 9)
                PlayerController.ins.isPause = false;
            Refresh();
        }

        public void OnBtnNext()
        {
            NextPhase();
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
                phase = 11;
            else if (state == Game.PlayerState.Fail)
            {
                phase = 10;
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

            if (phase == 8)
            {
                if (traj.Length == 4)
                {
                    if (pos == new Vector2Int(2,6) || pos == new Vector2Int(3,7))
                        phase = 9;
                    else
                    {
                        phase = 10;
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
                phase ++;
            }
            else if (phase == 11)
            {
                PageManager.SetPage(PageManager.EPage.Title);
            }
            else if (phase == 7)
            {
                PlayerController.ins.isPause = false;
                phase ++;
            }
            else if (phase == 10)
            {
                phase = 8;
                PlayerController.ins.isPause = false;
                game.RetryP();
                uiBoard.ShowKomaP(4, 4);
                uiBoard.HideTrajP();
                uiQuest.ShowP(0);
            }
            else
                phase ++;

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
                if (!MeiPrefs.isTutorialCleared)
                    UIFinger.PointAt(btnNext.transform, 30);

                text.text = "このゲームは、どちらが早くゴールにたどりつけるかのゲームだよ！";
            }
            else if (phase == 1)
            {
                UIFinger.Hide();

                text.text = "真ん中のマスが「スタート」、はたのマスが「ゴール」で…";    // TODO はた?
                // TODO 明示
            }
            else if (phase == 2)
            {
                text.text = "指定された色を順番に通ってゴールするのがルールだよ！";
                // TODO 明示
            }
            else if (phase == 3)
            {
                text.text = "スタートからゴールまでの道は、1つとはかぎらないよ。";
                // TODO 明示
            }
            else if (phase == 4)
            {
                text.text = "また、同じマスは2度通ってはダメだから注意してね！";
                // TODO 明示
            }

            else if (phase == 5)
            {
                text.text = "それでは、まずは僕が動きながら説明するね。オレンジ色のキューブが僕だよ。";
            }
            else if (phase == 6)
            {
                text.text = "青色がキミのキューブだけど、今はいったん手に持っておいてね。";
                yield return new WaitUntil(()=>!Device.cubes[0].isGrounded);
                yield return new WaitForSecondsRealtime(0.5f);
                AIController.ins.Move2Center();
            }
            else if (phase == 7)
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
            else if (phase == 8)
            {
                text.text = "あなたのキューブでマットをタッチしてみよう！\n マットには1秒以上タッチしてください。成功すると音が鳴ります。";
                yield break;
            }
            else if (phase == 9)
            {
                text.text = "よし！じゃぁゴールしてみて！";
                yield break;
            }
            else if (phase == 10)
            {
                text.text = "あれれ？僕の動きをよく見て。もう一度最初からやってみるね。";
            }
            else if (phase == 11)
            {
                text.text = "ゴール！\n問題の色の順番通りにゴールまでの道を探す、というルールが分かったかな？\n「バトル」では僕も本気でたたかうからね！";
                MeiPrefs.SetTutorialCleared();
            }

            yield return new WaitForSecondsRealtime(0.1f);
            btnNext.interactable = true;
        }
    }

}
