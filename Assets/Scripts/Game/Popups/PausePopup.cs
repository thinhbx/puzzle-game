using System;
using UnityEngine;
using UnityEngine.UI;
using PuzzleGame.Core;

namespace PuzzleGame.Game.Scenes
{
    /// <summary>
    /// Popup hiển thị khi pause game.
    /// </summary>
    public class PausePopup : Popup
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button soundToggle;
        [SerializeField] private Button musicToggle;

        private Action _onResume;
        private Action _onRetry;
        private Action _onHome;

        public void Setup(Action onResume, Action onRetry, Action onHome)
        {
            _onResume = onResume;
            _onRetry = onRetry;
            _onHome = onHome;

            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumePressed);
            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetryPressed);
            if (homeButton != null)
                homeButton.onClick.AddListener(OnHomePressed);
        }

        private void OnResumePressed()
        {
            Time.timeScale = 1;
            Close();
            _onResume?.Invoke();
        }

        private void OnRetryPressed()
        {
            Time.timeScale = 1;
            Close();
            _onRetry?.Invoke();
        }

        private void OnHomePressed()
        {
            Time.timeScale = 1;
            Close();
            _onHome?.Invoke();
        }
    }
}