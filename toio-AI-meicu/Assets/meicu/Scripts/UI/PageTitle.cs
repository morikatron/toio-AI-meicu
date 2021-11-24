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

        void Update()
        {
            byte c = (byte)(Mathf.Cos(Time.time*5) * 55/2 + 200+55/2);
            if (Device.nConnected < 2)
            {
                if (isConnecting)
                {
                    (btnConnect.transform.Find("Busy") as RectTransform).eulerAngles += new Vector3(0, 0, 1);
                }
                else
                {
                    btnConnect.GetComponent<Image>().color = new Color32(c, c, c, 255);
                }
            }
            else if (!MeiPrefs.isTutorialCleared)
            {
                btnTutorial.GetComponent<Image>().color = new Color32(c, c, c, 255);
            }
            else if (!MeiPrefs.isLearnCleared)
            {
                btnLearn.GetComponent<Image>().color = new Color32(c, c, c, 255);
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

                text.text = "左上のCONNECT（コネクト）ボタンからキューブを接続してね！";
            }
            else if (Device.nConnected == 1)
            {
                btnConnect.interactable = true;
                btnTutorial.interactable = false;
                btnBattle.interactable = false;
                btnLearn.interactable = true;   // TODO

                text.text = "もう1つ接続してね！";
            }
            else if (Device.nConnected == 2)
            {
                btnConnect.interactable = false;

                if (!MeiPrefs.isTutorialCleared)
                {
                    btnTutorial.interactable = true;
                    btnBattle.interactable = false;
                    btnLearn.interactable = false;
                    text.text = "まずは迷路パズルのルールを説明するよ！";
                }
                else if (!MeiPrefs.isLearnCleared)
                {
                    btnTutorial.interactable = false;
                    btnBattle.interactable = false;
                    btnLearn.interactable = true;
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
            this.btnConnect.interactable = false;

            this.isConnecting = true;
            btnConnect.transform.Find("Mask").gameObject.SetActive(true);
            btnConnect.transform.Find("Busy").gameObject.SetActive(true);

            #if UNITY_WEBGL
            await Device.cubeManager.SingleConnect();
            #else
            await Device.cubeManager.MultiConnect(2);
            #endif

            btnConnect.transform.Find("Mask").gameObject.SetActive(false);
            btnConnect.transform.Find("Busy").gameObject.SetActive(false);
            this.isConnecting = false;

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
