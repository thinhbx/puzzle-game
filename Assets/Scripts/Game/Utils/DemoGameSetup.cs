using UnityEngine;
using PuzzleGame.Game.Board;
using PuzzleGame.Game.Config;
using PuzzleGame.Game.Managers;
using PuzzleGame.Game.Common;
using System.Collections.Generic;

namespace PuzzleGame.Game.Utils
{
    /// <summary>
    /// Auto-setup scene cho demo puzzle match game.
    /// Attach script này vào một GameObject trong Level scene, tự động tạo board + UI khi Play.
    /// </summary>
    public class DemoGameSetup : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GameConfig config;
        [SerializeField] private int levelId = 1;
        [SerializeField] private bool useDefaultLevel = true;

        [Header("Board Size")]
        [SerializeField] private int boardWidth = 7;
        [SerializeField] private int boardHeight = 9;
        [SerializeField] private int numberOfBlockTypes = 5;

        private GameBoard _board;
        private BoardInput _boardInput;

        private void Start()
        {
            SetupDemo();
        }

        private void SetupDemo()
        {
            // Ensure GameConfig exists
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<GameConfig>();
                config.defaultBoardWidth = boardWidth;
                config.defaultBoardHeight = boardHeight;
                config.numberOfBlockTypes = numberOfBlockTypes;
            }

            // Create board container
            var boardObj = new GameObject("GameBoard");
            var boardContainer = new GameObject("BoardContainer");
            boardContainer.transform.SetParent(boardObj.transform);

            _board = boardObj.AddComponent<GameBoard>();
            _boardInput = boardObj.AddComponent<BoardInput>();

            // Generate block prefabs
            var blockPrefabs = new GameObject[config.numberOfBlockTypes];
            for (int i = 0; i < config.numberOfBlockTypes; i++)
            {
                blockPrefabs[i] = DemoBlockGenerator.CreateSimpleBlock(i, config.cellSize * 0.85f);
                blockPrefabs[i].SetActive(false); // Deactivate template
            }

            // Generate cell prefab
            var cellPrefab = DemoBlockGenerator.CreateSimpleCell(config.cellSize);
            cellPrefab.SetActive(false);

            // Set serialized fields via reflection (since they're [SerializeField])
            SetPrivateField(_board, "config", config);
            SetPrivateField(_board, "boardContainer", boardContainer.transform);
            SetPrivateField(_board, "cellPrefab", cellPrefab);
            SetPrivateField(_board, "blockPrefabs", blockPrefabs);
            SetPrivateField(_boardInput, "board", _board);

            // Create level
            Level level;
            if (useDefaultLevel)
            {
                var levelManager = gameObject.AddComponent<LevelManager>();
                level = levelManager.CreateDefaultLevel(levelId);
                level.width = boardWidth;
                level.height = boardHeight;
            }
            else
            {
                level = CreateSimpleLevel();
            }

            // Start game via GameManager
            var gameManager = GameManager.Instance;
            SetPrivateField(gameManager, "config", config);
            gameManager.StartLevel(level, _board);

            // Subscribe for debug logs
            gameManager.OnScoreChanged += score => Debug.Log($"[Demo] Score: {score}");
            gameManager.OnMovesChanged += moves => Debug.Log($"[Demo] Moves: {moves}");
            gameManager.OnCombo += combo => Debug.Log($"[Demo] Combo x{combo}!");
            gameManager.OnWin += () => Debug.Log("[Demo] YOU WIN!");
            gameManager.OnLose += () => Debug.Log("[Demo] YOU LOSE!");

            // Adjust camera to fit board
            AdjustCamera();

            Debug.Log($"[Demo] Puzzle Match Game started! Board: {boardWidth}x{boardHeight}, Blocks: {numberOfBlockTypes}");
        }

        private Level CreateSimpleLevel()
        {
            var level = new Level
            {
                id = levelId,
                width = boardWidth,
                height = boardHeight,
                limitType = LimitType.Moves,
                limit = 30,
                score1 = 500,
                score2 = 1500,
                score3 = 3000,
                goals = new List<Goal> { new ReachScoreGoal(500) },
                availableBoosters = new Dictionary<BoosterType, bool>
                {
                    { BoosterType.HorizontalBomb, true },
                    { BoosterType.VerticalBomb, true },
                    { BoosterType.ColorBomb, true },
                    { BoosterType.DynamicBomb, true }
                }
            };

            for (int i = 0; i < level.width * level.height; i++)
            {
                level.tiles.Add(new BlockTile
                {
                    type = BlockType.RandomBlock,
                    blockerType = BlockerType.None
                });
            }

            return level;
        }

        private void AdjustCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;

            cam.orthographic = true;
            float boardWorldWidth = boardWidth * (config.cellSize + config.cellSpacing);
            float boardWorldHeight = boardHeight * (config.cellSize + config.cellSpacing);

            float screenRatio = (float)Screen.width / Screen.height;
            float targetRatio = boardWorldWidth / boardWorldHeight;

            if (screenRatio >= targetRatio)
            {
                cam.orthographicSize = boardWorldHeight / 2f + 1f;
            }
            else
            {
                cam.orthographicSize = (boardWorldWidth / screenRatio) / 2f + 1f;
            }

            cam.transform.position = new Vector3(0, 0, -10);
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var type = obj.GetType();
            while (type != null)
            {
                var field = type.GetField(fieldName,
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public);
                if (field != null)
                {
                    field.SetValue(obj, value);
                    return;
                }
                type = type.BaseType;
            }
            Debug.LogWarning($"Field '{fieldName}' not found on {obj.GetType().Name}");
        }
    }
}