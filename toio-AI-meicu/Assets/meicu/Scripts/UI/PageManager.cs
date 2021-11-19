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


        static PageManager ins;


        void Start()
        {
            ins = this;

            // PlayerPrefs.DeleteAll();
            MeiPrefs.SetTutorialCleared();
            MeiPrefs.SetLearnCleared();
            if (MeiPrefs.level <= 2)
                MeiPrefs.level = 3;

            SetPage(EPage.Title);
        }


        internal enum EPage
        {
            Title, Tutorial, Battle, Learn
        }
        internal static void SetPage(EPage ePage)
        {
            ins.pageTitle?.SetActive(ePage == EPage.Title);
            ins.pageTutorial?.SetActive(ePage == EPage.Tutorial);
            ins.pageBattle?.SetActive(ePage == EPage.Battle);
            ins.pageLearn?.SetActive(ePage == EPage.Learn);
        }
    }

}
