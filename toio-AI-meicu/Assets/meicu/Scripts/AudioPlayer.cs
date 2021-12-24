using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace toio.AI.meicu
{

    public class AudioPlayer : MonoBehaviour
    {
        public static AudioPlayer ins {get; private set;}

        [Header("Audio Sources")]
        public AudioSource bgmTitle;
        public AudioSource bgmTutorial;
        public AudioSource bgmBattle;
        public AudioSource srcPerform;
        public AudioSource srcSE;

        public AudioClip confirmed;
        public AudioClip confirming;
        public AudioClip countdown;
        public AudioClip start;

        public AudioClip win;
        public AudioClip lose;
        public AudioClip levelup;

        private bool _isBGMOn = false;
        internal bool isBGMOn { get { return _isBGMOn; }
            set {
                _isBGMOn = value;
                Prefs.isMute = !value;
                if (value)
                {
                    bgmTitle.volume = 0.3f;
                    bgmTutorial.volume = 0.3f;
                    bgmBattle.volume = 0.3f;
                }
                else
                {
                    bgmTitle.volume = 0;
                    bgmTutorial.volume = 0;
                    bgmBattle.volume = 0;
                }
            }
        }


        void Awake()
        {
            ins = this;

            // Init isBGMOn
            isBGMOn = !Prefs.isMute;
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


        internal enum ESE : byte
        {
            StartConfirmed, StartCount, Start,
            StepConfirmed, StepConfirming,
            Win, Lose, Draw, LevelUp
        }
        internal void PlaySE(ESE se, float volume=0.5f)
        {
            if (se == ESE.StartConfirmed)
            {
                srcSE.PlayOneShot(confirmed, volume);
            }
            else if (se == ESE.StartCount)
            {
                srcSE.PlayOneShot(countdown, volume);
            }
            else if (se == ESE.Start)
            {
                srcSE.PlayOneShot(start, volume);
            }
            else if (se == ESE.StepConfirming)
            {
                srcSE.PlayOneShot(confirming, volume);
            }
            else if (se == ESE.StepConfirmed)
            {
                srcSE.PlayOneShot(confirmed, volume);
            }
            else if (se == ESE.Win)
            {
                srcPerform.PlayOneShot(win, volume);
            }
            else if (se == ESE.Lose)
            {
                srcPerform.PlayOneShot(lose, volume);
            }
            else if (se == ESE.Draw)
            {
                srcPerform.PlayOneShot(lose, volume);
            }
            else if (se == ESE.LevelUp)
            {
                srcPerform.PlayOneShot(levelup, volume);
            }
        }

        internal void StopSE(){
            srcSE.Stop();
            srcSE.clip = null;
        }
        internal void StopPerform(){
            srcPerform.Stop();
            srcPerform.clip = null;
        }
    }

}
