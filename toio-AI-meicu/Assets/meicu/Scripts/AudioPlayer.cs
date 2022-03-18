using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace toio.AI.meicu
{

    public class AudioPlayer : MonoBehaviour
    {
        public static AudioPlayer ins {get; private set;}

        [Header("Audio Sources")]
        public AudioSource srcBGM;
        public AudioSource srcPerform;
        public AudioSource srcSE;

        [Header("Audio Clips - BGM")]
        public AudioClip bgmTitle;
        public AudioClip bgmTutorial;
        public AudioClip bgmBattle;

        [Header("Audio Clips - Perform")]
        public AudioClip win;
        public AudioClip lose;
        public AudioClip levelup;

        [Header("Audio Clips - SE")]
        public AudioClip confirmed;
        public AudioClip confirming;
        public AudioClip countdown;
        public AudioClip start;

        public AudioClip wrong;
        public AudioClip correct;
        public AudioClip on;
        public AudioClip off;
        public AudioClip decide;
        public AudioClip cancel;
        public AudioClip transit;


        private bool _isBGMOn = false;
        internal bool isBGMOn { get { return _isBGMOn; }
            set {
                _isBGMOn = value;
                Prefs.isMute = !value;
                if (value)
                {
                    srcBGM.volume = 0.25f;
                }
                else
                {
                    srcBGM.volume = 0;
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
                srcBGM.clip = bgmTitle;
                srcBGM.Play();
            }
            else if (page == PageManager.EPage.Battle)
            {
                srcBGM.clip = bgmBattle;
                srcBGM.Play();
            }
            else if (page == PageManager.EPage.Tutorial || page == PageManager.EPage.Learn || page == PageManager.EPage.Trainer)
            {
                srcBGM.clip = bgmTutorial;
                srcBGM.Play();
            }
        }


        internal enum ESE : byte
        {
            StartConfirmed, StartCount, Start,
            StepConfirmed, StepConfirming,
            Wrong, Correct,
            TurnOn, TurnOff,
            Decide, Transit, Cancel,
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
            else if (se == ESE.Wrong)
            {
                srcSE.PlayOneShot(wrong, volume);
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
            else if (se == ESE.Correct)
            {
                srcPerform.PlayOneShot(correct, volume);
            }
            else if (se == ESE.TurnOn)
            {
                srcPerform.PlayOneShot(on, volume);
            }
            else if (se == ESE.TurnOff)
            {
                srcPerform.PlayOneShot(off, volume);
            }
            else if (se == ESE.Decide)
            {
                srcPerform.PlayOneShot(decide, volume);
            }
            else if (se == ESE.Cancel)
            {
                srcPerform.PlayOneShot(cancel, volume);
            }
            else if (se == ESE.Transit)
            {
                srcPerform.PlayOneShot(transit, volume);
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
