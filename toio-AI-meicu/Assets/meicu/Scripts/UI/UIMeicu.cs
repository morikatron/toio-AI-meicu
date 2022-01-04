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

        // private IEnumerator ie = null;
        private Face face = Face.Default;


        public enum Face
        {
            Default, Dull, Laugh, Regret, Trouble
        }

        public void SetFace(Face face)
        {
            var tr = img.transform as RectTransform;

            if (this.face != face)
            {
                if (face == Face.Default)
                {
                    img.sprite = spriteDefault;
                }
                else if (face == Face.Dull)
                {
                    img.sprite = spriteDull;
                }
                else if (face == Face.Laugh)
                {
                    img.sprite = spriteLaugh;
                    // IEnumerator Laugh(){
                    //     Debug.Log(tr.localPosition);
                    // }
                }
                else if (face == Face.Regret)
                {
                    img.sprite = spriteRegret;
                }
                else if (face == Face.Trouble)
                {
                    img.sprite = spriteTrouble;
                }
            }

            this.face = face;
        }

        public void Reset()
        {
            GetComponent<Animator>().SetBool("isThinking", false);
            SetFace(Face.Default);
        }
        public void PerformThinkBegin()
        {
            GetComponent<Animator>().SetBool("isThinking", true);
            SetFace(Face.Dull);
        }
        public void PerformThinkEnd()
        {
            GetComponent<Animator>().SetBool("isThinking", false);
            SetFace(Face.Default);
        }
        public void PerformFail()
        {
            // TODO
            SetFace(Face.Trouble);
        }
    }

}

