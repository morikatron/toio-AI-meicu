using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace toio.AI.meicu
{
    public class UIQuest : MonoBehaviour
    {
        internal void Reset()
        {
            ShowP(0);
            ShowA(0);

            for (int i = 0; i < 13; i++)
            {
                var tr = transform.Find($"Q ({i})");
                tr.gameObject.SetActive(false);
            }

            var goal = transform.Find("Goal") as RectTransform;
            goal.gameObject.SetActive(false);
        }

        internal void ShowQuest(Env.Space[] colors)
        {
            int spaces = colors.Length*2 - 1;
            for (int i = 0; i < Mathf.Min(13, spaces); i++)
            {
                var tr = transform.Find($"Q ({i})");
                tr.gameObject.SetActive(true);

                Color color = Color.white;
                if (i % 2 == 0)
                    switch (colors[i/2])
                    {
                        case Env.Space.R: color = UIBoard.BoardRed; break;
                        case Env.Space.B: color = UIBoard.BoardBlue; break;
                        case Env.Space.G: color = UIBoard.BoardGreen; break;
                        case Env.Space.Y: color = UIBoard.BoardYellow; break;
                    }
                tr.GetComponent<RawImage>().color = color;
            }
            for (int i = spaces; i < 13; i++)
            {
                var tr = transform.Find($"Q ({i})");
                tr.gameObject.SetActive(false);
            }

            var line = transform.Find("Line") as RectTransform;
            line.sizeDelta = new Vector2(35 * spaces, 5);

            var goal = transform.Find("Goal") as RectTransform;
            goal.gameObject.SetActive(true);
            goal.anchoredPosition = new Vector2(35 * spaces - 5, 0);
        }
        internal void ShowQuest(MeiQuest quest)
        {
            ShowQuest(quest.colors);
        }

        internal void ShowP(int step)
        {
            var tr = transform.Find("KomaP") as RectTransform;
            tr.gameObject.SetActive(true);
            tr.anchoredPosition = new Vector2(15 + 35*step, 15);
        }
        internal void HideP()
        {
            transform.Find("KomaP").gameObject.SetActive(false);
        }
        internal void ShowA(int step)
        {
            var tr = transform.Find("KomaA") as RectTransform;
            tr.gameObject.SetActive(true);
            tr.anchoredPosition = new Vector2(15 + 35*step, -15);
        }
        internal void HideA()
        {
            transform.Find("KomaA").gameObject.SetActive(false);
        }
    }

}
