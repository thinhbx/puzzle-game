using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PuzzleGame.Core;

namespace PuzzleGame.Game.Scenes
{
    /// <summary>
    /// Popup hiển thị khi thua level.
    /// </summary>
    public class LosePopup : Popup
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button homeButton;

        private Action _onRetry;
        private Action _onHome;

        public void Setup(int score, Action onRetry, Action onHome)
        {
            _onRetry = onRetry;
            _onHome = onHome;

            if (scoreText != null)
                scoreText.text = score.ToString("N0");

            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetryPressed);
            if (homeButton != null)
                homeButton.onClick.AddListener(OnHomePressed);
        }

        private void OnRetryPressed()
        {
            Close();
            _onRetry?.Invoke();
        }

        private void OnHomePressed()
        {
            Close();
            _onHome?.Invoke();
        }
    }
}