using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace toio.AI.meicu
{
    public class UIStage : MonoBehaviour
    {
        public Image hand;
        public Text text;

        void Start()
        {
            SetHand(1);
        }

        public void SetHand(int value, int max = 5)
        {
            var tr = hand.transform as RectTransform;
            const float left = 65, right = -65;
            var step = (right - left) / (max - 1);
            var deg = left + step * (value - 1);
            tr.localEulerAngles = new Vector3(0, 0, deg);

            text.text = value.ToString();
        }
    }

}
