using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace toio.AI.meicu
{

    public class UIMeicu : MonoBehaviour
    {
        public Image img;
        public Sprite spriteDefault;
        public Sprite spriteDull;
        public Sprite spriteLaugh;
        public Sprite spriteRegret;
        public Sprite spriteTrouble;

        private IEnumerator ie = null;
        private State state = State.Default;


        public enum State
        {
            Default, Dull, Laugh, Regret, Trouble
        }

        public void SetState(State state)
        {
            var tr = img.transform as RectTransform;

            if (this.state != state)
            {
                if (state == State.Default)
                {
                    img.sprite = spriteDefault;
                }
                else if (state == State.Dull)
                {
                    img.sprite = spriteDull;
                }
                else if (state == State.Laugh)
                {
                    img.sprite = spriteLaugh;
                    // IEnumerator Laugh(){
                    //     Debug.Log(tr.localPosition);
                    // }
                }
                else if (state == State.Regret)
                {
                    img.sprite = spriteRegret;
                }
                else if (state == State.Trouble)
                {
                    img.sprite = spriteTrouble;
                }
            }

            this.state = state;
        }
    }

}

