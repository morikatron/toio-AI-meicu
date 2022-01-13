using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace toio.AI.meicu
{

    public class UILevelView : MonoBehaviour
    {
        public Sprite currentSprite;
        public Sprite defeatSprite;

        public void ShowLevel(int level)
        {
            for (int lv = 1; lv < level; lv++)
            {
                var img = transform.Find($"Lv ({lv})").GetComponent<Image>();
                img.color = Color.white;
                img.sprite = defeatSprite;
            }

            {
                var img = transform.Find($"Lv ({level})").GetComponent<Image>();
                img.color = Color.white;
                img.sprite = currentSprite;
            }

            for (int lv = level+1; lv <= 11; lv++)
            {
                var img = transform.Find($"Lv ({lv})").GetComponent<Image>();
                img.color = new Color32(0, 0, 0, 80);
            }
        }
    }

}
