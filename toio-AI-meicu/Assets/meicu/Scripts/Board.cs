using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace toio.AI.meicu
{
    public class Board : MonoBehaviour
    {
        public static readonly Env.Space[,] defaultBoard = {
            {Env.Space.W, Env.Space.B, Env.Space.W, Env.Space.Y, Env.Space.W, Env.Space.R, Env.Space.W, Env.Space.R, Env.Space.W},
            {Env.Space.G, Env.Space.W, Env.Space.R, Env.Space.W, Env.Space.B, Env.Space.W, Env.Space.B, Env.Space.W, Env.Space.Y},
            {Env.Space.W, Env.Space.Y, Env.Space.W, Env.Space.Y, Env.Space.W, Env.Space.G, Env.Space.W, Env.Space.G, Env.Space.W},
            {Env.Space.B, Env.Space.W, Env.Space.G, Env.Space.W, Env.Space.R, Env.Space.W, Env.Space.B, Env.Space.W, Env.Space.R},
            {Env.Space.W, Env.Space.R, Env.Space.W, Env.Space.Y, Env.Space.W, Env.Space.G, Env.Space.W, Env.Space.G, Env.Space.W},
            {Env.Space.Y, Env.Space.W, Env.Space.B, Env.Space.W, Env.Space.B, Env.Space.W, Env.Space.Y, Env.Space.W, Env.Space.R},
            {Env.Space.W, Env.Space.G, Env.Space.W, Env.Space.R, Env.Space.W, Env.Space.G, Env.Space.W, Env.Space.B, Env.Space.W},
            {Env.Space.B, Env.Space.W, Env.Space.Y, Env.Space.W, Env.Space.B, Env.Space.W, Env.Space.R, Env.Space.W, Env.Space.Y},
            {Env.Space.W, Env.Space.R, Env.Space.W, Env.Space.G, Env.Space.W, Env.Space.Y, Env.Space.W, Env.Space.G, Env.Space.W},
        };

        public static readonly Color red = new Color(0.85f, 0.5f, 0.5f);
        public static readonly Color blue = new Color(0.45f, 0.7f, 0.85f);
        public static readonly Color green = new Color(0.45f, 0.85f, 0.45f);
        public static readonly Color yellow = new Color(0.9f, 0.8f, 0.5f);
        public static readonly Color nullColor = new Color(0.6f, 0.6f, 0.6f);

        void Start()
        {
            SetDefaultBoard();
        }

        void Update()
        {

        }

        public void SetDefaultBoard()
        {
            SetBoard(defaultBoard);
        }

        public void ShowHeatmap(float[,] heatmap)
        {
            var rows = heatmap.GetLength(0);
            var cols = heatmap.GetLength(1);

            for (int r=0; r<9; r++)
            {
                for (int c=0; c<9; c++)
                {
                    var spaceObj = GetSpaceTransform(r, c);
                    if (r < rows && c < cols)
                        spaceObj.GetComponent<MeshRenderer>().material.color = new Color(heatmap[r, c], heatmap[r, c], heatmap[r, c]);
                    else
                        spaceObj.GetComponent<MeshRenderer>().material.color = Color.black;
                }
            }
        }

        public void SetBoard(Env.Space[,] spaces)
        {
            var rows = spaces.GetLength(0);
            var cols = spaces.GetLength(1);

            for (int r=0; r<9; r++)
            {
                for (int c=0; c<9; c++)
                {
                    var spaceObj = GetSpaceTransform(r, c);

                    if (r < rows && c < cols)
                    {
                        var space = spaces[r, c];
                        if (space == Env.Space.B)
                        {
                            spaceObj.GetComponent<MeshRenderer>().material.color = blue;
                            spaceObj.Find("Signal").gameObject.tag = "mei_blue";
                        }
                        else if (space == Env.Space.R)
                        {
                            spaceObj.GetComponent<MeshRenderer>().material.color = red;
                            spaceObj.Find("Signal").gameObject.tag = "mei_red";
                        }
                        else if (space == Env.Space.G)
                        {
                            spaceObj.GetComponent<MeshRenderer>().material.color = green;
                            spaceObj.Find("Signal").gameObject.tag = "mei_green";
                        }
                        else if (space == Env.Space.Y)
                        {
                            spaceObj.GetComponent<MeshRenderer>().material.color = yellow;
                            spaceObj.Find("Signal").gameObject.tag = "mei_yellow";
                        }
                        else if (space == Env.Space.W)
                        {
                            spaceObj.GetComponent<MeshRenderer>().material.color = Color.white;
                            spaceObj.Find("Signal").gameObject.tag = "mei_white";
                        }
                        else if (space == Env.Space.Passed)
                        {
                            spaceObj.GetComponent<MeshRenderer>().material.color = nullColor;
                            spaceObj.Find("Signal").gameObject.tag = "mei_passed";
                        }
                        else if (space == Env.Space.None)
                        {
                            spaceObj.GetComponent<MeshRenderer>().material.color = nullColor;
                            spaceObj.Find("Signal").gameObject.tag = "Untagged";
                        }
                    }
                    else
                    {
                        spaceObj.GetComponent<MeshRenderer>().material.color = nullColor;
                            spaceObj.Find("Signal").gameObject.tag = "Untagged";
                    }
                }
            }
        }

        public Env.Space[,] GetState()
        {
            Env.Space[,] state = new Env.Space[9, 9];
            for (int r=0; r<9; r++)
            {
                for (int c=0; c<9; c++)
                {
                    var spaceObj = GetSpaceTransform(r, c);
                    var tag = spaceObj.Find("Signal").gameObject.tag;
                    switch (tag){
                        case "mei_blue" : state[r,c] = Env.Space.B; break;
                        case "mei_red" : state[r,c] = Env.Space.R; break;
                        case "mei_green" : state[r,c] = Env.Space.G; break;
                        case "mei_yellow" : state[r,c] = Env.Space.Y; break;
                        case "mei_white" : state[r,c] = Env.Space.W; break;
                        case "mei_passed" : state[r,c] = Env.Space.Passed; break;
                        default : state[r,c] = Env.Space.None; break;
                    }
                }
            }
            return state;
        }

        protected Transform GetSpaceTransform(int row, int col)
        {
            return transform.Find("Row" + row).Find("Space" + col);
        }
    }

}
