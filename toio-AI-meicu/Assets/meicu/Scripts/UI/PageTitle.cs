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

                // Only on App opened, make self introduction
                if (!isHiSaid)
                {
                    text.text = "こんにちは！\nボクの名前は「迷キュー」";
                    yield return new WaitForSecondsRealtime(2f);
                    isHiSaid = true;
                }
                text.text = "キューブの電源を入れて\n「接続」ボタンから接続してね！";

                // Guide to click btnConnect, on first time opening App
                if (!Prefs.isEverConnected)
                    UIFinger.PointAt(btnConnect.transform, biasX:70);
                else
                    UIFinger.Hide();
            }
            // One cube connected
            else if (Device.nConnected == 1)
            {
                // Only btnConnect available
                btnConnect.interactable = true;
                btnTutorial.interactable = false;
                btnBattle.interactable = false;
                btnLearn.interactable = false;

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

                    if (!Prefs.isTutorialAccessed)
                    {
                        // Guide to click btnTutorial
                        UIFinger.PointAt(btnTutorial.transform, biasX:130);

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
                            UIFinger.PointAt(btnBattle.transform, biasX:130);

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

                    // Learn NOT cleared
                    if (!Prefs.isLearnCleared)
                    {
                        // Guide to Learn
                        UIFinger.PointAt(btnLearn.transform, biasX:130);

                        while (true)
                        {
                            text.text = "レベル1のクリア、おめでとう！";
                            yield return new WaitForSecondsRealtime(2f);
                            text.text = "かいせつボタンを押すと、\nボクの強さのひみつが分かるよ";
                            yield return new WaitForSecondsRealtime(2f);
                        }
                    }
                    // Learn Cleared
                    else
                    {
                        // Right after Learn cleared
                        if (!Prefs.isBattleEnteredAfterLearn)
                        {
                            // Guide to Battle again
                            UIFinger.PointAt(btnBattle.transform, biasX:130);
                        }

                        while (true)
                        {
                            text.text = "みんなは迷路が好きかな？";
                            yield return new WaitForSecondsRealtime(2f);
                            text.text = "ボクと迷路パズルでバトルしよう！";
                            yield return new WaitForSecondsRealtime(2f);
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

        public void OnBtnBGM()
        {
            AudioPlayer.ins.isBGMOn = btnBGM.isOn;
        }
    }

}
