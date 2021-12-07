using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using toio;


namespace toio.AI.meicu
{

    public class Device
    {
        static internal Action<int, bool> connectionCallback;

        static CubeManager _cubeManager;
        internal static CubeManager cubeManager
        {
            get {
                if (_cubeManager == null)
                    _cubeManager = new CubeManager(ConnectType.Auto);
                return _cubeManager;
            }
        }

        internal static bool isCube0Connected { get {
            if (cubeManager.cubes.Count == 0) return false;
            return cubeManager.cubes[0].isConnected;
        }}
        internal static bool isCube1Connected { get {
            if (cubeManager.cubes.Count < 2) return false;
            return cubeManager.cubes[1].isConnected;
        }}
        internal static bool isBothConnected => isCube0Connected && isCube1Connected;

        internal static int nConnected { get {
            int n = 0;
            if (isCube0Connected) n++;
            if (isCube1Connected) n++;
            return n;
        }}

        internal static async UniTask Connect()
        {
            var cube = await cubeManager.SingleConnect();
            var cubeIdx = cubeManager.cubes.FindIndex(c => c == cube);

            if (cubeIdx == 0)
            {
                var color = Config.LEDBlue;
                cube.TurnLedOn(color.r, color.g, color.b, 0);
            }
            else
            {
                var color = Config.LEDOrange;
                cube.TurnLedOn(color.r, color.g, color.b, 0);
            }

            // Set Disconnection Callback
            if (cube is CubeReal)
            {
                (cube as CubeReal).peripheral.AddConnectionListener(
                    "meicu.Device",
                    peri => connectionCallback?.Invoke(cubeIdx, peri.isConnected));
            }
        }

        internal static bool TargetMove(int idx, int row, int col, int biasX=0, int biasY=0)
        {
            if (cubeManager.cubes.Count < idx+1) return false;
            var pos = SpaceCoords2ID(row, col);
            cubeManager.cubes[idx].TargetMove(
                pos.x, pos.y, 0,
                timeOut: 2,
                targetMoveType:Cube.TargetMoveType.RoundBeforeMove,
                targetRotationType:Cube.TargetRotationType.NotRotate);
            return true;
        }

        internal static List<Cube> cubes => cubeManager.cubes;

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
