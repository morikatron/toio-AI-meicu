using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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


        static PageManager ins;
        static EPage page;


        void Start()
        {
            ins = this;

            Config.Init();

            Device.connectionCallback = OnDeviceConnection;

            // PlayerPrefs.DeleteAll();
            MeiPrefs.SetTutorialCleared();
            MeiPrefs.SetLearnCleared();
            if (MeiPrefs.level <= 2)
                MeiPrefs.level = 3;
            // Load video
            videoPlayer.url = Application.streamingAssetsPath + "/training.mp4";
            videoPlayer.Prepare();

            SetPage(EPage.Title);
            dialogConnect.gameObject.SetActive(false);
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

    }

}
