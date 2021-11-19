using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace toio.AI.meicu
{

    internal static class MeiPrefs
    {
        internal static readonly string KeyTutorialCleared = "MeiTC";
        internal static readonly string KeyLearnCleared = "MeiLC";
        internal static readonly string KeyLevel = "MeiLv";
        // internal static readonly string KeyStage = "MeiStage";

        internal static bool isTutorialCleared
        {
            get {
                return PlayerPrefs.HasKey(KeyTutorialCleared);
            }
        }

        internal static bool isLearnCleared
        {
            get {
                return PlayerPrefs.HasKey(KeyLearnCleared);
            }
        }


        internal static int level
        {
            get {
                if (!PlayerPrefs.HasKey(KeyLevel))
                    PlayerPrefs.SetInt(KeyLevel, 1);
                return PlayerPrefs.GetInt(KeyLevel, 1);
            }
            set {
                value = Mathf.Clamp(value, 1, 5);
                PlayerPrefs.SetInt(KeyLevel, value);
            }
        }

        // internal static int stage
        // {
        //     get {
        //         if (!PlayerPrefs.HasKey(KeyStage))
        //             PlayerPrefs.SetInt(KeyStage, 1);
        //         return PlayerPrefs.GetInt(KeyStage, 1);
        //     }
        //     set {
        //         value = Mathf.Clamp(value, 1, 10);
        //         PlayerPrefs.SetInt(KeyStage, value);
        //     }
        // }

        internal static void SetTutorialCleared()
        {
            PlayerPrefs.SetInt(KeyTutorialCleared, 1);
        }

        internal static void SetLearnCleared()
        {
            PlayerPrefs.SetInt(KeyLearnCleared, 1);
        }

        // internal static void StageUp()
        // {
        //     if (stage == 10 && level == 4) return;

        //     level += stage / 10;
        //     stage = stage % 10 + 1;
        // }

        internal static void LevelUp()
        {
            if (level == 5) return;

            level += 1;
        }
    }

}
