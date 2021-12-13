using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace toio.AI.meicu
{

    public class AudioPlayer : MonoBehaviour
    {
        public static AudioPlayer ins {get; private set;}

        public AudioSource bgmTitle;
        public AudioSource bgmTutorial;
        public AudioSource bgmBattle;


        void OnEnable()
        {
            ins = this;
        }

        internal void PlayBGM(PageManager.EPage page)
        {
            if (page == PageManager.EPage.Title)
            {
                bgmTutorial.Stop();
                bgmBattle.Stop();
                if (!bgmTitle.isPlaying)
                    bgmTitle.Play();
            }
            else if (page == PageManager.EPage.Battle)
            {
                bgmTutorial.Stop();
                bgmTitle.Stop();
                if (!bgmBattle.isPlaying)
                    bgmBattle.Play();
            }
            else if (page == PageManager.EPage.Tutorial || page == PageManager.EPage.Learn)
            {
                bgmTitle.Stop();
                bgmBattle.Stop();
                if (!bgmTutorial.isPlaying)
                    bgmTutorial.Play();
            }
        }
    }

}
