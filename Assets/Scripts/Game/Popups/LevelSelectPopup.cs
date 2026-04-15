using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PuzzleGame.Core;
using PuzzleGame.Game.Managers;

namespace PuzzleGame.Game.Scenes
{
    /// <summary>
    /// Popup chọn level từ level map.
    /// </summary>
    public class LevelSelectPopup : Popup
    {
        [SerializeField] private Transform levelButtonContainer;
        [SerializeField] private GameObject levelButtonPrefab;
        [SerializeField] private int levelsPerPage = 20;

        private int _currentPage;

        protected override void Start()
        {
            base.Start();
            ShowPage(0);
        }

        public void ShowPage(int page)
        {
            _currentPage = page;

            // Clear existing buttons
            foreach (Transform child in levelButtonContainer)
            {
                Destroy(child.gameObject);
            }

            int maxLevel = GameManager.Instance.GetMaxUnlockedLevel();
            int startLevel = page * levelsPerPage + 1;
            int endLevel = startLevel + levelsPerPage;

            for (int i = startLevel; i < endLevel; i++)
            {
                if (levelButtonPrefab == null) break;

                var buttonObj = Instantiate(levelButtonPrefab, levelButtonContainer);
                var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.text = i.ToString();

                var button = buttonObj.GetComponent<Button>();
                bool isUnlocked = i <= maxLevel;

                if (button != null)
                {
                    int levelId = i; // Capture for closure
                    button.interactable = isUnlocked;
                    button.onClick.AddListener(() => OnLevelSelected(levelId));
                }

                // Show stars
                int stars = GameManager.Instance.GetLevelStars(i);
                var starImages = buttonObj.GetComponentsInChildren<Image>();
                // Star display logic would go here based on your prefab structure
            }
        }

        public void NextPage()
        {
            ShowPage(_currentPage + 1);
        }

        public void PrevPage()
        {
            if (_currentPage > 0)
                ShowPage(_currentPage - 1);
        }

        private void OnLevelSelected(int levelId)
        {
            Close();
            // Transition to game scene with selected level
            PlayerPrefs.SetInt("selected_level", levelId);
            Transition.LoadLevel("Level", 0.5f, Color.black);
        }
    }
}