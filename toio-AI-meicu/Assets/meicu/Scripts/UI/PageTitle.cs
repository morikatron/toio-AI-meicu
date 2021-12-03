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
            if (Device.nConnected == 0)
            {
                btnConnect.interactable = true;
                btnTutorial.interactable = false;
                btnBattle.interactable = false;
                btnLearn.interactable = true;   // TODO

                // if (!MeiPrefs.isTutorialCleared)
                    UIFinger.PointAt(btnConnect.transform, biasX:70);

                text.text = "左上のCONNECT（コネクト）ボタンからキューブを接続してね！";
            }
            else if (Device.nConnected == 1)
            {
                btnConnect.interactable = true;
                btnTutorial.interactable = false;
                btnBattle.interactable = false;
                btnLearn.interactable = true;   // TODO

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

                    text.text = "まずは迷路パズルのルールを説明するよ！";
                }
                else if (!MeiPrefs.isLearnCleared)
                {
                    btnTutorial.interactable = false;
                    btnBattle.interactable = false;
                    btnLearn.interactable = true;

                    UIFinger.PointAt(btnLearn.transform, biasX:130);

                    text.text = "僕たちAIロボットが迷路パズルをどのように解いているのか、かいせつするよ！";
                }
                else
                {
                    btnTutorial.interactable = true;
                    btnBattle.interactable = true;
                    btnLearn.interactable = true;

                    text.text = "では、一緒にあそぼう~";
                }
            }
        }

        public async void OnBtnConnect()
        {
            btnConnect.interactable = false;
            btnConnect.GetComponent<ButtonConnect>().SetBusy(true);

            await Device.Connect();

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
