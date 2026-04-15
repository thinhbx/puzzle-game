// Copyright (C) 2017-2020 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;

using PuzzleGame.Core;
using UnityEngine.UI;

namespace PuzzleGame.Game.Scenes
{
    /// <summary>
    /// This class contains the logic associated to the home scene.
    /// </summary>
    public class HomeScene : BaseScene
    {
        [SerializeField]
        private Button soundButton;

        [SerializeField]
        private Button musicButton;

        [SerializeField]
        private Button playButton;

        [SerializeField]
        private Button levelSelectButton;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            Assert.IsNotNull(soundButton);
            Assert.IsNotNull(musicButton);
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        private void Start()
        {
            UpdateButtons();

            if (playButton != null)
                playButton.onClick.AddListener(OnPlayButtonPressed);
            if (levelSelectButton != null)
                levelSelectButton.onClick.AddListener(OnLevelSelectPressed);
        }

        /// <summary>
        /// Called when the play button is pressed - go to last unlocked level.
        /// </summary>
        public void OnPlayButtonPressed()
        {
            int maxLevel = Managers.GameManager.Instance.GetMaxUnlockedLevel();
            PlayerPrefs.SetInt("selected_level", maxLevel);
            Transition.LoadLevel("Level", 0.5f, Color.black);
        }

        /// <summary>
        /// Called when the level select button is pressed.
        /// </summary>
        public void OnLevelSelectPressed()
        {
            OpenPopup<LevelSelectPopup>("Popups/LevelSelectPopup");
        }

        /// <summary>
        /// Called when the settings button is pressed.
        /// </summary>
        public void OnSettingsButtonPressed()
        {
            Debug.Log("Settings button pressed");
        }

        /// <summary>
        /// Called when the sound button is pressed.
        /// </summary>
        public void OnSoundButtonPressed()
        {
            var soundManager = Managers.SoundManager.Instance;
            soundManager.IsSoundEnabled = !soundManager.IsSoundEnabled;
            UpdateButtons();
        }

        /// <summary>
        /// Called when the music button is pressed.
        /// </summary>
        public void OnMusicButtonPressed()
        {
            var soundManager = Managers.SoundManager.Instance;
            soundManager.IsMusicEnabled = !soundManager.IsMusicEnabled;
            UpdateButtons();
        }

        /// <summary>
        /// Updates the state of the UI buttons according to the values stored in PlayerPrefs.
        /// </summary>
        public void UpdateButtons()
        {
            var sound = PlayerPrefs.GetInt("sound_enabled", 1);
            var music = PlayerPrefs.GetInt("music_enabled", 1);
        }
    }
}
