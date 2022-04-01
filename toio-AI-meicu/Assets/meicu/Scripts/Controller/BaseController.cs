using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;
using toio.Navigation;
using toio.MathUtils;


namespace toio.AI.meicu
{
    public class BaseController : MonoBehaviour
    {
        internal virtual int id => -1;
        internal Cube cube => Device.GetCube(id);
        internal CubeHandle handle => Device.GetHandle(id);
        protected IEnumerator ieMotion;
        internal bool isAvoidEnabled = false;
        internal bool isPerforming { get; private set; } = false;
        internal bool isMoving { get; private set; } = false;
        public Vector2Int targetCoords {get; protected set;}
        internal Vector2Int targetMatCoordBias { get; set; } = Vector2Int.zero;

        internal Vector2 coordsMat => this.cube.pos;
        internal Vector2Int coordsSpace => Device.ID2SpaceCoord(this.cube.x, this.cube.y);
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
            CubeCoordinater.RegisterController(this);
        }

        protected virtual void OnConnection(int id, bool connected)
        {
            if (this.id != id) return;

            if (cube != null && connected)
            {
                cube.idCallback.ClearListener();
                cube.idMissedCallback.ClearListener();
                cube.targetMoveCallback.ClearListener();
                cube.idCallback.AddListener(id.ToString(), OnCubeID);
                cube.idMissedCallback.AddListener(id.ToString(), OnCubeIDMissed);
                cube.targetMoveCallback.AddListener(id.ToString(), OnCubeTargetMove);
            }

            PerformWake();
        }
        protected virtual void OnCubeID(Cube cube) {}
        protected virtual void OnCubeIDMissed(Cube cube) {}
        protected virtual void OnCubeTargetMove(Cube cube, int configId, Cube.TargetMoveRespondType type) {
            CubeCoordinater.UpdateNavi(this, 0, 0);
            hasResponseTargetMove = true;
            targetMoveResponse = type;
        }

        protected bool hasResponseTargetMove = false;
        protected Cube.TargetMoveRespondType targetMoveResponse;

        internal bool IsAtCenter => Device.IsAtSpace(id, 4, 4);

        internal virtual void StopMotion(bool sendCmd = false)
        {
            isMoving = false;
            isPerforming = false;
            if (ieMotion != null)
            {
                StopCoroutine(ieMotion);
                ieMotion = null;
            }

            if (sendCmd)
            {
                cube.Move(0, 0, 0, Cube.ORDER_TYPE.Strong);
            }
        }

        internal void Move2Center()
        {
            Device.TargetMove(id, 4, 4, targetMatCoordBias.x, targetMatCoordBias.y);
        }


        // Start ienumerator which trys TargetMove in loop until target reached or overwritten.
        internal void RequestMove(int row, int col, byte spd = 30, float confirmTime = 0.5f)
        {
            if (ieMotion != null && !isPerforming && this.targetCoords.x == row && this.targetCoords.y == col)
                return;
            this.targetCoords = new Vector2Int(row, col);

            StopMotion();
            ieMotion = isAvoidEnabled? IE_Navi(spd, confirmTime) : IE_Move(spd, confirmTime);
            StartCoroutine(ieMotion);
        }

        // Loop of moving to target
        protected IEnumerator IE_Move(byte spd, float confirmTime, bool timeCorrection = false)
        {
            // Debug.Log($"IE_Move : Begin");
            isMoving = true;

            float timeout = 3;
            float retryTime = timeout + 1;
            float t = Time.realtimeSinceStartup;

            if (!CubeCoordinater.CanMoveStraight(this))
            {
                Debug.Log($"Con#{id} wait for move.");
                yield return new WaitUntil(() => CubeCoordinater.CanMoveStraight(this));
                Debug.Log($"Con#{id} wait over.");
            }

            // Wait Cube to Arrive
            while (Device.ID2SpaceCoord(cube.x, cube.y) != targetCoords)
            {
                // Wait while disconnected
                if (!cube.isConnected)
                {
                    yield return new WaitUntil(()=>cube.isConnected);
                    retryTime = timeout + 1;
                }
                // Move a bit if position ID missed
                else if (!cube.isGrounded)
                {
                    cube.Move(20, -20, 400, Cube.ORDER_TYPE.Strong);
                    retryTime = timeout + 1;
                }
                // Re-send command if timeout
                else if (retryTime > timeout)
                {
                    retryTime = 0;
                    Debug.Log($"IE_Move : TargetMove({targetCoords.x}, {targetCoords.y})");
                    Device.TargetMove(id, targetCoords.x, targetCoords.y, targetMatCoordBias.x, targetMatCoordBias.y, maxSpd:spd);
                }

                yield return new WaitForSecondsRealtime(0.1f);
                retryTime += 0.1f;
            }

            // Simulate confirm time
            if (timeCorrection)
                confirmTime = confirmTime - (Time.realtimeSinceStartup - t - 25f/spd);
            yield return new WaitForSecondsRealtime(confirmTime);

            isMoving = false;
        }

        protected IEnumerator IE_Navi(byte spd, float confirmTime, bool timeCorrection = false)
        {
            isMoving = true;

            bool waitResponse = false;

            float timeout = 1f;
            float retryTime = timeout + 1;
            float t = Time.realtimeSinceStartup;
            int stuckCnt = 0;
            Vector2 target_center = Device.SpaceCoords2ID(targetCoords.x, targetCoords.y);
            Vector2 target_original = target_center + targetMatCoordBias;
            Vector2 target = target_original;
            Vector2 waypoint = default;

            // Wait Cube to Arrive
            while (Device.ID2SpaceCoord(cube.x, cube.y) != targetCoords)
            {
                // Wait while disconnected
                if (!cube.isConnected)
                {
                    yield return new WaitUntil(()=>cube.isConnected);
                    waitResponse = false; hasResponseTargetMove = false;
                    retryTime = timeout + 1;
                    stuckCnt = 0;
                    continue;
                }
                // Move a bit if position ID missed
                else if (!cube.isGrounded)
                {
                    cube.Move(20, -20, 400, Cube.ORDER_TYPE.Strong);
                    waitResponse = false; hasResponseTargetMove = false;
                    retryTime = timeout + 1;
                    stuckCnt = 0;
                    yield return new WaitForSecondsRealtime(0.1f);
                    continue;
                }
                else if (waitResponse && hasResponseTargetMove)
                {
                    waitResponse = false;

                    // Target Reached
                    if (Vector2.Distance(target, waypoint) < 5 && targetMoveResponse == Cube.TargetMoveRespondType.Normal)
                        break;

                    // Avoid sending command too frequently
                    if (retryTime < 0.5f)
                        yield return new WaitForSecondsRealtime(0.5f - retryTime);
                    retryTime = timeout + 1;
                    continue;
                }

                // Re-send command
                if (retryTime > timeout)
                {
                    // Update states
                    CubeCoordinater.UpdateAllNaviPos();

                    // // Calc. Target
                    // Vector2 bias = targetMatCoordBias;
                    // // Try to change direction of bias
                    // bias = new Vector2((stuckCnt/4%4<2?1:-1) * bias.x, (stuckCnt/4%2<1?1:-1) * bias.y);
                    // // Try to increase range of bias
                    // bias = bias * (1 + (stuckCnt/4/4) * 0.5f);
                    // target = target_center + bias;

                    target = CubeCoordinater.GetBestTargetInSpace(this, target_center, target_original);

                    // Run Avoid Algo.
                    (Vector wp, bool isCol, double spdLimit) = CubeCoordinater.RunAvoid(this, target, spd);
                    waypoint = new Vector2((float)wp.x, (float)wp.y);

                    // Stuck
                    if (Vector2.Distance(cube.pos, waypoint) < 10 && Vector2.Distance(target, waypoint) > 5
                        && !CubeCoordinater.IsAnyOtherMoving(this))
                        stuckCnt ++;
                    else stuckCnt = 0;

                    // Handle Stuck
                    if (stuckCnt >= 2)
                    {
                        waypoint = CubeCoordinater.GetWaypointAroundOther(this, target);
                    }

                    // TargetMove to Waypoint
                    int wx = Mathf.RoundToInt(waypoint.x) + cube.x;
                    int wy = Mathf.RoundToInt(waypoint.y) + cube.y;
                    // Debug.Log($"[{id}]IE_Navi : TargetMoveByID({wx}, {wy}) stucked={stuckCnt >= 2}");
                    Device.TargetMoveByID(id, wx, wy, maxSpd:spd);
                    waitResponse = true;
                    hasResponseTargetMove = false;

                    retryTime = 0;
                }

                yield return new WaitForSecondsRealtime(0.1f);
                retryTime += 0.1f;
            }

            // Simulate confirm time
            if (timeCorrection)
                confirmTime = confirmTime - (Time.realtimeSinceStartup - t - 25f/spd);
            yield return new WaitForSecondsRealtime(confirmTime);

            isMoving = false;
        }


        #region ======== Performing ========

        internal virtual void PerformThink()
        {
            StopMotion();
            ieMotion = IE_PerformThink();
            StartCoroutine(ieMotion);
        }

        protected virtual IEnumerator IE_PerformThink()
        {
            isPerforming = true;
            var t0 = Time.realtimeSinceStartup;
            int dir = UnityEngine.Random.Range(-1f, 1f) > 0f? 1 : -1;
            while (Time.realtimeSinceStartup - t0 < 10f)
            {
                var dur = UnityEngine.Random.Range(0.3f, 0.7f);
                cube.Move(30*dir, -30*dir, (int)(dur*1000), Cube.ORDER_TYPE.Strong);
                yield return new WaitForSecondsRealtime(dur + 0.6f);
                dir = -dir;
            }
            ieMotion = null;
            isPerforming = false;
        }

        internal virtual void PerformWake()
        {
            StopMotion();
            IEnumerator ie()
            {
                isPerforming = true;
                cube.Move(-80, 80, 500, order: Cube.ORDER_TYPE.Strong);
                yield return new WaitForSecondsRealtime(0.5f);
                ieMotion = null;
                isPerforming = false;
            }
            ieMotion = ie();
            StartCoroutine(ieMotion);
        }

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
                handle.Update();
                handle.Move(u, 0, durationMs, order: Cube.ORDER_TYPE.Strong);
                yield return new WaitForSecondsRealtime(interval);
                handle.Update();
                handle.Move(-u, 0, durationMs, order: Cube.ORDER_TYPE.Strong);
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



    internal static class CubeCoordinater
    {
        private static List<BaseController> cons = new List<BaseController>();
        private static Dictionary<BaseController, CNavigator> conNaviDict = new Dictionary<BaseController, CNavigator>();


        internal static void RegisterController(BaseController con)
        {
            if (cons.Contains(con)) return;

            cons.Add(con);
            CNavigator navi = new CNavigator();
            navi.avoid.margin = 15;
            navi.ClearWall();
            navi.AddBorder(10, 545, 955, 45, 455);
            navi.Update(con.cube, 0, 0);
            conNaviDict.Add(con, navi);
        }

        internal static void UpdateNavi(BaseController con, double spdL, double spdR)
        {
            if (!cons.Contains(con)) return;
            var navi = conNaviDict[con];
            navi.Update(con.cube, spdL, spdR);
        }
        internal static void UpdateAllNaviPos()
        {
            foreach (var con in cons)
            {
                var navi = conNaviDict[con];
                navi.Update(con.cube);
            }
        }

        internal static (Vector, bool, double) RunAvoid(BaseController con, Vector2 target, double spd)
        {
            if (!cons.Contains(con)) return (default, false, 0);
            // UpdateNavi(con, spd, spd);

            var navi = conNaviDict[con];

            List<Navigator> others = new List<Navigator>();
            foreach (var o in cons)
                if (o != con) others.Add(conNaviDict[o]);

            Entity tar = new Entity(new Vector(target.x, target.y));
            List<Wall> walls = new List<Wall>(1);

            // (Vector waypoint, bool isCollision, double spdLimit)
            return navi.avoid.RunTowards(others, tar, walls);
        }


        internal static bool CanMoveStraight(BaseController con, float minDistance = 25)
        {
            var pos = con.coordsMat;
            var tar = Device.SpaceCoords2ID(con.targetCoords.x, con.targetCoords.y) + con.targetMatCoordBias;

            foreach (var o in cons)
            {
                if (o == con) continue;

                var opos = o.coordsMat;
                Vector2 otar = Device.SpaceCoords2ID(o.targetCoords.x, o.targetCoords.y) + o.targetMatCoordBias;
                // if (!o.isMoving) otar = opos;
                if (!o.isMoving) continue;

                if (IsSegCollideSeg(pos, tar, opos, otar, minDistance)) return false;
            }
            return true;
        }

        internal static Vector2 GetBestTargetInSpace(BaseController con, Vector2 center, Vector2 prefer)
        {
            BaseController other = null;
            foreach (var o in cons)
                if (o != con)
                    other = o;

            Vector2 opos = new Vector2(other.cube.x, other.cube.y);
            if (Vector2.Distance(opos, prefer) > 30 || other.isMoving)
                return prefer;

            var dir = (center - opos).normalized;
            return center + dir * 10;
        }
        internal static bool IsAnyOtherMoving(BaseController con)
        {
            foreach (var o in cons)
            {
                if (o != con)
                {
                    if (o.isMoving) return true;
                }
            }
            return false;
        }
        internal static Vector2 GetWaypointAroundOther(BaseController con, Vector2 target)
        {
            BaseController other = null;
            foreach (var o in cons)
                if (o != con)
                    other = o;
            Vector2 opos = new Vector2(other.cube.x, other.cube.y);
            Vector2 pos = new Vector2(con.cube.x, con.cube.y);

            var self_o = opos - pos;
            var left = new Vector2(-self_o.y, self_o.x).normalized;
            var wp_left = (-self_o).normalized * 32 + left * 64 + opos;
            var wp_right = (-self_o).normalized * 32 - left * 64 + opos;

            if (wp_left.x < 545 || wp_left.x > 955 || wp_left.y < 45 || wp_left.y > 455)
                return wp_right;
            if (wp_right.x < 545 || wp_right.x > 955 || wp_right.y < 45 || wp_right.y > 455)
                return wp_left;
            if (Vector2.Dot(target - pos, left) > 0)
                return wp_left;
            else
                return wp_right;
        }

        internal static (Vector2Int, Vector2Int) MoveAllHome()
        {
            if (cons.Count < 2) return (default, default);

            var conA = AIController.ins;
            var conP = PlayerController.ins;

            var start = Device.SpaceCoords2ID(4, 4);
            Vector tar0 = new Vector(start.x + conP.targetMatCoordBias.x, start.y + conP.targetMatCoordBias.y);
            Vector tar1 = new Vector(start.x + conA.targetMatCoordBias.x, start.y + conA.targetMatCoordBias.y);
            Vector pos0 = new Vector(conP.cube.x, conP.cube.y);
            Vector pos1 = new Vector(conA.cube.x, conA.cube.y);

            var dpos = pos1 - pos0;
            var dtar = tar1 - tar0;

            // error of rad
            var erad = Utils.Rad((pos1 - pos0).rad - (tar1 - tar0).rad);

            if (Utils.AbsRad(erad) < Utils.Deg2Rad(45))
            {
                Device.TargetMoveByID(conP.id, (int)tar0.x, (int)tar0.y);
                Device.TargetMoveByID(conA.id, (int)tar1.x, (int)tar1.y);
                return (new Vector2Int((int)tar0.x, (int)tar0.y), new Vector2Int((int)tar1.x, (int)tar1.y));
            }
            else
            {
                double sign = Math.Sign(erad);
                // mostRotVec is the vector that rotate con0-con1 mostly (without collision)
                var mostRotVec1 = Vector.fromRadMag(dpos.rad - sign * 1, 30) + pos0 - pos1;
                var mostRotVec0 = Vector.fromRadMag((-dpos).rad - sign * 1, 30) + pos1 - pos0;
                Debug.Log($"mostRotVec1 {mostRotVec1}, mostRotVec0 {mostRotVec0}");

                // TODO consider speed

                // leastRotVec is the vector that rotate con0-con1 least, i.e. they move in opposite direction which means no rotation.
                var leastRotVec1 = dpos;
                var leastRotVec0 = -dpos;

                // pick optimal direction vector to target, from [leastRotVec, mostRotVec]
                bool directToTarget1 = (Utils.Rad((tar1 - pos1).rad - mostRotVec1.rad)*sign + 2*Math.PI) % (2*Math.PI) <
                                        (Utils.Rad(leastRotVec1.rad - mostRotVec1.rad)*sign + 2*Math.PI) % (2*Math.PI);
                bool directToTarget0 = (Utils.Rad((tar0 - pos0).rad - mostRotVec0.rad)*sign + 2*Math.PI) % (2*Math.PI) <
                                        (Utils.Rad(leastRotVec0.rad - mostRotVec0.rad)*sign + 2*Math.PI) % (2*Math.PI);
                Debug.Log($"directToTarget1 {directToTarget1}, directToTarget0 {directToTarget0}");

                Vector vec1, vec0;
                if (directToTarget1)
                    vec1 = tar1 - pos1;
                else
                {
                    if (Utils.AbsRad((tar1 - pos1).rad - mostRotVec1.rad) <= Utils.AbsRad((tar1 - pos1).rad - leastRotVec1.rad))
                        vec1 = mostRotVec1.unit;
                    else
                        vec1 = leastRotVec1.unit;
                    // Step size
                    vec1 = Math.Max(0, (tar1 - pos1) * vec1) * vec1;
                }
                if (directToTarget0)
                    vec0 = tar0 - pos0;
                else
                {
                    if (Utils.AbsRad((tar0 - pos0).rad - mostRotVec0.rad) <= Utils.AbsRad((tar0 - pos0).rad - leastRotVec0.rad))
                        vec0 = mostRotVec0.unit;
                    else
                        vec0 = leastRotVec0.unit;
                    // Step size
                    vec0 = Math.Max(0, (tar0 - pos0) * vec0) * vec0;
                }
                Debug.Log($"vec1 {vec1}, vec0 {vec0}");

                // Border
                if ((545 - pos1.x)/vec1.x > 0) vec1 *= Math.Min(1, (545 - pos1.x)/vec1.x);
                if ((955 - pos1.x)/vec1.x > 0) vec1 *= Math.Min(1, (955 - pos1.x)/vec1.x);
                if (( 45 - pos1.y)/vec1.y > 0) vec1 *= Math.Min(1, ( 45 - pos1.y)/vec1.y);
                if ((455 - pos1.y)/vec1.y > 0) vec1 *= Math.Min(1, (455 - pos1.y)/vec1.y);

                if ((545 - pos0.x)/vec0.x > 0) vec0 *= Math.Min(1, (545 - pos0.x)/vec0.x);
                if ((955 - pos0.x)/vec0.x > 0) vec0 *= Math.Min(1, (955 - pos0.x)/vec0.x);
                if (( 45 - pos0.y)/vec0.y > 0) vec0 *= Math.Min(1, ( 45 - pos0.y)/vec0.y);
                if ((455 - pos0.y)/vec0.y > 0) vec0 *= Math.Min(1, (455 - pos0.y)/vec0.y);

                // Move
                int wx1 = (int)(pos1.x + vec1.x);
                int wy1 = (int)(pos1.y + vec1.y);
                int wx0 = (int)(pos0.x + vec0.x);
                int wy0 = (int)(pos0.y + vec0.y);

                Device.TargetMoveByID(conP.id, wx0, wy0);
                Device.TargetMoveByID(conA.id, wx1, wy1);
                // Debug.Log($"wx0 {wx0}, wy0 {wy0} wx1 {wx1}, wy1 {wy1}");
                return (new Vector2Int(wx0, wy0), new Vector2Int(wx1, wy1));
            }
        }

        private static bool IsSegCollideSeg(Vector2 seg1Start, Vector2 seg1End, Vector2 seg2Start, Vector2 seg2End, float minDistance)
        {
            // Parallel
            var d = (seg1Start.x - seg1End.x) * (seg2Start.y - seg2End.y) - (seg1Start.y - seg1End.y) * (seg2Start.x - seg2End.x);
            if (d == 0)
            {
                if (seg1Start == seg1End) return minDistance > DistancePoint2Seg(seg1Start, seg2Start, seg2End);
                var foot = ProjectPoint2Seg(seg2Start, seg1Start, seg1End);
                return minDistance > Vector2.Distance(foot, seg2Start);
            }

            // Intersection
            var k1 = ((seg1Start.x - seg2Start.x) * (seg2Start.y - seg2End.y) - (seg1Start.y - seg2Start.y) * (seg2Start.x - seg2End.x)) / d;
            var k2 = ((seg1Start.x - seg2Start.x) * (seg1Start.y - seg1End.y) - (seg1Start.y - seg2Start.y) * (seg1Start.x - seg1End.x)) / d;
            if (0 <= k1 && k1 <= 1 && 0 <= k2 && k2 <= 1) return true;

            // Min Distance
            var dist = DistancePoint2Seg(seg2Start, seg1Start, seg1End);
            dist = Mathf.Min(dist, DistancePoint2Seg(seg2End, seg1Start, seg1End));
            dist = Mathf.Min(dist, DistancePoint2Seg(seg1Start, seg2Start, seg2End));
            dist = Mathf.Min(dist, DistancePoint2Seg(seg1End, seg2Start, seg2End));
            return minDistance > dist;
        }
        private static float DistancePoint2Seg(Vector2 point, Vector2 segStart, Vector2 segEnd)
        {
            var proj = ProjectPoint2Seg(point, segStart, segEnd);
            return Vector2.Distance(proj, point);
        }
        private static Vector2 ProjectPoint2Seg(Vector2 point, Vector2 segStart, Vector2 segEnd)
        {
            var len = Vector2.Distance(segStart, segEnd);
            if (len == 0) return segStart;

            var k = Mathf.Clamp01(Vector2.Dot(point-segStart, segEnd-segStart)/len/len);
            var proj = segStart + k * (segEnd - segStart);
            return proj;
        }

    }


    internal class CEntity : Entity
    {
        public void Update(Cube cube, double spdL, double spdR){
            if (cube == null) return;
            this.x = cube.x;
            this.y = cube.y;
            this.pos = new Vector(x, y);
            this.rad = Utils.Deg2Rad(Utils.Deg(cube.angle));
            this.spdL = spdL;
            this.spdR = spdR;
            this.spd = (spdL + spdR) / 2;
            this.w = (spdL - spdR) / CubeHandle.TireWidthDot;
            this.v = Vector.fromRadMag(rad, this.spd);
        }
        public void Update(Cube cube){
            if (cube == null) return;
            this.x = cube.x;
            this.y = cube.y;
            this.pos = new Vector(x, y);
            this.rad = Utils.Deg2Rad(Utils.Deg(cube.angle));
            this.v = Vector.fromRadMag(rad, this.spd);
        }
        public CEntity(double margin=15)
        {
            this.margin = margin;
        }
    }

    internal class CNavigator : Navigator
    {
        public CNavigator()
        {
            this.mode = Mode.AVOID;
            this.ego = new CEntity();
            this.avoid = new HLAvoid(this.ego);
        }

        public void Update(Cube cube, double spdL, double spdR)
        {
            (this.ego as CEntity).Update(cube, spdL, spdR);
        }
        public void Update(Cube cube)
        {
            (this.ego as CEntity).Update(cube);
        }
    }
}
