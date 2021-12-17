using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace toio.AI.meicu
{

    public static class Config
    {
        public static readonly Color32 UIOrange = new Color32(255, 74, 11, 255);
        public static readonly Color32 UIBlue = new Color32(102, 107, 255, 255);
        public static readonly Color32 LEDOrange = new Color32(222, 100, 0, 255);
        public static readonly Color32 LEDBlue = new Color32(22, 11, 255, 255);


        public struct LevelSetting
        {
            public LevelSetting(
                int questSize,
                string modelName,
                int nStages,
                bool retryOnFail
            ) {
                this.questSize = questSize;
                this.modelName = modelName;
                this.nStages = nStages;
                this.retryOnFail = retryOnFail;
                this.stageSettings = new List<StageSetting>();
            }

            public int questSize;
            public string modelName;
            public int nStages;
            public bool retryOnFail;
            public List<StageSetting> stageSettings;
        }

        public struct StageSetting
        {
            public StageSetting(float intervalBegin, float intervalEnd)
            {
                this.intervelBegin = intervalBegin;
                this.intervalEnd = intervalEnd;
            }
            public float intervelBegin;
            public float intervalEnd;
        }


        public static readonly int nLevels = 11;
        public static readonly string bestModelName = "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-20000014";
        public static List<LevelSetting> levelSettings = new List<LevelSetting>();

        public static void Init()
        {
            levelSettings.Clear();

            // Lv 0
            {
                var lv = new LevelSetting(
                    questSize: 3,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-399999",  // acc = 0.79
                    nStages: 5,
                    retryOnFail: true
                );
                lv.stageSettings.Add(new StageSetting(3f  , 2f  ));
                lv.stageSettings.Add(new StageSetting(2.5f, 1.5f));
                lv.stageSettings.Add(new StageSetting(2f  , 1.5f));
                lv.stageSettings.Add(new StageSetting(1.5f, 1f  ));
                lv.stageSettings.Add(new StageSetting(1f  , 0.5f));
                levelSettings.Add(lv);
            }

            // Lv 1
            {
                var lv = new LevelSetting(
                    questSize: 3,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-20000014",  // acc = 0.988
                    nStages: 5,
                    retryOnFail: true
                );
                lv.stageSettings.Add(new StageSetting(3f  , 2f  ));
                lv.stageSettings.Add(new StageSetting(2.5f, 1.5f));
                lv.stageSettings.Add(new StageSetting(2f  , 1.5f));
                lv.stageSettings.Add(new StageSetting(1.5f, 1f  ));
                lv.stageSettings.Add(new StageSetting(1f  , 0.5f));
                levelSettings.Add(lv);
            }

            // Lv 2
            {
                var lv = new LevelSetting(
                    questSize: 4,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-999999",  // acc = 0.621
                    nStages: 5,
                    retryOnFail: true
                );
                lv.stageSettings.Add(new StageSetting(4f  , 2f  ));
                lv.stageSettings.Add(new StageSetting(3.5f, 1.5f));
                lv.stageSettings.Add(new StageSetting(3f  , 1.5f));
                lv.stageSettings.Add(new StageSetting(2.5f, 1f  ));
                lv.stageSettings.Add(new StageSetting(2f  , 0.5f));
                levelSettings.Add(lv);
            }

            // Lv 3
            {
                var lv = new LevelSetting(
                    questSize: 4,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-1999996",  // acc = 0.814
                    nStages: 5,
                    retryOnFail: false
                );
                lv.stageSettings.Add(new StageSetting(3.5f  , 2f  ));
                lv.stageSettings.Add(new StageSetting(3f, 1.5f));
                lv.stageSettings.Add(new StageSetting(2.5f  , 1.5f));
                lv.stageSettings.Add(new StageSetting(2f, 1f  ));
                lv.stageSettings.Add(new StageSetting(1.5f  , 0.5f));
                levelSettings.Add(lv);
            }

            // Lv 4
            {
                var lv = new LevelSetting(
                    questSize: 4,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-20000014",  // acc = 0.989
                    nStages: 5,
                    retryOnFail: false
                );
                lv.stageSettings.Add(new StageSetting(3f  , 2f  ));
                lv.stageSettings.Add(new StageSetting(2.5f, 1.5f));
                lv.stageSettings.Add(new StageSetting(2f  , 1.5f));
                lv.stageSettings.Add(new StageSetting(1.5f, 1f  ));
                lv.stageSettings.Add(new StageSetting(1f  , 0.5f));
                levelSettings.Add(lv);
            }

            // Lv 5
            {
                var lv = new LevelSetting(
                    questSize: 5,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-6999997",  // acc = 0.636
                    nStages: 5,
                    retryOnFail: false
                );
                lv.stageSettings.Add(new StageSetting(5f  , 2f  ));
                lv.stageSettings.Add(new StageSetting(4.5f, 1.5f));
                lv.stageSettings.Add(new StageSetting(4f  , 1.5f));
                lv.stageSettings.Add(new StageSetting(3.5f, 1f  ));
                lv.stageSettings.Add(new StageSetting(3f  , 0.5f));
                levelSettings.Add(lv);
            }

            // Lv 6
            {
                var lv = new LevelSetting(
                    questSize: 5,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-10999999",  // acc = 0.821
                    nStages: 5,
                    retryOnFail: false
                );
                lv.stageSettings.Add(new StageSetting(5f  , 2f  ));
                lv.stageSettings.Add(new StageSetting(4.5f, 1.5f));
                lv.stageSettings.Add(new StageSetting(4f  , 1.5f));
                lv.stageSettings.Add(new StageSetting(3.5f, 1f  ));
                lv.stageSettings.Add(new StageSetting(3f  , 0.5f));
                levelSettings.Add(lv);
            }

            // Lv 7
            {
                var lv = new LevelSetting(
                    questSize: 5,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-20000014",  // acc = 0.923
                    nStages: 5,
                    retryOnFail: false
                );
                lv.stageSettings.Add(new StageSetting(5f  , 2f  ));
                lv.stageSettings.Add(new StageSetting(4.5f, 1.5f));
                lv.stageSettings.Add(new StageSetting(4f  , 1.5f));
                lv.stageSettings.Add(new StageSetting(3.5f, 1f  ));
                lv.stageSettings.Add(new StageSetting(3f  , 0.5f));
                levelSettings.Add(lv);
            }

            // Lv 8
            {
                var lv = new LevelSetting(
                    questSize: 6,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-10999999",  // acc = 0.622
                    nStages: 5,
                    retryOnFail: false
                );
                lv.stageSettings.Add(new StageSetting(6f  , 2f  ));
                lv.stageSettings.Add(new StageSetting(5f  , 1.5f));
                lv.stageSettings.Add(new StageSetting(4.5f  , 1.5f));
                lv.stageSettings.Add(new StageSetting(4f  , 1f  ));
                lv.stageSettings.Add(new StageSetting(3.5f  , 0.5f));
                levelSettings.Add(lv);
            }

            // Lv 9
            {
                var lv = new LevelSetting(
                    questSize: 6,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-20000014",  // acc = 0.857
                    nStages: 5,
                    retryOnFail: false
                );
                lv.stageSettings.Add(new StageSetting(6f  , 2f  ));
                lv.stageSettings.Add(new StageSetting(5f  , 1.5f));
                lv.stageSettings.Add(new StageSetting(4.5f  , 1.5f));
                lv.stageSettings.Add(new StageSetting(4f  , 1f  ));
                lv.stageSettings.Add(new StageSetting(3.5f  , 0.5f));
                levelSettings.Add(lv);
            }

            // Lv 10
            {
                var lv = new LevelSetting(
                    questSize: 7,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-20000014",  // acc = 0.73
                    nStages: 5,
                    retryOnFail: false
                );
                lv.stageSettings.Add(new StageSetting(7f  , 2f  ));
                lv.stageSettings.Add(new StageSetting(6f  , 1.5f));
                lv.stageSettings.Add(new StageSetting(5f  , 1.5f));
                lv.stageSettings.Add(new StageSetting(4f  , 1f  ));
                lv.stageSettings.Add(new StageSetting(3f  , 0.5f));
                levelSettings.Add(lv);
            }
        }

    }

}
