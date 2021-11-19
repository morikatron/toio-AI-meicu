using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace toio.AI.meicu
{

    public class UIBoard : MonoBehaviour
    {
        public static readonly Color BoardBlack = new Color32(55, 55, 55, 255);
        public static readonly Color BoardWhite = new Color32(255, 255, 255, 255);
        public static readonly Color BoardRed = new Color32(255, 148, 148, 255);
        public static readonly Color BoardBlue = new Color32(148, 221, 255, 255);
        public static readonly Color BoardGreen = new Color32(158, 255, 162, 255);
        public static readonly Color BoardYellow = new Color32(255, 234, 129, 255);

        private static readonly Vector2Int BiasP = new Vector2Int(2, 2);
        private static readonly Vector2Int BiasA = new Vector2Int(-2, -2);


        public float heatmapZeroOpacity = 0.1f;
        public float heatmapMinOpacity = 0.2f;

        private List<GameObject> trajPObjs = new List<GameObject>();
        private List<GameObject> trajAObjs = new List<GameObject>();


        internal void Reset()
        {
            HideTrajA();
            HideTrajP();
            HideKomaA();
            HideKomaP();
            HideGoal();

            // Reset Mat
            for (int r = 0; r < 9; r++)
            {
                var rowTr = transform.Find($"Row ({r})");
                for (int c = 0; c < 9; c++)
                {
                    var spaceTr = rowTr.Find($"Space ({c})");
                    var img = spaceTr.GetComponent<RawImage>();

                    switch (Env.defaultState[r, c])
                    {
                        case Env.Space.W: img.color = BoardWhite; break;
                        case Env.Space.R: img.color = BoardRed; break;
                        case Env.Space.B: img.color = BoardBlue; break;
                        case Env.Space.G: img.color = BoardGreen; break;
                        case Env.Space.Y: img.color = BoardYellow; break;
                        case Env.Space.None: img.color = BoardBlack; break;
                    }
                }
            }
        }

        internal void ShowTrajP(Vector2Int[] traj)
        {
            HideTrajP();

            Vector2Int lstCoords = RowCol2UICoords(new Vector2Int(4, 4));

            foreach (var rowCol in traj)
            {
                var coords = RowCol2UICoords(rowCol);

                var tr = CreateLine(trajPObjs);
                trajPObjs.Add(tr.gameObject);

                tr.anchoredPosition = (coords + lstCoords) / 2 + BiasP;
                var w = Mathf.Max(0.5f, Mathf.Abs(coords.x - lstCoords.x));
                var h = Mathf.Max(0.5f, Mathf.Abs(coords.y - lstCoords.y));
                tr.sizeDelta = new Vector2(w, h);

                lstCoords = coords;
            }
        }
        internal void HideTrajP()
        {
            foreach (var obj in trajPObjs)
                GameObject.Destroy(obj);
            trajPObjs.Clear();
        }

        internal void ShowKomaP(Vector2Int rowCol)
        {
            var coords = RowCol2UICoords(rowCol);

            var tr = transform.Find("KomaP") as RectTransform;
            tr.gameObject.SetActive(true);
            tr.anchoredPosition = coords + BiasP;
        }
        internal void ShowKomaP(int row, int col)
        {
            ShowKomaP(new Vector2Int(row, col));
        }
        internal void HideKomaP()
        {
            var komaP = transform.Find("KomaP") as RectTransform;
            komaP.gameObject.SetActive(false);
        }

        internal void ShowTrajA(Vector2Int[] traj)
        {
            HideTrajA();

            Vector2Int lstCoords = RowCol2UICoords(new Vector2Int(4, 4));

            foreach (var rowCol in traj)
            {
                var coords = RowCol2UICoords(rowCol);

                var tr = CreateLine(trajAObjs);
                trajAObjs.Add(tr.gameObject);

                tr.anchoredPosition = (coords + lstCoords) / 2 + BiasA;
                var w = Mathf.Max(0.5f, Mathf.Abs(coords.x - lstCoords.x));
                var h = Mathf.Max(0.5f, Mathf.Abs(coords.y - lstCoords.y));
                tr.sizeDelta = new Vector2(w, h);

                lstCoords = coords;
            }
        }
        internal void HideTrajA()
        {
            foreach (var obj in trajAObjs)
                GameObject.Destroy(obj);
            trajAObjs.Clear();
        }

        internal void ShowKomaA(Vector2Int rowCol)
        {
            var coords = RowCol2UICoords(rowCol);

            var tr = transform.Find("KomaA") as RectTransform;
            tr.gameObject.SetActive(true);
            tr.anchoredPosition = coords + BiasA;
        }
        internal void ShowKomaA(int row, int col)
        {
            ShowKomaA(new Vector2Int(row, col));
        }
        internal void HideKomaA()
        {
            var komaA = transform.Find("KomaA") as RectTransform;
            komaA.gameObject.SetActive(false);
        }

        internal void ShowGoal(Vector2Int rowCol)
        {
            var coords = RowCol2UICoords(rowCol);

            var goal = transform.Find("Goal") as RectTransform;
            goal.gameObject.SetActive(true);
            goal.anchoredPosition = coords;
        }
        internal void ShowGoal(int row, int col)
        {
            ShowGoal(new Vector2Int(row, col));
        }
        internal void HideGoal()
        {
            var goal = transform.Find("Goal") as RectTransform;
            goal.gameObject.SetActive(false);
        }

        internal void ShowHeatmap(float[,] heatmap = null)
        {
            for (int r = 0; r < 9; r++)
            {
                var rowTr = transform.Find($"Row ({r})");
                for (int c = 0; c < 9; c++)
                {
                    var spaceTr = rowTr.Find($"Space ({c})");
                    var img = spaceTr.GetComponent<RawImage>();
                    if (heatmap !=null && r < heatmap.GetLength(0) && c < heatmap.GetLength(1))
                    {
                        var color = img.color;
                        var a = heatmap[r, c];
                        if (a == 0)
                        {
                            color.a = heatmapZeroOpacity;
                        }
                        else
                        {
                            a = Mathf.Pow(a, 0.8f);     // Easier to see
                            a = a * (1-heatmapMinOpacity) + heatmapMinOpacity;   // Mapping to [heatmapMinOpacity, 1]
                            color.a = a;
                        }
                        img.color = color;
                    }
                    else
                    {
                        var color = img.color;
                        color.a = heatmapZeroOpacity;
                        img.color = color;
                    }
                }
            }
        }

        internal void HideHeatmap()
        {
            for (int r = 0; r < 9; r++)
            {
                var rowTr = transform.Find($"Row ({r})");
                for (int c = 0; c < 9; c++)
                {
                    var spaceTr = rowTr.Find($"Space ({c})");
                    var img = spaceTr.GetComponent<RawImage>();
                    var color = img.color;
                    color.a = 1;
                    img.color = color;
                }
            }
        }


        private RectTransform CreateLine(List<GameObject> addTo)
        {
            GameObject go = new GameObject("traj");
            go.transform.SetParent(transform.Find("Trajs"), false);
            addTo.Add(go);
            RawImage img = go.AddComponent<RawImage>();
            img.color = new Color32(114, 114, 114, 255);

            var tr = go.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0, 1);
            tr.anchorMax = new Vector2(0, 1);
            return tr;
        }

        internal Vector2Int RowCol2UICoords(Vector2Int rowCol)
        {
            return new Vector2Int(rowCol.y * 10 + 5, -rowCol.x * 10 - 5);
        }
    }

}
