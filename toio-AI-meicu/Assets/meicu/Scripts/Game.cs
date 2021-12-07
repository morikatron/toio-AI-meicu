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
            if (!Device.isBothConnected) return;

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
            while (true)
            {
                var coordA = Device.ID2SpaceCoord(Device.cubes[1].x, Device.cubes[1].y);
                if (coordA.x == 4 && coordA.y == 4 && !Device.cubes[0].isGrounded)
                {
                    readyCallback?.Invoke(true);
                    break;
                }
                else
                {
                    readyCallback?.Invoke(false);
                }

                yield return new WaitForSecondsRealtime(0.5f);

                Device.TargetMove(1, 4, 4, -10, 10);
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

            stepCallbackP?.Invoke(res);

            if (Env.IsResponseFail(res))
                FailP();
            else if (res == Env.Response.Goal)
                SuccessP();
        }

        internal void MoveA(Env.Action action)
        {
            if (!inGame) return;
            if (stateA != PlayerState.InGame) return;

            var res = envA.Step(action);

            if (res != Env.Response.FailOut)
                trajA.Add(GetPosA());

            stepCallbackA?.Invoke(res);

            if (Env.IsResponseFail(res))
                FailA();
            else if (res == Env.Response.Goal)
                SuccessA();
        }


        private void FailP()
        {
            if (stateP != PlayerState.InGame) return;
            stateP = PlayerState.Fail;
            StartCoroutine(IE_Fail(0));

            this.overCallbackP?.Invoke(stateP);
            CheckOver();
        }
        private void FailA()
        {
            if (stateA != PlayerState.InGame) return;
            stateA = PlayerState.Fail;
            StartCoroutine(IE_Fail(1));

            this.overCallbackA?.Invoke(stateA);
            CheckOver();
        }

        private void SuccessP()
        {
            if (stateP != PlayerState.InGame) return;
            stateP = PlayerState.Success;
            this.overCallbackP?.Invoke(stateP);
            this.overCallbackA?.Invoke(stateA);
            CheckOver();
            StartCoroutine(IE_Success(0));
        }
        private void SuccessA()
        {
            if (stateA != PlayerState.InGame) return;
            stateA = PlayerState.Success;
            this.overCallbackA?.Invoke(stateA);
            this.overCallbackP?.Invoke(stateP);
            CheckOver();
            StartCoroutine(IE_Success(1));
        }

        private void CheckOver()
        {
            if (stateA == PlayerState.Success ||
                stateP == PlayerState.Success ||
                stateA == PlayerState.Fail && stateP == PlayerState.Fail)
            {
                inGame = false;
                overCallback?.Invoke(stateP, stateA);
            }
        }

        IEnumerator IE_Fail(int idx)
        {
            Device.cubes[idx].Move(20, -20, 1500, Cube.ORDER_TYPE.Strong);
            Device.cubes[idx].PlayPresetSound(2);
            yield break;
        }

        IEnumerator IE_Success(int idx)
        {
            yield return new WaitForSecondsRealtime(0.7f);
            Device.cubes[1-idx].Move(20, -20, 1500, Cube.ORDER_TYPE.Strong);
            for (int i=0; i<4; i++)
            {
                Device.cubes[idx].Move(30, 30, 400, Cube.ORDER_TYPE.Strong);
                yield return new WaitForSecondsRealtime(0.5f);
                Device.cubes[idx].Move(-30, -30, 400, Cube.ORDER_TYPE.Strong);
                yield return new WaitForSecondsRealtime(0.5f);
            }
        }




        public enum PlayerState
        {
            None, InGame, Fail, Success
        }
    }

}
