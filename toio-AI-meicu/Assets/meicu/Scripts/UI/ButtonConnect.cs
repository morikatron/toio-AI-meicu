using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace toio.AI.meicu
{

    public class ButtonConnect : MonoBehaviour
    {
        public GameObject mask;
        public RectTransform busy;

        private bool isBusy;


        void Update()
        {
            if (isBusy)
            {
                busy.eulerAngles += new Vector3(0, 0, 1);
            }
        }

        internal void SetBusy(bool isBusy)
        {
            this.isBusy = isBusy;
            mask.SetActive(isBusy);
            busy.gameObject.SetActive(isBusy);
        }
    }

}
