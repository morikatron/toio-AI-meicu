using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;


namespace toio.AI.meicu
{

    public class Game : MonoBehaviour
    {
        internal Env envP;
        internal Env envA;

        internal MeiQuest quest;
        internal bool inGame { get; private set; } = false;

        internal PlayerState stateP { get; private set; }
        internal PlayerState stateA { get; private set; }
        List<Vector2Int> trajP, trajA;


        // Callbacks
        internal event Action initedCallback;
        internal event Action<bool> readyCallback;
        internal event Action<int> startCallback;
        internal event Action<PlayerState, PlayerState> overCallback;
        internal event Action<PlayerState> overCallbackA;
        internal event Action<PlayerState> overCallbackP;
        internal event Action<Env.Response> stepCallbackP;
        internal event Action<Env.Response> stepCallbackA;
        internal event Action stopCallback;
        internal event Action retryCallbackP;

        void OnEnable()
        {
            this.envP = new Env();
            this.envA = new Env();
            this.trajP = new List<Vector2Int>();
            this.trajA = new List<Vector2Int>();
        }

        #region GET
        internal Vector2Int GetPosP()
        {
            return new Vector2Int(envP.row, envP.col);
        }
        internal Vector2Int GetPosA()
        {
            return new Vector2Int(envA.row, envA.col);
        }
        internal Vector2Int[] GetTrajP()
        {
            return this.trajP.ToArray();
        }
        internal Vector2Int[] GetTrajA()
        {
            return this.trajA.ToArray();
        }
        #endregion


        internal void InitGame(MeiQuest quest)
        {
            StopGame();

            envP.Reset();
            envA.Reset();

            this.quest = quest;

            envP.SetQuest(quest);
            envA.SetQuest(quest);

            initedCallback?.Invoke();
        }

        internal void InitGame(int size = 0, bool keepQuest = false)
        {
            StopGame();

            envP.Reset();
            envA.Reset();

            if (!keepQuest || quest == null)
            {
                if (size == 0)
                    size = UnityEngine.Random.Range(3, 7);
                quest = envP.GenerateQuest(size);
            }

            envP.SetQuest(quest);
            envA.SetQuest(quest);

            initedCallback?.Invoke();
        }

        internal void WaitReady()
        {
            StartCoroutine(IE_WaitReady());
        }

        internal void StartGame()
        {
            if (!Device.isTwoConnected) return;

            StartCoroutine(IE_Starting());
        }

        internal void StopGame()
        {
            inGame = false;

            StopAllCoroutines();

            inGame = false;
            stateP = PlayerState.None;
            stateA = PlayerState.None;
            trajP.Clear();
            trajA.Clear();
            stopCallback?.Invoke();
        }

        IEnumerator IE_WaitReady()
        {
            // AI go home
            AIController.ins.RequestMove(4, 4);

            while (true)
            {
                if (AIController.ins.IsAtCenter && !PlayerController.ins.isGrounded)
                {
                    readyCallback?.Invoke(true);
                    break;
                }
                else
                {
                    readyCallback?.Invoke(false);
                }

                yield return new WaitForSecondsRealtime(0.5f);
            }
        }

        IEnumerator IE_Starting()
        {
            // Count Down
            for (int t = 3; t >0; t--)
            {
                startCallback?.Invoke(t);
                yield return new WaitForSecondsRealtime(1f);
            }

            // Start
            stateP = PlayerState.InGame;
            stateA = PlayerState.InGame;

            inGame = true;

            startCallback?.Invoke(0);
        }


        internal void RetryP()
        {
            if (!inGame) return;
            // if (stateP != PlayerState.Fail) return;

            stateP = PlayerState.InGame;
            trajP.Clear();
            envP.Reset();

            retryCallbackP?.Invoke();
        }


        internal void MoveP(Env.Action action)
        {
            if (!inGame) return;
            if (stateP != PlayerState.InGame) return;

            var res = envP.Step(action);

            if (res != Env.Response.FailOut)
                trajP.Add(GetPosP());

            // Fail
            if (Env.IsResponseFail(res))
            {
                stateP = PlayerState.Fail;

                if (stateA == PlayerState.Fail)
                {
                    inGame = false;
                    stateP = PlayerState.Draw;
                    stateA = PlayerState.Draw;
                }

                stepCallbackP?.Invoke(res);
                overCallbackP?.Invoke(stateP);
                if (!inGame) overCallbackA?.Invoke(stateA);
                if (!inGame) overCallback?.Invoke(stateP, stateA);
            }
            // Goal
            else if (res == Env.Response.Goal)
            {
                stateP = PlayerState.Win;
                inGame = false;
                if (stateA == PlayerState.Fail)
                {
                    stateA = PlayerState.LoseFail;
                }
                else
                {
                    stateA = PlayerState.LoseNotFail;
                }

                stepCallbackP?.Invoke(res);
                overCallbackP?.Invoke(stateP);
                overCallbackA?.Invoke(stateA);
                overCallback?.Invoke(stateP, stateA);
            }
            // Nothing
            else
                stepCallbackP?.Invoke(res);
        }

        internal void MoveA(Env.Action action)
        {
            if (!inGame) return;
            if (stateA != PlayerState.InGame) return;

            var res = envA.Step(action);

            if (res != Env.Response.FailOut)
                trajA.Add(GetPosA());

            // Fail
            if (Env.IsResponseFail(res))
            {
                stateA = PlayerState.Fail;

                if (stateP == PlayerState.Fail)
                {
                    inGame = false;
                    stateA = PlayerState.Draw;
                    stateP = PlayerState.Draw;
                }

                stepCallbackA?.Invoke(res);
                overCallbackA?.Invoke(stateA);
                if (!inGame) overCallbackP?.Invoke(stateP);
                if (!inGame) overCallback?.Invoke(stateP, stateA);
            }
            // Goal
            else if (res == Env.Response.Goal)
            {
                stateA = PlayerState.Win;
                inGame = false;
                if (stateP == PlayerState.Fail)
                {
                    stateP = PlayerState.LoseFail;
                }
                else
                {
                    stateP = PlayerState.LoseNotFail;
                }

                stepCallbackA?.Invoke(res);
                overCallbackA?.Invoke(stateA);
                overCallbackP?.Invoke(stateP);
                overCallback?.Invoke(stateP, stateA);
            }
            // Nothing
            else
                stepCallbackA?.Invoke(res);
        }


        public enum PlayerState
        {
            None, InGame, Win, Fail, LoseFail, LoseNotFail, Draw
        }
    }

}
