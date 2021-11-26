using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace toio.AI.meicu
{
    [RequireComponent(typeof(Image))]
    public class ButtonBlink : MonoBehaviour
    {
        public float period = 1;
        public byte maxValue = 255;
        public byte minValue = 200;

        bool isBlinking = false;
        Color formerColor;


        void Update()
        {
            if (isBlinking)
            {
                var m = Mathf.Cos(Time.time*Mathf.PI*2/period) * (maxValue-minValue)/2f + ((float)maxValue+minValue)/2;
                // GetComponent<Image>().color = new Color32(m, m, m, 255);
                Color color = formerColor;
                color.r *= m/255;
                color.g *= m/255;
                color.b *= m/255;
                GetComponent<Image>().color = color;
            }
        }

        internal void Blink(bool valid)
        {
            if (!isBlinking && valid)
            {
                formerColor = GetComponent<Image>().color;
            }
            else if (isBlinking && !valid)
            {
                GetComponent<Image>().color = formerColor;
            }
            isBlinking = valid;
        }
    }

}
