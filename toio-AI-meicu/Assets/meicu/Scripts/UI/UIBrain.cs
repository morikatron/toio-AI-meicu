using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace toio.AI.meicu
{
    public class UIBrain : MonoBehaviour
    {
        public Image imgLevel;
        public Text text;
        public Sprite[] levelSprites;

        public void SetLevel(int lv)
        {
            imgLevel.sprite = levelSprites[lv-1];
            text.text = lv.ToString();
        }
    }

}
