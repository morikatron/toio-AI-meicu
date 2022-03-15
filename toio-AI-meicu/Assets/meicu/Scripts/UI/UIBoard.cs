using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace toio.AI.meicu
{

    public class UIBoard : MonoBehaviour, IPointerClickHandler
    {
        public static readonly Color BoardBlack = new Color32(55, 55, 55, 255);
        public static readonly Color BoardWhite = new Color32(255, 255, 255, 255);
        public static readonly Color BoardRed = new Color32(255, 148, 148, 255);
        public static readonly Color BoardBlue = new Color32(148, 221, 255, 255);
        public static readonly Color BoardGreen = new Color32(158, 255, 162, 255);
        public static readonly Color BoardYellow = new Color32(255, 234, 129, 255);

        internal Vector2Int biasP = new Vector2Int(2, 2);
        internal Vector2Int biasA = new Vector2Int(-2, -2);

        public GameObject uiRewardPrefab;
        public event Action<Vector2Int, RewardPositionType> onSpaceClicked;
        public float heatmapZeroOpacity = 0.1f;
        public float heatmapMinOpacity = 0.2f;

        private List<GameObject> trajPObjs = new List<GameObject>();
        private List<GameObject> trajAObjs = new List<GameObject>();

        private Dictionary<Vector3Int, GameObject> rewards = new Dictionary<Vector3Int, GameObject>(); // 3rd dim: 0 for right, 1 for bottom


        internal void Reset()
        {
            HideTrajA();
            HideTrajP();
            HideKomaA();
            HideKomaP();
            HideGoal();
            HideFail();
            ClearRewards();

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

                var tr = CreateLine(trajPObjs, Config.UIBlue);
                trajPObjs.Add(tr.gameObject);

                tr.anchoredPosition = (coords + lstCoords) / 2 + biasP;
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
            ShowKomaP(rowCol.x, rowCol.y);
        }
        internal void ShowKomaP(int row, int col)
        {
            var coords = RowCol2UICoords(row, col);

            var tr = transform.Find("KomaP") as RectTransform;
            tr.gameObject.SetActive(true);
            tr.anchoredPosition = coords + biasP;
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

                var tr = CreateLine(trajAObjs, Config.UIOrange);
                trajAObjs.Add(tr.gameObject);

                tr.anchoredPosition = (coords + lstCoords) / 2 + biasA;
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

        internal void ShowKomaA(Vector2Int rowCol, float scale=1)
        {
            ShowKomaA(rowCol.x, rowCol.y, scale);
        }
        internal void ShowKomaA(int row, int col, float scale=1)
        {
            var coords = RowCol2UICoords(row, col);

            var tr = transform.Find("KomaA") as RectTransform;
            tr.gameObject.SetActive(true);
            tr.localScale = new Vector3(scale, scale, scale);
            tr.anchoredPosition = coords + biasA;
        }
        internal void HideKomaA()
        {
            var komaA = transform.Find("KomaA") as RectTransform;
            komaA.gameObject.SetActive(false);
        }

        internal void ShowGoal(Vector2Int rowCol)
        {
            ShowGoal(rowCol.x, rowCol.y);
        }
        internal void ShowGoal(int row, int col)
        {
            var coords = RowCol2UICoords(row, col);

            var goal = transform.Find("Goal") as RectTransform;
            goal.gameObject.SetActive(true);
            goal.anchoredPosition = coords;
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

        internal void ShowFailP()
        {
            transform.Find("KomaP").Find("ImgFail").gameObject.SetActive(true);
        }
        internal void ShowFailA()
        {
            transform.Find("KomaA").Find("ImgFail").gameObject.SetActive(true);
        }
        internal void HideFail()
        {
            transform.Find("KomaP").Find("ImgFail").gameObject.SetActive(false);
            transform.Find("KomaA").Find("ImgFail").gameObject.SetActive(false);
        }


        #region ======== Reward ========
        internal int RewardCount => rewards.Count;
        internal List<Vector3Int> rewardList => new List<Vector3Int>(rewards.Keys);
        internal bool HasReward(int row, int col, RewardPositionType type)
        {
            Vector3Int pos = new Vector3Int(row, col, (int)type);
            return rewards.ContainsKey(pos);
        }
        internal (bool, bool) PutReward(int row, int col, RewardPositionType type, int maxCount=1)
        {
            if (!IsRewardPositionLegal(row, col, type)) return (false, false);

            Vector3Int pos = new Vector3Int(row, col, (int)type);
            // Delete if exist
            if (rewards.ContainsKey(pos))
            {
                GameObject.Destroy(rewards[pos]);
                rewards.Remove(pos);
                return (true, false);
            }
            // Add if max not reached
            else if (rewards.Count < maxCount)
            {
                var obj = GameObject.Instantiate(uiRewardPrefab, transform.Find("Rewards"));
                (obj.transform as RectTransform).anchoredPosition = GetRewardUICoords(row, col, type);
                rewards.Add(pos, obj);
                return (false, true);
            }
            return (false, false);
        }
        protected Vector2Int GetRewardUICoords(int row, int col, RewardPositionType type)
        {
            var uiCoords = RowCol2UICoords(row, col);
            if (type == RewardPositionType.Center) {}
            else if (type == RewardPositionType.RightBorder) uiCoords.x += 5;
            else if (type == RewardPositionType.BottomBorder) uiCoords.y -= 5;
            return uiCoords;
        }
        internal void ClearRewards()
        {
            foreach (var o in rewards.Values) GameObject.Destroy(o);
            rewards.Clear();
        }
        internal int GetReward(int row, int col, Env.Action action)
        {
            (int row_, int col_) = Env.Translate(action, row, col);
            int r = 0;
            if (IsRewardAvailable(row_, col_, RewardPositionType.Center))
            {
                r++;
                PerformRewardGot(row_, col_, RewardPositionType.Center);
            }
            if (IsRewardAvailable(row, col, RewardPositionType.RightBorder) && action == Env.Action.Right)
            {
                r++;
                PerformRewardGot(row, col, RewardPositionType.RightBorder);
            }
            if (IsRewardAvailable(row, col, RewardPositionType.BottomBorder) && action == Env.Action.Down)
            {
                r++;
                PerformRewardGot(row, col, RewardPositionType.BottomBorder);
            }
            if (IsRewardAvailable(row, col-1, RewardPositionType.RightBorder) && action == Env.Action.Left)
            {
                r++;
                PerformRewardGot(row, col-1, RewardPositionType.RightBorder);
            }
            if (IsRewardAvailable(row-1, col, RewardPositionType.BottomBorder) && action == Env.Action.Up)
            {
                r++;
                PerformRewardGot(row-1, col, RewardPositionType.BottomBorder);
            }
            return r;
        }
        protected bool IsRewardAvailable(int row, int col, RewardPositionType type)
        {
            Vector3Int pos = new Vector3Int(row, col, (int)type);
            if (!rewards.ContainsKey(pos)) return false;
            var reward = rewards[pos];
            return reward.gameObject.activeSelf;
        }
        protected void PerformRewardGot(int row, int col, RewardPositionType type, bool disable = true)
        {
            if (!IsRewardAvailable(row, col, type)) return;
            Vector3Int pos = new Vector3Int(row, col, (int)type);
            var reward = rewards[pos];
            if (disable) reward.gameObject.SetActive(false);

            var obj = GameObject.Instantiate(uiRewardPrefab, transform.Find("RewardsAnim"));
            (obj.transform as RectTransform).anchoredPosition = (reward.transform as RectTransform).anchoredPosition;
            var anim = obj.GetComponentInChildren<Animator>();
            anim.SetBool("Got", true);
            Destroy(obj, 0.5f);
        }
        internal void ResetRewardGot()
        {
            foreach (var o in rewards.Values)
            {
                // o.GetComponentInChildren<Animator>().SetBool("Got", false);
                o.SetActive(true);
            }
        }

        public void OnPointerClick(PointerEventData e)
        {
            var local = (transform as RectTransform).InverseTransformPoint(e.position);
            if (local.x < -45 || local.x > 45 || local.y < -45 || local.y > 45)
                return;

            var posPlace = UI2RowColCoords(local);
            // this.onSpaceClicked?.Invoke(coords);

            var placeCenter = RowCol2UICoords(posPlace);
            Vector2 diff = new Vector2(local.x+45, local.y-45) - placeCenter;   // convert to top-left coords

            // Inside place
            if (Mathf.Abs(diff.x) <= 3.3f && Mathf.Abs(diff.y) <= 3.3f)
            {
                this.onSpaceClicked?.Invoke(posPlace, RewardPositionType.Center);
            }
            // On border
            else
            {
                if (diff.x > Mathf.Abs(diff.y))  // right
                {
                    this.onSpaceClicked?.Invoke(new Vector2Int(posPlace.x, posPlace.y), RewardPositionType.RightBorder);
                }
                else if (diff.x < -Mathf.Abs(diff.y))  // left
                {
                    if (posPlace.y > 0)
                        this.onSpaceClicked?.Invoke(new Vector2Int(posPlace.x, posPlace.y-1), RewardPositionType.RightBorder);
                }
                else if (diff.y < -Mathf.Abs(diff.x))    // bottom
                {
                    this.onSpaceClicked?.Invoke(new Vector2Int(posPlace.x, posPlace.y), RewardPositionType.BottomBorder);
                }
                else if (diff.y > Mathf.Abs(diff.x))    // top
                {
                    if (posPlace.x > 0)
                        this.onSpaceClicked?.Invoke(new Vector2Int(posPlace.x-1, posPlace.y), RewardPositionType.BottomBorder);
                }
            }
        }
        internal bool IsRewardPositionLegal(int row, int col, RewardPositionType type)
        {
            if (row < 0 || row > 8 || col < 0 || col > 8) return false;
            if (type == RewardPositionType.RightBorder && col > 7) return false;
            if (type == RewardPositionType.BottomBorder && row > 7) return false;
            return true;
        }

        public enum RewardPositionType
        {
            Center, RightBorder, BottomBorder,
        }

        #endregion


        private RectTransform CreateLine(List<GameObject> addTo, Color color)
        {
            GameObject go = new GameObject("traj");
            go.transform.SetParent(transform.Find("Trajs"), false);
            addTo.Add(go);
            RawImage img = go.AddComponent<RawImage>();
            img.color = color;

            var tr = go.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0, 1);
            tr.anchorMax = new Vector2(0, 1);
            return tr;
        }

        /// <summary>
        /// UI coords is relative to top-left corder of mat.
        /// </summary>
        internal Vector2Int RowCol2UICoords(Vector2Int rowCol)
        {
            return new Vector2Int(rowCol.y * 10 + 5, -rowCol.x * 10 - 5);
        }
        /// <summary>
        /// UI coords is relative to top-left corder of mat.
        /// </summary>
        internal Vector2Int RowCol2UICoords(int row, int col)
        {
            return new Vector2Int(col * 10 + 5, -row * 10 - 5);
        }
        /// <summary>
        /// UI coords is relative to center of mat.
        /// </summary>
        internal Vector2Int UI2RowColCoords(Vector2 uiCoords)
        {
            return new Vector2Int((int)((uiCoords.y - 45)/(-10)), (int)((uiCoords.x + 45)/10));
        }
    }

}
