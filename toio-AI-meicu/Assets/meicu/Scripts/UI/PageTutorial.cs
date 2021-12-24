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
        public Button btnBack;
        public UISwitch btnBGM;
        public Transform indicators;

        private int phase = 0;
        private Quest demoQuest = new Quest(4, 4, new Env.Space[]{Env.Space.G, Env.Space.G, Env.Space.B}, 3, 6, null);
        private Quest tryQuest = new Quest(4, 4, new Env.Space[]{Env.Space.Y, Env.Space.R, Env.Space.G}, 6, 5, null);


        internal void SetActive(bool active)
        {
            if (ui.activeSelf == active) return;
            ui.SetActive(active);

            phase = 0;

            if (active)
            {
                btnBGM.isOn = AudioPlayer.ins.isBGMOn;

                uiQuest.Reset();
                uiBoard.Reset();
                game.initedCallback += OnGameInited;
                game.startCallback += OnGameStarted;
                game.overCallbackP += OnGameOverP;
                game.overCallbackA += OnGameOverA;
                game.stepCallbackP += OnGameStepP;
                game.stepCallbackA += OnGameStepA;

                // Pause controllers
                PlayerController.ins.isPause = true;
                AIController.ins.isPause = true;

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

                SetIndicator("Start", false);
            }
        }

        internal void Pause()
        {
            PlayerController.ins.isPause = true;
        }

        internal void Resume()
        {
            if (phase >= 13 && phase <= 18)
                PlayerController.ins.isPause = false;
            Refresh();
        }

        public void OnBtnNext()
        {
            NextPhase();
        }

        public void OnBtnBack()
        {
            LastPhase();
        }

        public void OnBtnBGM()
        {
            AudioPlayer.ins.isBGMOn = btnBGM.isOn;
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
            if (state == Game.PlayerState.Win)
            {
                PlayerController.ins.PerformHappy();
                phase = 20;
            }
            else if (state == Game.PlayerState.Fail)
            {
                AIController.ins.PerformSad();
                phase = 19;
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

            if (phase == 14)
            {
                if (!Env.IsResponseFail(res))
                {
                    if (pos.x == 4 && pos.y == 3)
                        phase = 15;
                    else
                        phase = 19;
                    Refresh();
                }
            }
            else if (phase == 15)
            {
                if (!Env.IsResponseFail(res))
                {
                    if (pos.x == 5 && pos.y == 3)
                        phase = 16;
                    else
                        phase = 19;
                    Refresh();
                }
            }
            else if (phase == 16)
            {
                if (!Env.IsResponseFail(res))
                {
                    if (pos.x == 6 && pos.y == 3)
                        phase = 17;
                    else
                        phase = 19;
                    Refresh();
                }
            }
            else if (phase == 17)
            {
                if (!Env.IsResponseFail(res))
                {
                    if (pos.x == 6 && pos.y == 4)
                        phase = 18;
                    else
                        phase = 19;
                    Refresh();
                }
            }

            if (phase == 19)
                PlayerController.ins.isPause = true;
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
            if (phase == 22)
            {
                PageManager.SetPage(PageManager.EPage.Title);
            }
            else if (phase == 9)
            {
                game.InitGame(tryQuest);
                game.StartGame();
                PlayerController.ins.isPause = true;
                phase ++;
            }
            else if (phase == 19)   // failed to retry
            {
                phase = 13;
                game.RetryP();
                uiBoard.ShowKomaP(4, 4);
                uiBoard.HideTrajP();
                uiQuest.ShowP(0);
            }
            else
                phase ++;

            Refresh();
        }

        void LastPhase()
        {
            if (phase == 0) {}
            else if (phase > 9) {}
            else
                phase --;
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
            btnBack.interactable = false;

            var indicators = ui.transform.Find("Indicators");

            if (phase == 0)
            {
                // Show demo quest
                game.InitGame(demoQuest);

                // Highlight next button for first time
                if (!Prefs.isTutorialCleared)
                    UIFinger.PointAt(btnNext.transform, 30);

                // Inactivate all indicators
                SetIndicator("Start", false);
                text.text = "こんにちは！ボクは「迷キュー」";
            }
            else if (phase == 1)
            {
                // Hide finger
                UIFinger.Hide();

                SetIndicator("Start", false);
                text.text = "このゲームは…";
            }
            else if (phase == 2)
            {
                SetIndicator("Start");
                text.text = "真ん中のマスの「スタート」から";
            }
            else if (phase == 3)
            {
                SetIndicator("Goal");
                text.text = "はたが立っている「ゴール」のマスまで";
            }
            else if (phase == 4)
            {
                SetIndicator("Start", false);
                text.text = "早く、正しい道を見つけるゲームだよ！";
            }
            else if (phase == 5)
            {
                SetIndicator("Start", false);
                text.text = "そして、大切なルール…";
            }
            else if (phase == 6)
            {
                SetIndicator("Quest");
                text.text = "ここを見て！\nこの「問題」の決まった色の通りに\n順番に通ってゴールしなきゃいけないんだ！";
            }
            else if (phase == 7)
            {
                SetIndicator("Quest");
                text.text = "ボクとキミと、どちらが早くゴールに\nたどりつけるか、勝負だよ！";
            }

            else if (phase == 8)
            {
                SetIndicator("Paths");
                text.text = "スタートからゴールまでの道は、\n1つとはかぎらないよ。";
            }
            else if (phase == 9)
            {
                game.InitGame(demoQuest);

                SetIndicator("Passed");
                text.text = "また、同じマスは2度通っては\nダメだから注意してね！";
            }

            else if (phase == 10)
            {
                SetIndicator("Start", false);
                text.text = "それでは、\nまずはボクが動きながら説明するね。\n\nオレンジ色のキューブがボクだよ。";
            }
            else if (phase == 11)
            {
                text.text = "裏のランプが青色に光っているのがキミのキューブだけど、\n今はいったん手に持っておいてね。\n（青キューブを持ち上げてください）";
                yield return new WaitUntil(() => PlayerController.ins.isConnected && !PlayerController.ins.isGrounded);
                AIController.ins.Move2Center();
                yield return new WaitForSecondsRealtime(0.5f);
                yield return new WaitUntil(() => AIController.ins.IsAtCenter);
                text.text = "では動いてみるよ！";
            }
            else if (phase == 12)
            {
                text.text = "まずは「きいろ」";
                AIController.ins.RequestMove(Env.Action.Left);
                yield return new WaitUntil(() => !AIController.ins.isMoving);
                yield return new WaitForSecondsRealtime(1f);

                text.text = "つぎは「しろ」...";
                AIController.ins.RequestMove(Env.Action.Down);
                yield return new WaitUntil(() => !AIController.ins.isMoving);
                yield return new WaitForSecondsRealtime(1f);

                text.text = "つぎは「あか」...";
                AIController.ins.RequestMove(Env.Action.Down);
                yield return new WaitUntil(() => !AIController.ins.isMoving);
                yield return new WaitForSecondsRealtime(1f);

                text.text = "また「しろ」...";
                AIController.ins.RequestMove(Env.Action.Right);
                yield return new WaitUntil(() => !AIController.ins.isMoving);
                yield return new WaitForSecondsRealtime(1f);

                text.text = "つぎの「みどり」でゴールだけど、\nボクはここで待ってるから、\nキミも同じように\nキューブを動かしてみて！";
                yield return new WaitForSecondsRealtime(0.1f);
            }

            else if (phase == 13)
            {
                text.text = "まずは「スタート」のマスに、\nキミのキューブでタッチしてみて！\n「ピコン」と音が鳴ったらOKだよ！";
                yield return new WaitUntil(() => PlayerController.ins.IsAtCenter);
                AudioPlayer.ins.PlaySE(AudioPlayer.ESE.StartConfirmed);

                PlayerController.ins.isPause = false;
                phase = 14;
                Refresh();
                yield break;
            }
            else if (phase == 14)
            {
                text.text = "次は「きいろ」のマスをタッチするよ。\nボクが進んだ道をよく見てタッチして！\n\n「プープープープープー・ピッ」\nという音が鳴り終わったらOKだよ。";
                yield break;
            }
            else if (phase == 15)
            {
                text.text = "次は「しろ」のマスに移動してみて！";
                yield break;
            }
            else if (phase == 16)
            {
                text.text = "そうそう！次は「あか」だよ！\nボクが通った道をよく見てね！";
                yield break;
            }
            else if (phase == 17)
            {
                text.text = "そして「しろ」。\n同じマスにならぶよ！\n\n同じマスにタッチしにくい場合は、ボクを少しずらしてね！";
                yield break;
            }
            else if (phase == 18)
            {
                text.text = "さぁ！つぎは「みどり」。\nゴールだ！";
                yield break;
            }

            else if (phase == 19)
            {
                text.text = "あれれ？ボクの動きをよく見て。もう一度最初からやってみてね。";
            }
            else if (phase == 20)
            {
                text.text = "ゴール！\n問題の色の順番通りにゴールまでの道を探す、というルールが分かったかな？";
                Prefs.SetTutorialCleared();
            }

            else if (phase == 21)
            {
                text.text = "遊び方をまた見たくなったら\n「チュートリアル」をえらんでね！";
            }
            else if (phase == 22)
            {
                text.text = "それじゃぁ、\nボクと「バトル」で勝負しよう！";
            }

            yield return new WaitForSecondsRealtime(0.1f);
            btnNext.interactable = true;

            if (phase > 0 && phase < 10)
                btnBack.interactable = true;
        }


        void SetIndicator(string name, bool active = true, bool hideOther = true)
        {
            var tar = indicators.Find(name);

            if (hideOther)
            for (int i=0; i < indicators.childCount; i++)
            {
                var ind = indicators.GetChild(i);
                if (ind != tar )
                    ind.gameObject.SetActive(false);
            }

            tar?.gameObject.SetActive(active);
        }
    }

}
