using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;


namespace toio.AI.meicu
{
    public class PageManager : MonoBehaviour
    {
        public Canvas canvas;
        public PageTitle pageTitle;
        public PageTutorial pageTutorial;
        public PageBattle pageBattle;
        public PageLearn pageLearn;
        public PageTrainer pageTrainer;
        public DialogConnect dialogConnect;
        public VideoPlayer videoPlayer;
        public Transform uiDebug;


        private bool isDebug = true;
        static PageManager ins;
        static EPage page;


        void Start()
        {
            ins = this;

            Config.Init();

            Device.connectionCallback += OnDeviceConnection;

            // Load video
            videoPlayer.url = Application.streamingAssetsPath + "/training.mp4";
            videoPlayer.Prepare();
            // videoPlayer.prepareCompleted += (vp) => { vp.Pause(); };

            SetPage(EPage.Title);
        }

        void Update()
        {
            if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyDown(KeyCode.Q))
            {
                isDebug = !isDebug;
                uiDebug.gameObject.SetActive(isDebug);
                UpdateDebugSliderLv();
            }
        }

        internal enum EPage
        {
            Title, Tutorial, Battle, Learn, Trainer
        }
        internal static void SetPage(EPage ePage)
        {
            UIFinger.Hide();

            page = ePage;
            ins.pageTitle?.SetActive(ePage == EPage.Title);
            ins.pageTutorial?.SetActive(ePage == EPage.Tutorial);
            ins.pageBattle?.SetActive(ePage == EPage.Battle);
            ins.pageLearn?.SetActive(ePage == EPage.Learn);
            ins.pageTrainer?.SetActive(ePage == EPage.Trainer);

            if (ins.isDebug && ePage == EPage.Title)
                ins.UpdateDebugSliderLv();
            ins.uiDebug?.gameObject.SetActive(ins.isDebug && ePage == EPage.Title);

            AudioPlayer.ins.PlayBGM(ePage);

            if (page == EPage.Title)
                ins.dialogConnect.SetActive(false);
        }

        public static void OnBtnHome()
        {
            AudioPlayer.ins.PlaySE(AudioPlayer.ESE.Cancel);
            SetPage(PageManager.EPage.Title);
        }

        void OnDeviceConnection(int cubeIdx, bool connected)
        {
            if (!connected)
            {
                Debug.Log($"Cube {cubeIdx} Disconnected! ");
                if (page == EPage.Title)
                {
                    pageTitle.Refresh();
                }
                else
                {
                    dialogConnect.SetActive(true);

                    if (page == EPage.Tutorial) pageTutorial?.Pause();
                    if (page == EPage.Battle) pageBattle?.Pause();
                    if (page == EPage.Learn) pageLearn?.Pause();
                    if (page == EPage.Trainer) pageTrainer?.Pause();
                }
            }
        }
        internal static void OnReconnected()
        {
            if (page == EPage.Title) return;

            ins.dialogConnect.SetActive(false);

            if (page == EPage.Tutorial) ins.pageTutorial?.Pause();
            if (page == EPage.Battle) ins.pageBattle?.Pause();
            if (page == EPage.Learn) ins.pageLearn?.Pause();
            if (page == EPage.Trainer) ins.pageTrainer?.Pause();
        }


        #region ========= Debug ========

        void UpdateDebugSliderLv()
        {
            uiDebug.Find("TextLv").GetComponent<Text>().text = $"レベル {Prefs.level}";
            uiDebug.Find("SliderLv").GetComponent<Slider>().value = Prefs.level;
        }

        public void OnDebugBtnResetFlags()
        {
            PlayerPrefs.DeleteAll();

            UpdateDebugSliderLv();
            if (page == EPage.Title) pageTitle.Refresh();
        }
        public void OnDebugBtnSetAllFlags()
        {
            Prefs.SetTutorialAccessed();
            Prefs.SetTutorialCleared();
            Prefs.SetBattleAccessed();
            Prefs.SetLearnAccessed();
            Prefs.SetLearnCleared();
            Prefs.SetBattleEnteredAfterLearn();
            Prefs.SetTrainerAccessed();
            if (Prefs.level == 1) Prefs.level = 3;
            Prefs.trainerStage = 2;

            UpdateDebugSliderLv();
            if (page == EPage.Title) pageTitle.Refresh();
        }
        public void OnDebugSliderLv()
        {
            Prefs.level = (int)uiDebug.Find("SliderLv").GetComponent<Slider>().value;

            UpdateDebugSliderLv();
            if (page == EPage.Title) pageTitle.Refresh();
        }

        #endregion
    }



}
