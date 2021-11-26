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
            button.GetComponent<ButtonBlink>()?.Blink(false);
            button.GetComponent<ButtonConnect>().SetBusy(true);

            await Device.Connect();

            button.GetComponent<ButtonConnect>().SetBusy(false);

            if (Device.isBothConnected)
                PageManager.OnReconnected();

            button.interactable = true;
        }
    }
}
