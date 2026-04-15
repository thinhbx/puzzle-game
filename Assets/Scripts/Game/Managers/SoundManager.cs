using System;
using UnityEngine;
using PuzzleGame.Core;
using PuzzleGame.Game.Config;

namespace PuzzleGame.Game.Managers
{
    /// <summary>
    /// SoundManager - quản lý sound effects và music cho game.
    /// </summary>
    public class SoundManager : Singleton<SoundManager>
    {
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        public bool IsSoundEnabled
        {
            get => PlayerPrefs.GetInt("sound_enabled", 1) == 1;
            set
            {
                PlayerPrefs.SetInt("sound_enabled", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public bool IsMusicEnabled
        {
            get => PlayerPrefs.GetInt("music_enabled", 1) == 1;
            set
            {
                PlayerPrefs.SetInt("music_enabled", value ? 1 : 0);
                if (musicSource != null)
                    musicSource.mute = !value;
                PlayerPrefs.Save();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
            }
            musicSource.mute = !IsMusicEnabled;
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip != null && IsSoundEnabled && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip != null && musicSource != null)
            {
                musicSource.clip = clip;
                musicSource.mute = !IsMusicEnabled;
                musicSource.Play();
            }
        }

        public void StopMusic()
        {
            if (musicSource != null)
                musicSource.Stop();
        }
    }
}