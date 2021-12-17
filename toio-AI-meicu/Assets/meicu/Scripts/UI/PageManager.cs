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
        public DialogConnect dialogConnect;
        public VideoPlayer videoPlayer;

        [Header("Debug")]
        public bool isDebug;
        public Transform uiDebug;


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
            videoPlayer.prepareCompleted += (vp) => { vp.StepForward(); vp.Pause(); };

            SetPage(EPage.Title);
        }


        internal enum EPage
        {
            Title, Tutorial, Battle, Learn
        }
        internal static void SetPage(EPage ePage)
        {
            UIFinger.Hide();

            page = ePage;
            ins.pageTitle?.SetActive(ePage == EPage.Title);
            ins.pageTutorial?.SetActive(ePage == EPage.Tutorial);
            ins.pageBattle?.SetActive(ePage == EPage.Battle);
            ins.pageLearn?.SetActive(ePage == EPage.Learn);

            if (ins.isDebug && ePage == EPage.Title)
                ins.UpdateDebugSliderLv();
            ins.uiDebug?.gameObject.SetActive(ins.isDebug && ePage == EPage.Title);

            AudioPlayer.ins.PlayBGM(ePage);

            if (page == EPage.Title)
                ins.dialogConnect.gameObject.SetActive(false);
        }

        public static void OnBtnHome()
        {
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
                    dialogConnect.gameObject.SetActive(true);

                    if (page == EPage.Tutorial) pageTutorial?.Pause();
                    if (page == EPage.Battle) pageBattle?.Pause();
                    if (page == EPage.Learn) pageLearn?.Pause();
                }
            }
        }
        internal static void OnReconnected()
        {
            if (page == EPage.Title) return;

            ins.dialogConnect.gameObject.SetActive(false);

            if (page == EPage.Tutorial) ins.pageTutorial?.Pause();
            if (page == EPage.Battle) ins.pageBattle?.Pause();
            if (page == EPage.Learn) ins.pageLearn?.Pause();
        }


        #region ========= Debug ========

        void UpdateDebugSliderLv()
        {
            uiDebug.Find("TextLv").GetComponent<Text>().text = $"Level {MeiPrefs.level}";
            uiDebug.Find("SliderLv").GetComponent<Slider>().value = MeiPrefs.level;
        }

        public void OnDebugBtnResetFlags()
        {
            PlayerPrefs.DeleteAll();

            UpdateDebugSliderLv();
            if (page == EPage.Title) pageTitle.Refresh();
        }
        public void OnDebugBtnClearFlags()
        {
            MeiPrefs.SetTutorialCleared();
            MeiPrefs.SetLearnCleared();
            MeiPrefs.SetBattleEnteredAfterLearn();
            if (MeiPrefs.level == 1) MeiPrefs.level = 2;

            UpdateDebugSliderLv();
            if (page == EPage.Title) pageTitle.Refresh();
        }
        public void OnDebugSliderLv()
        {
            MeiPrefs.level = (int)uiDebug.Find("SliderLv").GetComponent<Slider>().value;

            UpdateDebugSliderLv();
            if (page == EPage.Title) pageTitle.Refresh();
        }

        #endregion
    }



}
