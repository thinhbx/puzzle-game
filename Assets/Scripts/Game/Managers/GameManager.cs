using System;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.Core;
using PuzzleGame.Game.Board;
using PuzzleGame.Game.Common;
using PuzzleGame.Game.Config;

namespace PuzzleGame.Game.Managers
{
    /// <summary>
    /// Trạng thái tổng thể của game.
    /// </summary>
    public enum GamePhase
    {
        None,
        Loading,
        Playing,
        Paused,
        Win,
        Lose
    }

    /// <summary>
    /// GameManager - quản lý flow game tổng thể: level, scoring, win/lose conditions.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [Header("References")]
        [SerializeField] private GameConfig config;

        public GamePhase Phase { get; private set; } = GamePhase.None;
        public Level CurrentLevel { get; private set; }
        public GameState CurrentGameState { get; private set; }
        public int CurrentLevelId { get; private set; }
        public int MovesRemaining { get; private set; }
        public float TimeRemaining { get; private set; }
        public GameConfig Config => config;

        public event Action<GamePhase> OnPhaseChanged;
        public event Action<int> OnMovesChanged;
        public event Action<float> OnTimeChanged;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnCombo;
        public event Action OnWin;
        public event Action OnLose;

        private GameBoard _board;

        protected override void Awake()
        {
            base.Awake();
            CurrentGameState = new GameState();
        }

        /// <summary>
        /// Bắt đầu chơi một level.
        /// </summary>
        public void StartLevel(Level level, GameBoard board)
        {
            CurrentLevel = level;
            CurrentLevelId = level.id;
            _board = board;

            // Reset game state
            CurrentGameState.Reset();
            MovesRemaining = level.limit;
            TimeRemaining = level.limitType == LimitType.Time ? level.limit : 0;

            // Subscribe to board events
            _board.OnMatchFound += HandleMatchFound;
            _board.OnCombo += HandleCombo;
            _board.OnBoardStable += HandleBoardStable;
            _board.OnNoMatchesAvailable += HandleNoMatches;

            // Initialize board
            _board.Initialize(level.width, level.height, level);

            SetPhase(GamePhase.Playing);
        }

        /// <summary>
        /// Dừng level hiện tại.
        /// </summary>
        public void StopLevel()
        {
            if (_board != null)
            {
                _board.OnMatchFound -= HandleMatchFound;
                _board.OnCombo -= HandleCombo;
                _board.OnBoardStable -= HandleBoardStable;
                _board.OnNoMatchesAvailable -= HandleNoMatches;
                _board.Clear();
            }
            SetPhase(GamePhase.None);
        }

        public void PauseGame()
        {
            if (Phase == GamePhase.Playing)
            {
                SetPhase(GamePhase.Paused);
                Time.timeScale = 0;
            }
        }

        public void ResumeGame()
        {
            if (Phase == GamePhase.Paused)
            {
                Time.timeScale = 1;
                SetPhase(GamePhase.Playing);
            }
        }

        private void Update()
        {
            if (Phase != GamePhase.Playing) return;

            // Time-based level countdown
            if (CurrentLevel != null && CurrentLevel.limitType == LimitType.Time)
            {
                TimeRemaining -= Time.deltaTime;
                OnTimeChanged?.Invoke(TimeRemaining);

                if (TimeRemaining <= 0)
                {
                    TimeRemaining = 0;
                    CheckWinLose();
                }
            }
        }

        private void HandleMatchFound(List<BoardCell> matchedCells)
        {
            int score = CalculateMatchScore(matchedCells.Count);
            CurrentGameState.score += score;
            OnScoreChanged?.Invoke(CurrentGameState.score);

            // Track collected blocks
            foreach (var cell in matchedCells)
            {
                if (cell.Piece != null)
                {
                    var blockType = (BlockType)cell.Piece.TypeIndex;
                    if (CurrentGameState.collectedBlocks.ContainsKey(blockType))
                    {
                        CurrentGameState.collectedBlocks[blockType]++;
                    }

                    // Track blockers
                    if (cell.BlockerType != BlockerType.None)
                    {
                        if (CurrentGameState.collectedBlockers.ContainsKey(cell.BlockerType))
                        {
                            CurrentGameState.collectedBlockers[cell.BlockerType]++;
                        }
                    }
                }
            }
        }

        private void HandleCombo(int comboCount)
        {
            OnCombo?.Invoke(comboCount);
        }

        private void HandleBoardStable()
        {
            if (CurrentLevel == null) return;

            // Trừ move (chỉ cho move-based levels)
            if (CurrentLevel.limitType == LimitType.Moves)
            {
                MovesRemaining--;
                OnMovesChanged?.Invoke(MovesRemaining);
            }

            CheckWinLose();
        }

        private void HandleNoMatches()
        {
            if (_board != null)
            {
                _board.ShuffleBoard();
            }
        }

        private void CheckWinLose()
        {
            // Check win - tất cả goals đều completed
            bool allGoalsCompleted = true;
            foreach (var goal in CurrentLevel.goals)
            {
                if (!goal.IsCompleted(CurrentGameState))
                {
                    allGoalsCompleted = false;
                    break;
                }
            }

            if (allGoalsCompleted)
            {
                WinGame();
                return;
            }

            // Check lose
            bool outOfMoves = CurrentLevel.limitType == LimitType.Moves && MovesRemaining <= 0;
            bool outOfTime = CurrentLevel.limitType == LimitType.Time && TimeRemaining <= 0;

            if (outOfMoves || outOfTime)
            {
                LoseGame();
            }
        }

        private void WinGame()
        {
            SetPhase(GamePhase.Win);
            _board.SetState(BoardState.GameOver);

            // Calculate stars
            int stars = 1;
            if (CurrentGameState.score >= CurrentLevel.score2) stars = 2;
            if (CurrentGameState.score >= CurrentLevel.score3) stars = 3;

            // Save progress
            int savedStars = PlayerPrefs.GetInt($"level_{CurrentLevelId}_stars", 0);
            if (stars > savedStars)
            {
                PlayerPrefs.SetInt($"level_{CurrentLevelId}_stars", stars);
            }

            int currentMaxLevel = PlayerPrefs.GetInt("max_level", 1);
            if (CurrentLevelId >= currentMaxLevel)
            {
                PlayerPrefs.SetInt("max_level", CurrentLevelId + 1);
            }

            PlayerPrefs.Save();
            OnWin?.Invoke();

            if (config.winSound != null)
            {
                AudioSource.PlayClipAtPoint(config.winSound, Vector3.zero);
            }
        }

        private void LoseGame()
        {
            SetPhase(GamePhase.Lose);
            _board.SetState(BoardState.GameOver);
            OnLose?.Invoke();

            if (config.loseSound != null)
            {
                AudioSource.PlayClipAtPoint(config.loseSound, Vector3.zero);
            }
        }

        private int CalculateMatchScore(int matchCount)
        {
            int baseScore = config.matchScoreBase * matchCount;
            // Bonus cho match lớn hơn minimum
            int bonus = Mathf.Max(0, matchCount - config.minMatchCount) * config.comboScoreMultiplier;
            return baseScore + bonus;
        }

        private void SetPhase(GamePhase phase)
        {
            Phase = phase;
            OnPhaseChanged?.Invoke(phase);
        }

        /// <summary>
        /// Lấy số sao đã đạt được của một level.
        /// </summary>
        public int GetLevelStars(int levelId)
        {
            return PlayerPrefs.GetInt($"level_{levelId}_stars", 0);
        }

        /// <summary>
        /// Lấy max level đã unlock.
        /// </summary>
        public int GetMaxUnlockedLevel()
        {
            return PlayerPrefs.GetInt("max_level", 1);
        }
    }
}