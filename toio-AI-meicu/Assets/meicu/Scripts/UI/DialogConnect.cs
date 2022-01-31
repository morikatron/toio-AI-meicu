using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace toio.AI.meicu
{
    public class DialogConnect : MonoBehaviour
    {
        public TMP_Text text;
        public TMP_Text textError;
        public Button button;

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);

            textError.gameObject.SetActive(false);
        }

        public async void OnBtnConnect()
        {
            button.interactable = false;
            button.GetComponent<ButtonConnect>().SetBusy(true);

            int code = await Device.Connect();
            textError.gameObject.SetActive(code > 0);

            button.GetComponent<ButtonConnect>().SetBusy(false);

            if (Device.isTwoConnected)
            {
                PageManager.OnReconnected();
            }

            button.interactable = true;
        }
    }
}
