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

        private bool isConnecting = false;


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

                btnConnect.GetComponent<ButtonBlink>()?.Blink(true);
                btnTutorial.GetComponent<ButtonBlink>()?.Blink(false);
                btnLearn.GetComponent<ButtonBlink>()?.Blink(false);

                text.text = "左上のCONNECT（コネクト）ボタンからキューブを接続してね！";
            }
            else if (Device.nConnected == 1)
            {
                btnConnect.interactable = true;
                btnTutorial.interactable = false;
                btnBattle.interactable = false;
                btnLearn.interactable = true;   // TODO

                btnConnect.GetComponent<ButtonBlink>()?.Blink(true);
                btnTutorial.GetComponent<ButtonBlink>()?.Blink(false);
                btnLearn.GetComponent<ButtonBlink>()?.Blink(false);

                text.text = "もう1つ接続してね！";
            }
            else if (Device.nConnected == 2)
            {
                btnConnect.interactable = false;
                btnConnect.GetComponent<ButtonBlink>()?.Blink(false);

                if (!MeiPrefs.isTutorialCleared)
                {
                    btnTutorial.interactable = true;
                    btnBattle.interactable = false;
                    btnLearn.interactable = false;

                    btnTutorial.GetComponent<ButtonBlink>()?.Blink(true);
                    btnLearn.GetComponent<ButtonBlink>()?.Blink(false);

                    text.text = "まずは迷路パズルのルールを説明するよ！";
                }
                else if (!MeiPrefs.isLearnCleared)
                {
                    btnTutorial.interactable = false;
                    btnBattle.interactable = false;
                    btnLearn.interactable = true;

                    btnTutorial.GetComponent<ButtonBlink>()?.Blink(false);
                    btnLearn.GetComponent<ButtonBlink>()?.Blink(true);

                    text.text = "僕たちAIロボットが迷路パズルをどのように解いているのか、かいせつするよ！";
                }
                else
                {
                    btnTutorial.interactable = true;
                    btnBattle.interactable = true;
                    btnLearn.interactable = true;

                    btnTutorial.GetComponent<ButtonBlink>()?.Blink(false);
                    btnLearn.GetComponent<ButtonBlink>()?.Blink(false);

                    text.text = "では、一緒にあそぼう~";
                }
            }
        }

        public async void OnBtnConnect()
        {
            btnConnect.interactable = false;
            btnConnect.GetComponent<ButtonBlink>()?.Blink(false);
            btnConnect.GetComponent<ButtonConnect>().SetBusy(true);

            await Device.Connect();

            btnConnect.GetComponent<ButtonConnect>().SetBusy(false);

            if (Device.nConnected == 2)
            {
                // this.btnStart.interactable = true;
                Device.cubes[0].TurnLedOn(200,200,200,0);   // Lighted Cube is Player

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
