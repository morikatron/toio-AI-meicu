using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace toio.AI.meicu
{

    internal static class Prefs
    {
        internal static readonly string KeyMute = "MeicuMute";
        internal static readonly string KeyEverConnected = "MeicuEC";
        internal static readonly string KeyTutorialAccessed = "MeicuTA";
        internal static readonly string KeyTutorialCleared = "MeicuTC";
        internal static readonly string KeyLearnAccessed = "MeicuLA";
        internal static readonly string KeyLearnCleared = "MeicuLC";
        internal static readonly string KeyBattleAccessed = "MeicuBA";
        internal static readonly string KeyBattleEnteredAfterLearn = "MeicuBEAL";
        internal static readonly string KeyTrainerAccessed = "MeicuTrA";
        internal static readonly string KeyLevel = "MeicuLv";
        internal static readonly string KeyTrainerStage = "MeicuTrSt";
        // internal static readonly string KeyStage = "MeicuStage";

        internal static bool isMute
        {
            get {
                return PlayerPrefs.HasKey(KeyMute);
            }
            set {
                if (value) PlayerPrefs.SetInt(KeyMute, 1);
                else PlayerPrefs.DeleteKey(KeyMute);
            }
        }

        internal static bool isEverConnected
        {
            get {
                return PlayerPrefs.HasKey(KeyEverConnected);
            }
        }

        internal static bool isTutorialAccessed
        {
            get {
                return PlayerPrefs.HasKey(KeyTutorialAccessed);
            }
        }
        internal static bool isTutorialCleared
        {
            get {
                return PlayerPrefs.HasKey(KeyTutorialCleared);
            }
        }

        internal static bool isLearnAccessed
        {
            get {
                return PlayerPrefs.HasKey(KeyLearnAccessed);
            }
        }
        internal static bool isLearnCleared
        {
            get {
                return PlayerPrefs.HasKey(KeyLearnCleared);
            }
        }

        internal static bool isBattleAccessed
        {
            get {
                return PlayerPrefs.HasKey(KeyBattleAccessed);
            }
        }
        internal static bool isBattleEnteredAfterLearn
        {
            get {
                return PlayerPrefs.HasKey(KeyBattleEnteredAfterLearn);
            }
        }

        internal static bool isTrainerAccessed
        {
            get {
                return PlayerPrefs.HasKey(KeyTrainerAccessed);
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
                value = Mathf.Clamp(value, 1, Config.nLevels);
                PlayerPrefs.SetInt(KeyLevel, value);
            }
        }

        internal static int trainerStage
        {
            get {
                if (!PlayerPrefs.HasKey(KeyTrainerStage))
                    PlayerPrefs.SetInt(KeyTrainerStage, 0);
                return PlayerPrefs.GetInt(KeyTrainerStage, 0);
            }
            set {
                value = Mathf.Clamp(value, 0, 2);
                PlayerPrefs.SetInt(KeyTrainerStage, value);
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

        internal static void SetEverConnected()
        {
            PlayerPrefs.SetInt(KeyEverConnected, 1);
        }

        internal static void SetTutorialAccessed()
        {
            PlayerPrefs.SetInt(KeyTutorialAccessed, 1);
        }
        internal static void SetTutorialCleared()
        {
            PlayerPrefs.SetInt(KeyTutorialCleared, 1);
        }

        internal static void SetLearnAccessed()
        {
            PlayerPrefs.SetInt(KeyLearnAccessed, 1);
        }
        internal static void SetLearnCleared()
        {
            PlayerPrefs.SetInt(KeyLearnCleared, 1);
        }

        internal static void SetBattleAccessed()
        {
            PlayerPrefs.SetInt(KeyBattleAccessed, 1);
        }
        internal static void SetBattleEnteredAfterLearn()
        {
            PlayerPrefs.SetInt(KeyBattleEnteredAfterLearn, 1);
        }

        internal static void SetTrainerAccessed()
        {
            PlayerPrefs.SetInt(KeyTrainerAccessed, 1);
        }

        // internal static void StageUp()
        // {
        //     if (stage == 10 && level == 4) return;

        //     level += stage / 10;
        //     stage = stage % 10 + 1;
        // }

        internal static void LevelUp()
        {
            if (level == Config.nLevels) return;

            level += 1;
        }
    }

}
