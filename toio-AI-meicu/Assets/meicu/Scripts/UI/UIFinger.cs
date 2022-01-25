using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace toio.AI.meicu
{

    public class UIFinger : MonoBehaviour
    {
        public float showTime = 0.75f;
        public float hideTime = 0.75f;

        static UIFinger ins = null;

        private float biasTime = 0;

        void OnEnable()
        {
            if (ins == null)
                ins = this;
            else if (ins != this)
                Destroy(this.gameObject);
        }

        void Update()
        {
            var t = (Time.time - biasTime) % (showTime + hideTime);
            GetComponentInChildren<RawImage>().color = new Color32(255, 255, 255, t < showTime? (byte)255 : (byte)0);
        }

        internal static void PointAt(Transform tr, float biasX=50, float biasY=-24)
        {
            ins.biasTime = Time.time;

            var scaler = GameObject.Find("Canvas").GetComponent<CanvasScaler>();
            float canvasScale = Screen.height / scaler.referenceResolution.y;
            ins.gameObject.SetActive(true);
            (ins.transform as RectTransform).position = tr.position + new Vector3(biasX*canvasScale, biasY*canvasScale, 0);
        }

        internal static void Hide()
        {
            ins.gameObject.SetActive(false);
        }
    }

}
