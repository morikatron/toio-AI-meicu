using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;


namespace toio.AI.meicu
{

    public class PlayerController : MonoBehaviour
    {
        internal static PlayerController ins { get; private set; }

        public Game game;
        internal bool isPause = false;

        Cube cube;
        IEnumerator ie_ChantActionP = null;
        Env.Action candidateActionP;
        bool isGameRunning = false;


        void OnEnable()
        {
            ins = this;
            game.startCallback += OnGameStarted;
            game.stepCallbackP += OnGameStep;
            game.overCallbackP += OnGameOver;
            game.stopCallback += OnGameStop;
            game.retryCallbackP += OnGameRetry;
        }

        internal void Stop()
        {
            isGameRunning = false;
            StopAllCoroutines();
            ie_ChantActionP = null;
            lstCoord = new Vector2Int(-1, -1);
        }

        internal void Init()
        {
            if (Device.cubes.Count >= 1)
            {
                cube = Device.cubes[0];
                cube.idCallback.AddListener("P", OnID);
                cube.idMissedCallback.AddListener("P", OnIDMissed);
            }
        }

        Vector2Int lstCoord = new Vector2Int(-1, -1);
        void OnID(Cube c)
        {
            if (!isGameRunning) return;
            if (isPause) return;

            var pos = game.GetPosP();

            var coord = Device.ID2SpaceCoord(c.x, c.y);
            int row = coord.x; int col = coord.y;
            if (coord != lstCoord)
            {
                lstCoord = coord;
                bool valid = false;
                Env.Action action = default;
                if (row == pos.x+1 && col == pos.y)
                {
                    action = Env.Action.Down;
                    valid = true;
                }
                else if (row == pos.x-1 && col == pos.y)
                {
                    action = Env.Action.Up;
                    valid = true;
                }
                else if (row == pos.x && col == pos.y+1)
                {
                    action = Env.Action.Right;
                    valid = true;
                }
                else if (row == pos.x && col == pos.y-1)
                {
                    action = Env.Action.Left;
                    valid = true;
                }

                if (valid)
                {
                    if (ie_ChantActionP == null || candidateActionP != action)
                    {
                        if (ie_ChantActionP != null) StopCoroutine(ie_ChantActionP);
                        ie_ChantActionP = IE_ChantActionP();
                        StartCoroutine(ie_ChantActionP);
                        candidateActionP = action;
                    }
                }
                else
                {
                    if (ie_ChantActionP != null) StopCoroutine(ie_ChantActionP);
                    ie_ChantActionP = null;
                }
            }
        }

        void OnIDMissed(Cube c)
        {
            lstCoord = new Vector2Int(-1, -1);
            if (!isGameRunning) return;

            if (ie_ChantActionP != null)
            {
                StopCoroutine(ie_ChantActionP);
                ie_ChantActionP = null;
            }
        }


        IEnumerator IE_ChantActionP()
        {
            for (float t = 0; t < 1; t += 0.2f)
            {
                yield return new WaitForSecondsRealtime(0.2f);
                if (!isGameRunning || isPause)
                {
                    ie_ChantActionP = null;
                    yield break;
                }
                Cube.SoundOperation sound = new Cube.SoundOperation(200, 100, (byte)(50+t*10));
                cube.PlaySound(1, new Cube.SoundOperation[]{sound}, Cube.ORDER_TYPE.Weak);
            }

            game.MoveP(candidateActionP);
            cube.PlayPresetSound(1);
            ie_ChantActionP = null;
        }



        void OnGameStarted(int countDown)
        {
            if (countDown == 0)
            {
                isGameRunning = true;
                lstCoord = new Vector2Int(-1, -1);
            }
        }
        void OnGameRetry()
        {
            isGameRunning = true;
            lstCoord = new Vector2Int(-1, -1);
        }

        void OnGameStep(Env.Response res)
        {

        }

        void OnGameOver(Game.PlayerState state)
        {
            Stop();
        }

        void OnGameStop()
        {
            Stop();
        }
    }

}
