using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace toio.AI.meicu
{

    public class PageTitle : MonoBehaviour
    {
        public GameObject ui;
        public Button btnConnect;
        public Button btnTutorial;
        public Button btnBattle;
        public Button btnLearn;
        public Button btnTrainer;
        public UISwitch btnBGM;
        public TMP_Text text;

        private bool isHiSaid = false;
        private bool isError = false;


        internal void SetActive(bool active)
        {
            ui.SetActive(active);

            if (active)
            {
                isError = false;
                btnBGM.isOn = AudioPlayer.ins.isBGMOn;
                Refresh();
            }
            else
            {
                StopAllCoroutines();
            }
        }


        internal void Refresh()
        {
            StopAllCoroutines();
            StartCoroutine(IE_Refresh());
        }

        IEnumerator IE_Refresh()
        {
            // Update connected cube icons
            var icons = ui.transform.Find("ConnectionState");
            icons.Find("IconP").gameObject.SetActive(PlayerController.ins.isConnected);
            icons.Find("IconA").gameObject.SetActive(AIController.ins.isConnected);

            // Connection error
            if (isError && Device.nConnected < 2)
            {
                // Only btnConnect available
                btnConnect.interactable = true;
                btnTutorial.interactable = false;
                btnBattle.interactable = false;
                btnLearn.interactable = false;
                btnTrainer.interactable = false;

                text.text = "接続できない場合は\nブラウザーを再起動してみてね";

                // Guide to click btnConnect, on first time opening App
                if (!Prefs.isEverConnected)
                    UIFinger.PointAt(btnConnect.transform, biasX:70);
                else
                    UIFinger.Hide();
            }
            // No cube connected
            else if (Device.nConnected == 0)
            {
                // Only btnConnect available
                btnConnect.interactable = true;
                btnTutorial.interactable = false;
                btnBattle.interactable = false;
                btnLearn.interactable = false;
                btnTrainer.interactable = false;

                // Guide to click btnConnect, on first time opening App
                if (!Prefs.isEverConnected)
                    UIFinger.PointAt(btnConnect.transform, biasX:70);
                else
                    UIFinger.Hide();

                // Only on App opened, make self introduction
                if (!isHiSaid)
                {
                    text.text = "こんにちは！\nボクの名前は「迷キュー」";
                    yield return new WaitForSecondsRealtime(2f);
                    isHiSaid = true;
                }

                while (true)
                {
                    text.text = "キューブの電源を入れて\n「接続」ボタンから接続してね！";
                    yield return new WaitForSecondsRealtime(2f);
                    text.text = "コンソールの電源はオフにしておいてね！";
                    yield return new WaitForSecondsRealtime(2f);
                }
            }
            // One cube connected
            else if (Device.nConnected == 1)
            {
                // Only btnConnect available
                btnConnect.interactable = true;
                btnTutorial.interactable = false;
                btnBattle.interactable = false;
                btnLearn.interactable = false;
                btnTrainer.interactable = false;

                // Guide to click btnConnect, on first time opening App
                if (!Prefs.isEverConnected)
                    UIFinger.PointAt(btnConnect.transform, biasX:70);
                else
                    UIFinger.Hide();

                text.text = "もう1つ接続してね！";
            }
            // Connected over
            else if (Device.nConnected == 2)
            {
                btnConnect.interactable = false;
                UIFinger.Hide();

                // Tutorial, Battle always open
                btnTutorial.interactable = true;
                btnBattle.interactable = true;

                // Battle lv.1 NOT cleared
                if (Prefs.level == 1)
                {
                    // Learn NOT available
                    btnLearn.interactable = false;
                    btnTrainer.interactable = false;

                    if (!Prefs.isTutorialAccessed)
                    {
                        // Guide to click btnTutorial
                        UIFinger.PointAt(btnTutorial.transform, biasX:120);

                        text.text = "まずは迷路パズルのルールを\n説明するよ！";
                    }
                    else if (!Prefs.isTutorialCleared)
                    {
                        text.text = "まずは迷路パズルのルールを\n説明するよ！";
                    }
                    else
                    {
                        // Guide to click btnBattle
                        if (!Prefs.isBattleAccessed)
                            UIFinger.PointAt(btnBattle.transform, biasX:120);

                        while (true)
                        {
                            text.text = "みんなは迷路が好きかな？";
                            yield return new WaitForSecondsRealtime(2f);
                            text.text = "ボクと迷路パズルでバトルしよう！";
                            yield return new WaitForSecondsRealtime(2f);
                        }
                    }
                }
                // Battle lv.1 cleared
                else
                {
                    // Learn open
                    btnLearn.interactable = true;
                    btnTrainer.interactable = Prefs.level > 2;

                    // Learn NOT cleared
                    if (!Prefs.isLearnCleared)
                    {
                        // Guide to Learn
                        UIFinger.PointAt(btnLearn.transform, biasX:120);

                        while (true)
                        {
                            if (Prefs.level == 2)
                            {
                                text.text = "レベル1のクリア、おめでとう！";
                                yield return new WaitForSecondsRealtime(2f);
                            }
                            text.text = "かいせつボタンを押すと、\nボクの強さのひみつが分かるよ";
                            yield return new WaitForSecondsRealtime(2f);
                        }
                    }
                    // Learn Cleared BUT Trainer NOT Accessed
                    else if (Prefs.level > 2 && !Prefs.isTrainerAccessed)
                    {
                        // Guide to Trainer
                        UIFinger.PointAt(btnTrainer.transform, biasX:120);

                        while (true)
                        {
                            if (Prefs.level == 3)
                            {
                                text.text = "レベル2のクリア、おめでとう！";
                                yield return new WaitForSecondsRealtime(2f);
                            }
                            text.text = "キミだけのAIを育ててみようか！";
                            yield return new WaitForSecondsRealtime(2f);
                        }
                    }
                    else
                    {
                        // Right after Learn cleared
                        if (!Prefs.isBattleEnteredAfterLearn)
                        {
                            // Guide to Battle again
                            UIFinger.PointAt(btnBattle.transform, biasX:120);
                        }

                        while (true)
                        {
                            text.text = "みんなは迷路が好きかな？";
                            yield return new WaitForSecondsRealtime(2f);
                            text.text = "ボクと迷路パズルでバトルしよう！";
                            yield return new WaitForSecondsRealtime(2f);
                            if (Prefs.level == 2)
                            {
                                text.text = "レベルが上がると\n【チャレンジ】が解放されるよ。";
                                yield return new WaitForSecondsRealtime(2f);
                            }
                        }
                    }
                }

            }
        }

        public async void OnBtnConnect()
        {
            btnConnect.interactable = false;
            btnConnect.GetComponent<ButtonConnect>().SetBusy(true);
            UIFinger.Hide();

            var code = await Device.Connect();
            isError = code > 0;

            btnConnect.GetComponent<ButtonConnect>().SetBusy(false);

            if (Device.nConnected == 2) Prefs.SetEverConnected();

            Refresh();
        }

        public void OnBtnTutorial()
        {
            PageManager.SetPage(PageManager.EPage.Tutorial);
        }

        public void OnBtnBattle()
        {
            PageManager.SetPage(PageManager.EPage.Battle);
        }

        public void OnBtnLearn()
        {
            PageManager.SetPage(PageManager.EPage.Learn);
        }

        public void OnBtnTrainer()
        {
            PageManager.SetPage(PageManager.EPage.Trainer);
        }

        public void OnBtnBGM()
        {
            AudioPlayer.ins.isBGMOn = btnBGM.isOn;
        }
    }

}
