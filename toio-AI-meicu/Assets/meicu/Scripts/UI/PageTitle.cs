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
            if (Device.isTwoConnected)
            {
                btnConnect.interactable = false;

                if (!MeiPrefs.isTutorialCleared)
                {
                    btnTutorial.interactable = true;
                    btnBattle.interactable = false;
                    btnLearn.interactable = false;
                    text.text = "いいゲーム紹介してあげる（グヘヘ）";
                }
                else if (!MeiPrefs.isLearnCleared)
                {
                    btnTutorial.interactable = false;
                    btnBattle.interactable = false;
                    btnLearn.interactable = true;
                    text.text = "私のやり方も教えてあげるぅ";
                }
                else
                {
                    btnTutorial.interactable = true;
                    btnBattle.interactable = true;
                    btnLearn.interactable = true;
                    text.text = "では、一緒にあそぼう~";
                }
            }
            else
            {
                btnConnect.interactable = true;
                btnTutorial.interactable = false;
                btnBattle.interactable = false;
                btnLearn.interactable = true;   // TODO

                btnLearn.interactable = true;

                text.text = "私に「体」を2つください。";
            }
        }

        public async void OnBtnConnect()
        {
            this.btnConnect.interactable = false;

            #if UNITY_WEBGL
            await Device.cubeManager.SingleConnect();
            #else
            await Device.cubeManager.MultiConnect(2);
            #endif

            if (Device.isTwoConnected)
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
