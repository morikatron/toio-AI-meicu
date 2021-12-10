using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace toio.AI.meicu
{
    public class DialogConnect : MonoBehaviour
    {
        public Text text;
        public Button button;


        public async void OnBtnConnect()
        {
            button.interactable = false;
            button.GetComponent<ButtonConnect>().SetBusy(true);

            try
            {
                await Device.Connect();
            }
            catch (Exception e)     // Error occurs when user cancels connection request dialog
            {
                Debug.LogError(e.Message);
            }

            button.GetComponent<ButtonConnect>().SetBusy(false);

            if (Device.isBothConnected)
            {
                // this.btnStart.interactable = true;
                Device.cubeManager.handles[0].SetBorderRect(new RectInt(545, 45, 410, 410));
                Device.cubeManager.handles[1].SetBorderRect(new RectInt(545, 45, 410, 410));
                PlayerController.ins.Init();
                AIController.ins.Init();

                PageManager.OnReconnected();
            }

            button.interactable = true;
        }
    }
}
