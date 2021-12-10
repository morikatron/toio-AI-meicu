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
                    text.text = "こんにちは！\n僕の名前は「迷キュー」";
                    yield return new WaitForSecondsRealtime(2f);
                    isHiSaid = true;
                }
                text.text = "左上の「接続」ボタンから\nキューブを接続してね！";

                if (!MeiPrefs.isTutorialCleared)
                    UIFinger.PointAt(btnConnect.transform, biasX:70);
            }
            else if (Device.nConnected == 1)
            {
                btnConnect.interactable = true;
                btnTutorial.interactable = false;
                btnBattle.interactable = false;
                btnLearn.interactable = false;

                // if (!MeiPrefs.isTutorialCleared)
                    UIFinger.PointAt(btnConnect.transform, biasX:70);

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
                else if (!MeiPrefs.isLearnCleared)
                {
                    btnTutorial.interactable = false;
                    btnBattle.interactable = false;
                    btnLearn.interactable = true;

                    UIFinger.PointAt(btnLearn.transform, biasX:130);

                    text.text = "僕たちAIロボットが迷路パズルを\nどのように解いているのか、\nかいせつするよ！";
                }
                else
                {
                    btnTutorial.interactable = true;
                    btnBattle.interactable = true;
                    btnLearn.interactable = true;

                    while (true)
                    {
                        text.text = "みんなは迷路が好きかな？";
                        yield return new WaitForSecondsRealtime(2f);
                        text.text = "僕と迷路パズルでバトルしよう！";
                        yield return new WaitForSecondsRealtime(2f);
                    }
                }
            }
        }

        public async void OnBtnConnect()
        {
            btnConnect.interactable = false;
            btnConnect.GetComponent<ButtonConnect>().SetBusy(true);

            try
            {
                await Device.Connect();
            }
            catch (Exception e)     // Error occurs when user cancels connection request dialog
            {
                Debug.LogError(e.Message);
            }

            btnConnect.GetComponent<ButtonConnect>().SetBusy(false);

            if (Device.isBothConnected)
            {
                // this.btnStart.interactable = true;
                Device.cubeManager.handles[0].SetBorderRect(new RectInt(545, 45, 410, 410));
                Device.cubeManager.handles[1].SetBorderRect(new RectInt(545, 45, 410, 410));

                PlayerController.ins.Init();
                AIController.ins.Init();
            }

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
