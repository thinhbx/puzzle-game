using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PuzzleGame.Core;

namespace PuzzleGame.Game.Scenes
{
    /// <summary>
    /// Popup hiển thị khi thắng level.
    /// </summary>
    public class WinPopup : Popup
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Image[] starImages;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button retryButton;

        private Action _onNext;
        private Action _onRetry;

        public void Setup(int score, int stars, Action onNext, Action onRetry)
        {
            _onNext = onNext;
            _onRetry = onRetry;

            if (scoreText != null)
                scoreText.text = score.ToString("N0");

            if (starImages != null)
            {
                for (int i = 0; i < starImages.Length; i++)
                {
                    if (starImages[i] != null)
                        starImages[i].color = i < stars ? Color.yellow : Color.gray;
                }
            }

            if (nextButton != null)
                nextButton.onClick.AddListener(OnNextPressed);
            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetryPressed);
        }

        private void OnNextPressed()
        {
            Close();
            _onNext?.Invoke();
        }

        private void OnRetryPressed()
        {
            Close();
            _onRetry?.Invoke();
        }
    }
}