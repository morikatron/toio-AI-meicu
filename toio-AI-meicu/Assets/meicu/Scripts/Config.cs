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
                FailBehaviour failBehaviour
            ) {
                this.questSize = questSize;
                this.modelName = modelName;
                this.nStages = nStages;
                this.failBehaviour = failBehaviour;
                this.stageSettings = new List<StageSetting>();
            }

            public int questSize;
            public string modelName;
            public int nStages;
            public FailBehaviour failBehaviour;
            public List<StageSetting> stageSettings;
        }

        public struct StageSetting
        {
            public float[] thinkTimes;
            public byte[] speeds;
            public float[] confirmTimes;
            public static StageSetting CreateEmpty()
            {
                return new StageSetting();
            }
            public static StageSetting CreatePattern1(int length, byte motorBegin, byte motorLast, float wholeTimeBegin, int endIdx, float wholeTimeEnd, float factor=0.5f)
            {
                var s = new StageSetting();
                s.SetSpeeds_Linear(length, motorBegin, motorLast);     // motor speed of 25 takes about 1 sec.
                s.SetConfirmTimes_Seg2(length, 1, endIdx, 0.4f);
                s.SetThinkTimes_Power(length,
                    wholeTimeBegin - 25f/motorBegin - 1,
                    endIdx,
                    wholeTimeEnd - 25/((motorLast-motorBegin)*1f*endIdx/length + motorBegin) - 0.4f
                );
                return s;
            }
            public StageSetting SetThinkTimes_Power(int length, float begin, int endIdx, float end, float factor=0.5f)
            {
                thinkTimes = new float[length];
                // y = a f^n + b
                // a + b = begin
                // a f^endIdx + b = end
                float a = (begin - end) / (1 - Mathf.Pow(factor, endIdx));
                float b = begin - a;
                for (int i=0; i<length; i++)
                {
                    if (i < endIdx) thinkTimes[i] = a * Mathf.Pow(factor, i) + b;
                    else thinkTimes[i] = end;
                }
                return this;
            }
            public StageSetting SetSpeeds_Linear(int length, byte begin, byte end)
            {
                speeds = new byte[length];
                for (int i=0; i<length; i++)
                    speeds[i] = (byte)( (1f-1f*i/length) * begin + (1f*i/length) * end );
                return this;
            }
            public StageSetting SetConfirmTimes_Seg2(int length, float value1, int seg1, float value2)
            {
                confirmTimes = new float[length];
                for (int i=0; i<length; i++)
                {
                    if (i < seg1) confirmTimes[i] = value1;
                    else confirmTimes[i] = value2;
                }
                return this;
            }
        }


        public static readonly int nLevels = 11;
        public static readonly string bestModelName = "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-20000014";
        public static List<LevelSetting> levelSettings = new List<LevelSetting>();


        public enum FailBehaviour
        {
            KeepQuest, KeepStage, DropStage
        }

        public static void Init()
        {
            levelSettings.Clear();

            // Lv 0
            {
                var lv = new LevelSetting(
                    questSize: 3,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-399999",  // acc = 0.79
                    nStages: 5,
                    failBehaviour: FailBehaviour.KeepQuest
                );
                lv.stageSettings.Add(StageSetting.CreatePattern1(5, 30, 50, 5.0f, 5, 1.9f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(5, 40, 50, 4.5f, 5, 1.8f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(5, 40, 60, 4.0f, 5, 1.7f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(5, 50, 60, 3.5f, 5, 1.6f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(5, 60, 60, 3.0f, 5, 1.5f));
                levelSettings.Add(lv);
            }

            // Lv 1
            {
                var lv = new LevelSetting(
                    questSize: 3,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-20000014",  // acc = 0.988
                    nStages: 5,
                    failBehaviour: FailBehaviour.KeepQuest
                );
                lv.stageSettings.Add(StageSetting.CreatePattern1(5, 40, 50, 4.0f, 5, 1.9f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(5, 50, 60, 3.5f, 5, 1.6f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(5, 60, 70, 3.0f, 5, 1.3f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(5, 60, 70, 2.5f, 5, 1.2f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(5, 70, 70, 2.0f, 5, 1.0f));
                levelSettings.Add(lv);
            }

            // Lv 2
            {
                var lv = new LevelSetting(
                    questSize: 4,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-999999",  // acc = 0.621
                    nStages: 5,
                    failBehaviour: FailBehaviour.KeepQuest
                );
                lv.stageSettings.Add(StageSetting.CreatePattern1(7, 40, 50, 4.0f, 7, 1.9f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(7, 50, 60, 3.5f, 7, 1.6f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(7, 60, 70, 3.0f, 7, 1.4f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(7, 70, 80, 2.5f, 7, 1.2f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(7, 70, 80, 2.0f, 7, 1.0f));
                levelSettings.Add(lv);
            }

            // Lv 3
            {
                var lv = new LevelSetting(
                    questSize: 4,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-1999996",  // acc = 0.814
                    nStages: 5,
                    failBehaviour: FailBehaviour.KeepStage
                );
                lv.stageSettings.Add(StageSetting.CreatePattern1(7, 40, 50, 3.5f, 6, 1.9f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(7, 50, 60, 3.0f, 6, 1.6f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(7, 60, 70, 2.5f, 6, 1.4f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(7, 70, 80, 2.0f, 5, 1.2f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(7, 70, 80, 2.0f, 5, 1.0f));
                levelSettings.Add(lv);
            }

            // Lv 4
            {
                var lv = new LevelSetting(
                    questSize: 4,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-20000014",  // acc = 0.989
                    nStages: 5,
                    failBehaviour: FailBehaviour.KeepStage
                );
                lv.stageSettings.Add(StageSetting.CreatePattern1(7, 50, 50, 3.0f, 5, 1.9f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(7, 60, 60, 3.0f, 5, 1.6f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(7, 70, 70, 2.5f, 5, 1.4f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(7, 70, 80, 2.0f, 4, 1.2f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(7, 80, 80, 2.0f, 4, 1.0f));
                levelSettings.Add(lv);
            }

            // Lv 5
            {
                var lv = new LevelSetting(
                    questSize: 5,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-6999997",  // acc = 0.636
                    nStages: 5,
                    failBehaviour: FailBehaviour.KeepStage
                );
                lv.stageSettings.Add(StageSetting.CreatePattern1(9, 40, 50, 5.0f, 8, 1.9f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(9, 50, 60, 4.5f, 8, 1.6f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(9, 60, 70, 4.0f, 8, 1.4f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(9, 70, 80, 3.5f, 8, 1.2f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(9, 70, 80, 3.0f, 8, 1.0f));
                levelSettings.Add(lv);
            }

            // Lv 6
            {
                var lv = new LevelSetting(
                    questSize: 5,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-10999999",  // acc = 0.821
                    nStages: 5,
                    failBehaviour: FailBehaviour.KeepStage
                );
                lv.stageSettings.Add(StageSetting.CreatePattern1(9, 40, 50, 4.5f, 6, 1.6f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(9, 50, 60, 4.0f, 6, 1.4f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(9, 60, 70, 3.5f, 6, 1.3f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(9, 70, 80, 3.0f, 6, 1.2f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(9, 70, 80, 2.5f, 6, 1.0f));
                levelSettings.Add(lv);
            }

            // Lv 7
            {
                var lv = new LevelSetting(
                    questSize: 5,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-20000014",  // acc = 0.923
                    nStages: 5,
                    failBehaviour: FailBehaviour.KeepStage
                );
                lv.stageSettings.Add(StageSetting.CreatePattern1(9, 40, 50, 4.0f, 4, 1.6f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(9, 50, 60, 3.5f, 4, 1.4f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(9, 60, 70, 3.0f, 4, 1.3f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(9, 70, 80, 3.0f, 4, 1.2f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(9, 70, 80, 2.5f, 4, 1.0f));
                levelSettings.Add(lv);
            }

            // Lv 8
            {
                var lv = new LevelSetting(
                    questSize: 6,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-10999999",  // acc = 0.622
                    nStages: 5,
                    failBehaviour: FailBehaviour.KeepStage
                );
                lv.stageSettings.Add(StageSetting.CreatePattern1(11, 40, 50, 5.5f, 8, 1.6f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(11, 50, 60, 5.0f, 8, 1.4f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(11, 60, 70, 4.5f, 8, 1.3f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(11, 70, 80, 4.0f, 10, 1.0f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(11, 70, 80, 3.5f, 8, 1.0f));
                levelSettings.Add(lv);
            }

            // Lv 9
            {
                var lv = new LevelSetting(
                    questSize: 6,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-20000014",  // acc = 0.857
                    nStages: 5,
                    failBehaviour: FailBehaviour.KeepStage
                );
                lv.stageSettings.Add(StageSetting.CreatePattern1(11, 50, 60, 4.5f, 8, 1.6f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(11, 60, 70, 4.0f, 8, 1.4f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(11, 60, 70, 3.5f, 8, 1.0f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(11, 70, 80, 3.0f, 6, 1.0f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(11, 70, 80, 2.5f, 4, 1.0f));
                levelSettings.Add(lv);
            }

            // Lv 10
            {
                var lv = new LevelSetting(
                    questSize: 7,
                    modelName: "meicu-models\\M1024x2_L3to8_b1_2e7\\meicu-20000014",  // acc = 0.73
                    nStages: 5,
                    failBehaviour: FailBehaviour.DropStage
                );
                lv.stageSettings.Add(StageSetting.CreatePattern1(13, 60, 60, 6.0f, 11, 1.0f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(13, 60, 70, 4.5f, 9, 1.0f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(13, 70, 70, 3.0f, 7, 1.0f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(13, 70, 80, 3.0f, 5, 1.0f));
                lv.stageSettings.Add(StageSetting.CreatePattern1(13, 80, 80, 2.5f, 4, 1.0f));
                levelSettings.Add(lv);
            }
        }

    }

}
