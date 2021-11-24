using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;


namespace toio.AI.meicu
{

    public class Device
    {
        static CubeManager _cubeManager;
        internal static CubeManager cubeManager
        {
            get {
                if (_cubeManager == null)
                    _cubeManager = new CubeManager();
                return _cubeManager;
            }
        }

        internal static bool isTwoConnected => (cubeManager.cubes.Count >= 2);
        internal static int nConnected => cubeManager.cubes.Count;

        internal static bool TargetMove(int idx, int row, int col, int biasX=0, int biasY=0)
        {
            if (cubeManager.cubes.Count < idx+1) return false;
            var pos = SpaceCoords2ID(row, col);
            cubeManager.cubes[idx].TargetMove(
                pos.x, pos.y, 0,
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
