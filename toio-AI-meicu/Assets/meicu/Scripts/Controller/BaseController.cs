using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;


namespace toio.AI.meicu
{
    public class BaseController : MonoBehaviour
    {
        protected virtual int id { get; } = -1;
        protected Cube cube => Device.GetCube(id);
        protected IEnumerator ieMotion;
        protected bool isPerforming = false;

        internal bool isGrounded { get {
            if (cube == null) return false;
            if (!cube.isConnected) return false;
            return cube.isGrounded;
        }}
        internal bool isConnected { get {
            if (cube == null) return false;
            return cube.isConnected;
        }}


        protected virtual void Awake()
        {
            Device.Assign(id);
            Device.connectionCallback += OnConnection;
        }

        protected virtual void OnConnection(int id, bool connected)
        {
            if (this.id != id) return;

            if (cube != null && connected)
            {
                Debug.Log(id);
                cube.idCallback.ClearListener();
                cube.idMissedCallback.ClearListener();
                cube.idCallback.AddListener(id.ToString(), OnCubeID);
                cube.idMissedCallback.AddListener(id.ToString(), OnCubeIDMissed);
            }
        }
        protected virtual void OnCubeID(Cube cube) {}
        protected virtual void OnCubeIDMissed(Cube cube) {}

        internal bool IsAtCenter => Device.IsAtSpace(id, 4, 4);

        internal virtual void StopMotion(bool sendCmd = false)
        {
            if (ieMotion != null)
            {
                StopCoroutine(ieMotion);
                ieMotion = null;
            }
            isPerforming = false;

            if (sendCmd)
            {
                cube.Move(0, 0, 0, Cube.ORDER_TYPE.Strong);
            }
        }


        #region ======== Performing ========

        internal virtual void PerformHappy()
        {
            StopMotion();
            ieMotion = IE_PerformHappy();
            StartCoroutine(ieMotion);
        }

        protected virtual IEnumerator IE_PerformHappy()
        {
            isPerforming = true;
            int u = 30;
            int durationMs = 400;
            float interval = 0.5f;
            for (int i = 0; i < 3; i++)
            {
                yield return new WaitForSecondsRealtime(interval);
                cube.Move(u, u, durationMs, Cube.ORDER_TYPE.Strong);    // TODO use handle instead
                yield return new WaitForSecondsRealtime(interval);
                cube.Move(-u, -u, durationMs, Cube.ORDER_TYPE.Strong);
            }
            ieMotion = null;
            isPerforming = false;
        }

        internal virtual void PerformSad()
        {
            StopMotion();
            ieMotion = IE_PerformSad();
            StartCoroutine(ieMotion);
        }

        protected virtual IEnumerator IE_PerformSad()
        {
            isPerforming = true;
            int u = 25;
            int intervalMs = 500;
            for (int i = 0; i < 3; i++)
            {
                yield return new WaitForSecondsRealtime(intervalMs/1000f);
                cube.Move(u, -u, intervalMs, Cube.ORDER_TYPE.Strong);
                yield return new WaitForSecondsRealtime(intervalMs/1000f);
                cube.Move(-u, u, intervalMs, Cube.ORDER_TYPE.Strong);
            }
            ieMotion = null;
            isPerforming = false;
        }

        internal virtual void PerformRegret()
        {
            StopMotion();
            ieMotion = IE_PerformRegret();
            StartCoroutine(ieMotion);
        }

        protected virtual IEnumerator IE_PerformRegret()
        {
            isPerforming = true;
            int u = 25;
            yield return new WaitForSecondsRealtime(0.5f);
            cube.Move(u, -u, 1500, Cube.ORDER_TYPE.Strong);
            yield return new WaitForSecondsRealtime(2f);
            cube.Move(-u, u, 1500, Cube.ORDER_TYPE.Strong);
            yield return new WaitForSecondsRealtime(0.5f);
            ieMotion = null;
            isPerforming = false;
        }

        #endregion
    }
}