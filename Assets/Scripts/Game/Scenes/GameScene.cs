using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PuzzleGame.Core;
using PuzzleGame.Game.Board;
using PuzzleGame.Game.Managers;
using PuzzleGame.Game.Common;

namespace PuzzleGame.Game.Scenes
{
    /// <summary>
    /// GameScene - scene chính cho gameplay puzzle match.
    /// </summary>
    public class GameScene : BaseScene
    {
        [Header("Board")]
        [SerializeField] private GameBoard board;
        [SerializeField] private BoardInput boardInput;

        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI movesText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI goalText;
        [SerializeField] private Image[] starImages;
        [SerializeField] private Slider progressBar;

        [Header("Buttons")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button restartButton;

        [Header("Combo")]
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private float comboDisplayDuration = 1.5f;

        private LevelManager _levelManager;
        private GameManager _gameManager;
        private int _currentLevelId = 1;
        private float _comboTimer;

        private void Awake()
        {
            _levelManager = GetComponent<LevelManager>();
            if (_levelManager == null)
                _levelManager = gameObject.AddComponent<LevelManager>();
        }

        private void Start()
        {
            _gameManager = GameManager.Instance;

            // Subscribe UI events
            _gameManager.OnScoreChanged += UpdateScore;
            _gameManager.OnMovesChanged += UpdateMoves;
            _gameManager.OnTimeChanged += UpdateTimer;
            _gameManager.OnCombo += ShowCombo;
            _gameManager.OnPhaseChanged += OnPhaseChanged;

            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPausePressed);
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartPressed);

            // Load level
            LoadLevel();
        }

        private void OnDestroy()
        {
            if (_gameManager != null)
            {
                _gameManager.OnScoreChanged -= UpdateScore;
                _gameManager.OnMovesChanged -= UpdateMoves;
                _gameManager.OnTimeChanged -= UpdateTimer;
                _gameManager.OnCombo -= ShowCombo;
                _gameManager.OnPhaseChanged -= OnPhaseChanged;
            }
        }

        private void Update()
        {
            // Hide combo text after duration
            if (comboText != null && comboText.gameObject.activeSelf)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0)
                {
                    comboText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Load và bắt đầu chơi level.
        /// </summary>
        public void LoadLevel()
        {
            // Thử load từ JSON trước, nếu không có thì tạo default
            Level level;
            if (_levelManager.LevelExists(_currentLevelId))
            {
                level = _levelManager.LoadLevel(_currentLevelId);
            }
            else
            {
                level = _levelManager.CreateDefaultLevel(_currentLevelId);
            }

            if (level == null)
            {
                Debug.LogError($"Failed to load level {_currentLevelId}");
                return;
            }

            // Update HUD
            if (levelText != null)
                levelText.text = $"Level {_currentLevelId}";
            if (goalText != null && level.goals.Count > 0)
                goalText.text = level.goals[0].ToString();

            UpdateScore(0);
            UpdateMoves(level.limit);
            UpdateStars(0);

            if (timerText != null)
                timerText.gameObject.SetActive(level.limitType == LimitType.Time);
            if (movesText != null)
                movesText.gameObject.SetActive(level.limitType == LimitType.Moves);

            _gameManager.StartLevel(level, board);
        }

        public void SetLevelId(int levelId)
        {
            _currentLevelId = levelId;
        }

        #region UI Updates

        private void UpdateScore(int score)
        {
            if (scoreText != null)
                scoreText.text = score.ToString("N0");

            // Update stars based on score thresholds
            if (_gameManager.CurrentLevel != null)
            {
                int stars = 0;
                if (score >= _gameManager.CurrentLevel.score1) stars = 1;
                if (score >= _gameManager.CurrentLevel.score2) stars = 2;
                if (score >= _gameManager.CurrentLevel.score3) stars = 3;
                UpdateStars(stars);

                // Update progress bar
                if (progressBar != null)
                {
                    float maxScore = _gameManager.CurrentLevel.score3;
                    progressBar.value = maxScore > 0 ? Mathf.Clamp01(score / maxScore) : 0;
                }
            }
        }

        private void UpdateMoves(int moves)
        {
            if (movesText != null)
                movesText.text = moves.ToString();
        }

        private void UpdateTimer(float time)
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(time / 60);
                int seconds = Mathf.FloorToInt(time % 60);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }
        }

        private void UpdateStars(int stars)
        {
            if (starImages == null) return;
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] != null)
                {
                    starImages[i].color = i < stars ? Color.yellow : Color.gray;
                }
            }
        }

        private void ShowCombo(int comboCount)
        {
            if (comboText != null && comboCount > 1)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = $"Combo x{comboCount}!";
                _comboTimer = comboDisplayDuration;
            }
        }

        #endregion

        #region Phase Handling

        private void OnPhaseChanged(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.Win:
                    ShowWinPopup();
                    break;
                case GamePhase.Lose:
                    ShowLosePopup();
                    break;
            }
        }

        private void ShowWinPopup()
        {
            OpenPopup<WinPopup>("Popups/WinPopup", popup =>
            {
                popup.Setup(
                    _gameManager.CurrentGameState.score,
                    _gameManager.GetLevelStars(_currentLevelId),
                    () => { NextLevel(); },
                    () => { RestartLevel(); }
                );
            });
        }

        private void ShowLosePopup()
        {
            OpenPopup<LosePopup>("Popups/LosePopup", popup =>
            {
                popup.Setup(
                    _gameManager.CurrentGameState.score,
                    () => { RestartLevel(); },
                    () => { GoHome(); }
                );
            });
        }

        #endregion

        #region Actions

        public void OnPausePressed()
        {
            _gameManager.PauseGame();
            OpenPopup<PausePopup>("Popups/PausePopup", popup =>
            {
                popup.Setup(
                    () => { _gameManager.ResumeGame(); },
                    () => { RestartLevel(); },
                    () => { GoHome(); }
                );
            });
        }

        public void OnRestartPressed()
        {
            RestartLevel();
        }

        private void RestartLevel()
        {
            _gameManager.StopLevel();
            LoadLevel();
        }

        private void NextLevel()
        {
            _gameManager.StopLevel();
            _currentLevelId++;
            LoadLevel();
        }

        private void GoHome()
        {
            _gameManager.StopLevel();
            Time.timeScale = 1;
            Transition.LoadLevel("Home", 0.5f, Color.black);
        }

        #endregion
    }
}