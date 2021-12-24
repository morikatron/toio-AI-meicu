using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace toio.AI.meicu
{

    public class Env
    {
        public static readonly Env.Space[,] defaultState = {
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

        protected Env.Space[,] state;
        public Response response { get; protected set; } = Response.None;
        public Quest quest = null;

        [HideInInspector]
        public int row { get; protected set; } = 4;
        [HideInInspector]
        public int col { get; protected set; } = 4;

        [HideInInspector]
        public Env.Space spaceBoard { get {return defaultState[row, col];} }
        [HideInInspector]
        public Env.Space spaceState { get {return this.state[row, col];} }
        [HideInInspector]

        public int passedColorSpaceCnt { get; private set; } = 0;
        public int passedSpaceCnt { get; private set; } = 0;


        public Env Clone()
        {
            var c = new Env();
            c.state = (Env.Space[,]) this.state.Clone();
            c.passedColorSpaceCnt = this.passedColorSpaceCnt;
            c.passedSpaceCnt = this.passedSpaceCnt;
            c.SetQuest(this.quest);
            c.row = this.row;
            c.col = this.col;
            return c;
        }

        public void Reset(int row=4, int col=4)
        {
            this.row = Mathf.Clamp(row, 0, 8);
            this.col = Mathf.Clamp(col, 0, 8);

            this.state = (Env.Space[,]) defaultState.Clone();
            this.state[this.row, this.col] = Env.Space.Passed;

            this.passedColorSpaceCnt = 0;
            this.passedSpaceCnt = 0;
            this.response = Response.None;
        }

        public void SetQuest(Quest quest)
        {
            this.quest = quest;
        }

        public Response Step(Action action)
        {
            if (quest == null)
                return Response.None;
            if (Env.IsResponseFail(this.response))
                return this.response;

            if (!IsInside(action))
                this.response = Response.FailOut;
            else if (IsPassed(action))
            {
                this.response = Response.FailPassed;
                (row, col) = Translate(action ,row, col);
            }
            else
            {
                // Move
                (row, col) = Translate(action ,row, col);

                this.state[row, col] = Env.Space.Passed;
                passedSpaceCnt ++;
                if (spaceBoard != Env.Space.W)
                {
                    passedColorSpaceCnt ++;

                    // Stepped Wrong color
                    if (spaceBoard != quest.colors[passedColorSpaceCnt-1])
                        this.response = Response.FailWrong;
                    // Reach at Goal
                    else if (row == quest.goalRow && col == quest.goalCol)
                        // And complete quest
                        if (passedColorSpaceCnt == quest.Length)
                            this.response = Response.Goal;
                        // Before complete quest
                        else
                            this.response = Response.FailEarlyGoal;
                    // Not Reach Goal
                    else if (passedColorSpaceCnt == quest.Length)
                        this.response = Response.FailNotGoal;
                    // Other (Stepped correct color)
                    else
                        this.response = Response.StepColor;
                }
                else
                    this.response = Response.StepWhite;
            }

            return this.response;
        }

        private bool StepIgnoringQuest(Env.Action action)
        {
            if (!IsInside(action)) return false;

            (row, col) = Translate(action ,row, col);
            if (state[row, col] == Env.Space.Passed)
                return false;
            state[row, col] = Env.Space.Passed;
            return true;
        }

        public bool IsInside(int actionCode)
        {
            return IsInside((Env.Action) actionCode);
        }
        public bool IsInside(Env.Action action)
        {
            return IsInside(action, this.row, this.col);
        }
        public bool IsInside(Env.Action action, int row, int col)
        {
            (int r, int c) = Translate(action ,row, col);

            return !(r < 0 || r > 8 || c < 0 || c > 8 || this.state[r, c] == Env.Space.None);
        }

        public bool IsPassed(int actionCode)
        {
            return IsPassed((Env.Action) actionCode);
        }

        public bool IsPassed(Env.Action action)
        {
            (int r, int c) = Translate(action ,row, col);

            return this.state[r, c] == Env.Space.Passed;
        }

        public bool IsMoveable(int actionCod)
        {
            return IsInside(actionCod) && !IsPassed(actionCod);
        }

        public bool IsMoveable(Env.Action action)
        {
            return IsInside(action) && !IsPassed(action);
        }

        public static (int, int) Translate(Env.Action action, int row, int col)
        {
            int r = row;
            int c = col;
            if (action == Env.Action.Up) r--;
            else if (action == Env.Action.Right) c++;
            else if (action == Env.Action.Down) r++;
            else if (action == Env.Action.Left) c--;
            return (r, c);
        }

        public Env.Space[,] GetState()
        {
            return (Env.Space[,]) this.state.Clone();
        }

        public Env.Space GetSpace(int row, int col)
        {
            // Out of board
            if (row < 0 || row > 8 || col < 0 || col > 8)
                return Env.Space.None;
            return this.state[row, col];
        }

        public Env.Space[,] GetLocalObs(int size=9) // must be odd
        {
            Env.Space[,] obs = new Env.Space[size, size];

            for (int r=0; r<size; r++)
                for (int c=0; c<size; c++)
                {
                    int _r = this.row - size/2 + r;
                    int _c = this.col - size/2 + c;

                    if (_r < 0 || _r > 8 || _c < 0 || _c > 8)
                        obs[r, c] = Env.Space.Passed;
                    else
                        obs[r, c] = this.state[_r, _c];
                }

            return obs;
        }

        public Quest GenerateQuest(int size=2)
        {
            var env = this.Clone();

            var rand = new System.Random();
            int[] actions = {0, 1, 2, 3};

            List<Env.Space> colors = new List<Env.Space>(10);
            List<Env.Action> refActions = new List<Env.Action>();

            while (colors.Count < size)
            {
                // Sample Action
                var validActions = actions.Where(x => env.IsMoveable(x)).ToArray();
                if (validActions.Length == 0) break;
                var action = (Env.Action) validActions.OrderBy(_=>rand.NextDouble()).ToArray()[0];

                refActions.Add(action);
                env.StepIgnoringQuest(action);
                if (env.spaceBoard != Env.Space.W)
                {
                    colors.Add(env.spaceBoard);
                }
            }

            return new Quest(this.row, this.col, colors.ToArray(), env.row, env.col, refActions.ToArray());
        }

        public static Quest GenerateQuest(int size, int startRow=4, int startCol=4)
        {
            Env env = new Env();
            env.Reset(startRow, startCol);
            return env.GenerateQuest(size);
        }

        public enum Space
        {
            None, W, B, Y, R, G, Passed
        }

        public enum Action
        {
            Up, Right, Down, Left
        }

        public enum Response
        {
            None, StepWhite, StepColor, Goal, FailOut, FailPassed, FailWrong, FailEarlyGoal, FailNotGoal
        }

        public static bool IsResponseFail(Response res)
        {
            return res == Response.FailOut || res == Response.FailPassed || res == Response.FailEarlyGoal || res == Response.FailWrong || res == Response.FailNotGoal;
        }
    }

    public class Quest
    {
        public int startRow, startCol;
        public Env.Space[] colors;
        public int goalRow;
        public int goalCol;
        public Env.Action[] refActions;
        public Quest(int startRow, int startCol, Env.Space[] colors, int goalRow, int goalCol, Env.Action[] refActions=null)
        {
            this.startRow = startRow;
            this.startCol = startCol;
            this.colors = colors;
            this.goalRow = goalRow;
            this.goalCol = goalCol;
            this.refActions = refActions;
        }
        public int Length => colors.Length;

        public override int GetHashCode()
        {
            int code = 0;
            foreach (var c in colors)
            {
                code ^= c.GetHashCode();
            }
            code ^= startRow.GetHashCode();
            code ^= startCol.GetHashCode();
            code ^= goalRow.GetHashCode();
            code ^= goalCol.GetHashCode();
            return code;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var other = obj as Quest;

            if (Length != other.Length) return false;
            for (int i=0; i<Length; i++)
                if (colors[i] != other.colors[i])
                    return false;
            return goalRow == other.goalRow && goalCol == other.goalCol && startRow == other.startRow && startCol == other.startCol;
        }
    }

}
