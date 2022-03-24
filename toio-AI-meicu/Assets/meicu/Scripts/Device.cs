using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using toio;
using toio.Navigation;


namespace toio.AI.meicu
{

    public class Device
    {
        static internal event Action<int, bool> connectionCallback;

        static CubeManager _cubeManager;
        internal static CubeManager cubeManager
        {
            get {
                if (_cubeManager == null)
                    _cubeManager = new CubeManager(ConnectType.Auto);
                return _cubeManager;
            }
        }
        private static List<Cube> cubes => cubeManager.cubes;
        private static List<CubeNavigator> navigators => cubeManager.navigators;
        private static Dictionary<int, int> IdxTable = new Dictionary<int, int>();


        internal static void Assign(int id)
        {
            if (!IdxTable.ContainsKey(id))
            {
                IdxTable.Add(id, -1);
            }
            UpdateAssign();
        }
        internal static bool IsConnected(int id)
        {
            if (!IdxTable.ContainsKey(id))
                Assign(id);

            int idx = IdxTable[id];
            if (cubes.Count <= idx || idx < 0) return false;
            return cubes[idx].isConnected;
        }

        internal static Cube GetCube(int id)
        {
            if (!IdxTable.ContainsKey(id)) return null;

            int idx = IdxTable[id];
            if (cubes.Count <= idx || idx < 0) return null;
            return cubes[idx];
        }
        internal static CubeNavigator GetNavi(int id)
        {
            if (!IdxTable.ContainsKey(id)) return null;

            int idx = IdxTable[id];
            if (navigators.Count <= idx || idx < 0) return null;
            return navigators[idx];
        }

        internal static bool isTwoConnected => nConnected >= 2;
        internal static int nConnected { get {
            int n = 0;
            foreach (var c in cubes)
            {
                if (c.isConnected) n++;
            }
            return n;
        }}


        static void UpdateAssign()
        {
            for (int idx = 0; idx < cubes.Count; idx ++)
            {
                if (!cubes[idx].isConnected) continue;
                if (IdxTable.ContainsValue(idx)) continue;
                // Find a not connected id, assign idx to it
                foreach (var id in IdxTable.Keys)
                {
                    var _idx = IdxTable[id];
                    if (_idx < 0 || _idx >= cubes.Count || !cubes[_idx].isConnected)
                    {
                        IdxTable[id] = idx;

                        if (cubes[idx] is CubeReal)
                            (cubes[idx] as CubeReal).peripheral.AddConnectionListener(
                                "meicu.Device", peri => connectionCallback.Invoke(id, peri.isConnected));

                        // Connected event
                        connectionCallback.Invoke(id, true);

                        if (0 <= _idx && _idx < cubes.Count && cubes[_idx] is CubeReal)
                        {
                            (cubes[_idx] as CubeReal).peripheral.RemoveConnectionListener("meicu.Device");
                        }
                        break;
                    }
                }
            }
        }

        internal static async UniTask<int> Connect()
        {
            var cube = await cubeManager.SingleConnect();
            if (cube == null) return 1;
            var cubeIdx = cubes.FindIndex(c => c == cube);
            cubeManager.handles[cubeIdx].SetBorderRect(new RectInt(545, 45, 410, 410));

            UpdateAssign();

            int id = -1;
            foreach (var _id in IdxTable.Keys)
            {
                if (IdxTable[_id] == cubeIdx)
                {
                    id = _id; break;
                }
            }

            if (id == -1)
            {
                Debug.LogWarning("Device.Connect: 3rd cube connected.");
                return 2;
            }

            // Turn on LED
            if (id == 0)
            {
                var color = Config.LEDBlue;
                cube.TurnLedOn(color.r, color.g, color.b, 0);
            }
            else if (id == 1)
            {
                var color = Config.LEDOrange;
                cube.TurnLedOn(color.r, color.g, color.b, 0);
            }
            return 0;
        }

        internal static bool TargetMoveByID(int id, int x, int y, byte maxSpd=80)
        {
            if (!IdxTable.ContainsKey(id)) return false;
            int idx = IdxTable[id];
            if (cubeManager.cubes.Count < idx+1) return false;

            cubeManager.cubes[idx].TargetMove(
                x, y, 0,
                timeOut: 2,
                maxSpd: maxSpd,
                targetMoveType:Cube.TargetMoveType.RoundBeforeMove,
                targetRotationType:Cube.TargetRotationType.NotRotate);
            return true;
        }

        internal static bool TargetMove(int id, int row, int col, int biasX=0, int biasY=0, byte maxSpd=80)
        {
            if (!IdxTable.ContainsKey(id)) return false;
            int idx = IdxTable[id];

            row = Mathf.Clamp(row, 0, 8);
            col = Mathf.Clamp(col, 0, 8);
            biasX = Mathf.Clamp(biasX, -20, 20);
            biasY = Mathf.Clamp(biasY, -20, 20);

            if (cubeManager.cubes.Count < idx+1) return false;
            var pos = SpaceCoords2ID(row, col);
            cubeManager.cubes[idx].TargetMove(
                pos.x + biasX, pos.y + biasY, 0,
                timeOut: 2,
                maxSpd: maxSpd,
                targetMoveType:Cube.TargetMoveType.RoundBeforeMove,
                targetRotationType:Cube.TargetRotationType.NotRotate);
            return true;
        }

        public static bool IsAtSpace(int id, int row, int col)
        {
            if (!IdxTable.ContainsKey(id)) return false;
            int idx = IdxTable[id];

            if (!IsConnected(idx)) return false;
            if (!cubes[idx].isGrounded) return false;
            var rowCol = ID2SpaceCoord(cubes[idx].x, cubes[idx].y);
            return rowCol.x == row && rowCol.y == col;
        }

        public static Vector2Int SpaceCoords2ID(int row, int col)
        {
            return new Vector2Int(750 + (col-4)*44, 250 + (row-4)*44);
        }
        public static Vector2Int ID2SpaceCoord(int x, int y)
        {
            return new Vector2Int((y-250+176+22)/44, (x-750+176+22)/44);
        }
    }

}
