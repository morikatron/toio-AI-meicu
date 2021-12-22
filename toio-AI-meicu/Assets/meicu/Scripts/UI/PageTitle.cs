using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace toio.AI.meicu
{

    public class PageTitle : MonoBehaviour
    {
        public GameObject ui;
        public Button btnConnect;
        public Button btnTutorial;
        public Button btnBattle;
        public Button btnLearn;
        public Text text;

        private bool isHiSaid = false;


        internal void SetActive(bool active)
        {
            ui.SetActive(active);

            if (active)
            {
                Refresh();
            }
        }


        internal void Refresh()
        {
            StopAllCoroutines();
            StartCoroutine(IE_Refresh());
        }

        IEnumerator IE_Refresh()
        {
            if (Device.nConnected == 0)
            {
                btnConnect.interactable = true;
                btnTutorial.interactable = false;
                btnBattle.interactable = false;
                btnLearn.interactable = false;

                if (!isHiSaid)
                {
                    text.text = "こんにちは！\nボクの名前は「迷キュー」";
                    yield return new WaitForSecondsRealtime(2f);
                    isHiSaid = true;
                }
                text.text = "左上の「接続」ボタンから\nキューブを接続してね！";

                if (!MeiPrefs.isTutorialCleared)
                    UIFinger.PointAt(btnConnect.transform, biasX:70);
                else
                    UIFinger.Hide();
            }
            else if (Device.nConnected == 1)
            {
                btnConnect.interactable = true;
                btnTutorial.interactable = false;
                btnBattle.interactable = false;
                btnLearn.interactable = false;

                if (!MeiPrefs.isTutorialCleared)
                    UIFinger.PointAt(btnConnect.transform, biasX:70);
                else
                    UIFinger.Hide();

                text.text = "もう1つ接続してね！";
            }
            else if (Device.nConnected == 2)
            {
                btnConnect.interactable = false;
                UIFinger.Hide();

                if (!MeiPrefs.isTutorialCleared)
                {
                    btnTutorial.interactable = true;
                    btnBattle.interactable = false;
                    btnLearn.interactable = false;

                    UIFinger.PointAt(btnTutorial.transform, biasX:130);

                    text.text = "まずは迷路パズルのルールを\n説明するよ！";
                }
                else if (MeiPrefs.level == 1)
                {
                    btnTutorial.interactable = true;
                    btnBattle.interactable = true;
                    btnLearn.interactable = false;

                    while (true)
                    {
                        text.text = "みんなは迷路が好きかな？";
                        yield return new WaitForSecondsRealtime(2f);
                        text.text = "ボクと迷路パズルでバトルしよう！";
                        yield return new WaitForSecondsRealtime(2f);
                    }
                }
                else if (!MeiPrefs.isLearnCleared)
                {
                    btnTutorial.interactable = true;
                    btnBattle.interactable = true;
                    btnLearn.interactable = true;

                    UIFinger.PointAt(btnLearn.transform, biasX:130);

                    while (true)
                    {
                        text.text = "レベル1のクリア、おめでとう！";
                        yield return new WaitForSecondsRealtime(2f);
                        text.text = "かいせつボタンを押すと、\nボクの強さのひみつが分かるよ";
                        yield return new WaitForSecondsRealtime(2f);
                    }
                }
                else
                {
                    if (!MeiPrefs.isBattleEnteredAfterLearn)
                    {
                        UIFinger.PointAt(btnBattle.transform, biasX:130);
                    }

                    btnTutorial.interactable = true;
                    btnBattle.interactable = true;
                    btnLearn.interactable = true;

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

        public async void OnBtnConnect()
        {
            btnConnect.interactable = false;
            btnConnect.GetComponent<ButtonConnect>().SetBusy(true);
            UIFinger.Hide();

            try
            {
                await Device.Connect();
            }
            catch (Exception e)     // Error occurs when user cancels connection request dialog
            {
                Debug.LogError(e.Message);
            }

            btnConnect.GetComponent<ButtonConnect>().SetBusy(false);

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
    }

}
