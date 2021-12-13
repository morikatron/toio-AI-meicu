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
        public AudioSource sound;


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

        internal enum ESE : byte
        {
            StartConfirm, StartCount, Start,
            StepConfirm,
            Win, Lose, Draw,
        }
        internal void PlaySE(ESE se, float volume=0.5f)
        {
            IEnumerator ie = null;
            if (se == ESE.StartConfirm)
            {
                IEnumerator IE()
                {
                    PlaySoundEndless(69, volume);
                    yield return new WaitForSecondsRealtime(0.04f);
                    PlaySoundEndless(72, volume);
                    yield return new WaitForSecondsRealtime(0.2f);
                    StopSound();
                    iePlaySound = null;
                }
                ie = IE();
            }
            else if (se == ESE.StartCount)
            {
                IEnumerator IE()
                {
                    PlaySoundEndless(65, volume);
                    yield return new WaitForSecondsRealtime(0.5f);
                    StopSound();
                    iePlaySound = null;
                }
                ie = IE();
            }
            else if (se == ESE.Start)
            {
                IEnumerator IE()
                {
                    PlaySoundEndless(72, volume);
                    yield return new WaitForSecondsRealtime(1f);
                    StopSound();
                    iePlaySound = null;
                }
                ie = IE();
            }
            else if (se == ESE.StepConfirm)
            {
                IEnumerator IE()
                {
                    PlaySoundEndless(69, volume);
                    yield return new WaitForSecondsRealtime(0.04f);
                    PlaySoundEndless(72, volume);
                    yield return new WaitForSecondsRealtime(0.2f);
                    StopSound();
                    iePlaySound = null;
                }
                ie = IE();
            }
            else if (se == ESE.Win)
            {
                IEnumerator IE()
                {
                    PlaySoundEndless(69, volume);
                    yield return new WaitForSecondsRealtime(0.15f);
                    PlaySoundEndless(71, volume);
                    yield return new WaitForSecondsRealtime(0.15f);
                    PlaySoundEndless(76, volume);
                    yield return new WaitForSecondsRealtime(0.3f);
                    PlaySoundEndless(73, volume);
                    yield return new WaitForSecondsRealtime(0.08f);
                    PlaySoundEndless(76, volume);
                    yield return new WaitForSecondsRealtime(0.3f);
                    StopSound();
                    iePlaySound = null;
                }
                ie = IE();
            }
            else if (se == ESE.Lose)
            {
                IEnumerator IE()
                {
                    PlaySoundEndless(65, volume);
                    yield return new WaitForSecondsRealtime(0.15f);
                    PlaySoundEndless(63, volume);
                    yield return new WaitForSecondsRealtime(0.15f);
                    PlaySoundEndless(59, volume);
                    yield return new WaitForSecondsRealtime(0.3f);
                    StopSound();
                    iePlaySound = null;
                }
                ie = IE();
            }

            if (ie == null) return;

            if (iePlaySound != null)
                StopCoroutine(iePlaySound);
            iePlaySound = ie;
            StartCoroutine(iePlaySound);
        }

        IEnumerator iePlaySound = null;
        internal void PlaySound(int soundId, float duration, float volume=0.3f)
        {
            IEnumerator ie()
            {
                PlaySoundEndless(soundId, volume);
                yield return new WaitForSecondsRealtime(duration);
                StopSound();
                iePlaySound = null;
            }

            if (iePlaySound != null)
                StopCoroutine(iePlaySound);
            iePlaySound = ie();
            StartCoroutine(iePlaySound);
        }

        private int playingSoundId = -1;
        internal void PlaySoundEndless(int soundId, float volume)
        {
            if (soundId >= 128) { StopSound(); return; }
            if (soundId != playingSoundId)
            {
                playingSoundId = soundId;
                int octave = (int)(soundId/12);
                int idx = (int)(soundId%12);
                var clip = Resources.Load("Octave/" + (octave*12+9)) as AudioClip;
                sound.pitch = Mathf.Pow(2, ((float)idx-9)/12);
                sound.clip = clip;
            }
            sound.volume = volume;
            if (!sound.isPlaying)
                sound.Play();
        }
        internal void StopSound(){
            playingSoundId = -1;
            sound.Stop();
            sound.clip = null;
        }
    }

}
