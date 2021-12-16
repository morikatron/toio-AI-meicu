using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;


namespace toio.AI.meicu
{

    public class PlayerController : BaseController
    {
        internal static PlayerController ins { get; private set; }

        public Game game;

        protected override int id => 0;

        internal bool isPause = false;

        IEnumerator ie_ChantActionP = null;
        Env.Action candidateActionP;
        bool isGameRunning = false;


        protected override void Awake()
        {
            base.Awake();
            ins = this;
            game.startCallback += OnGameStarted;
            game.stepCallbackP += OnGameStep;
            game.overCallbackP += OnGameOver;
            game.stopCallback += OnGameStop;
            game.retryCallbackP += OnGameRetry;
        }

        internal void Stop()
        {
            StopMotion();

            StopAllCoroutines();
            isGameRunning = false;
            ie_ChantActionP = null;
            lstCoord = new Vector2Int(-1, -1);
        }


        #region ======== Cube Callbacks ========
        Vector2Int lstCoord = new Vector2Int(-1, -1);
        protected override void OnCubeID(Cube c)
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
                        CancelChant();
                        ie_ChantActionP = IE_ChantActionP();
                        StartCoroutine(ie_ChantActionP);
                        candidateActionP = action;
                    }
                }
                else
                {
                    CancelChant();
                }
            }
        }

        protected override void OnCubeIDMissed(Cube c)
        {
            lstCoord = new Vector2Int(-1, -1);
            // if (!isGameRunning) return;

            CancelChant();
        }

        #endregion

        void CancelChant()
        {
            if (ie_ChantActionP != null)
            {
                StopCoroutine(ie_ChantActionP);
                AudioPlayer.ins.StopSE();
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
                if (t == 0)
                    AudioPlayer.ins.PlaySE(AudioPlayer.ESE.StepConfirming);
            }

            game.MoveP(candidateActionP);
            AudioPlayer.ins.PlaySE(AudioPlayer.ESE.StepConfirmed);
            ie_ChantActionP = null;
        }


        #region ======== Game Callbacks ========

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

        #endregion
    }

}
