using System.Collections.Generic;
using UnityEngine;
using FullSerializer;
using PuzzleGame.Core;
using PuzzleGame.Game.Common;

namespace PuzzleGame.Game.Managers
{
    /// <summary>
    /// LevelManager - load và quản lý level data từ JSON files.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        private static readonly fsSerializer _serializer = new fsSerializer();
        private Dictionary<int, Level> _cachedLevels = new Dictionary<int, Level>();

        /// <summary>
        /// Load level data từ Resources.
        /// </summary>
        public Level LoadLevel(int levelId)
        {
            if (_cachedLevels.TryGetValue(levelId, out var cached))
                return cached;

            string path = $"Levels/level_{levelId}";
            var level = FileUtils.LoadJson<Level>(_serializer, path);

            if (level != null)
            {
                level.id = levelId;
                _cachedLevels[levelId] = level;
            }

            return level;
        }

        /// <summary>
        /// Tạo level mặc định cho demo nếu không có file JSON.
        /// </summary>
        public Level CreateDefaultLevel(int levelId = 1)
        {
            var level = new Level
            {
                id = levelId,
                width = 7,
                height = 9,
                limitType = LimitType.Moves,
                limit = 30,
                score1 = 500,
                score2 = 1000,
                score3 = 2000,
                availableColors = new List<ColorBlockType>
                {
                    ColorBlockType.ColorBlock1,
                    ColorBlockType.ColorBlock2,
                    ColorBlockType.ColorBlock3,
                    ColorBlockType.ColorBlock4,
                    ColorBlockType.ColorBlock5
                },
                goals = new List<Goal>
                {
                    new ReachScoreGoal(500)
                },
                availableBoosters = new Dictionary<BoosterType, bool>
                {
                    { BoosterType.HorizontalBomb, true },
                    { BoosterType.VerticalBomb, true },
                    { BoosterType.ColorBomb, true },
                    { BoosterType.DynamicBomb, true }
                }
            };

            // Fill tiles với random blocks
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

        /// <summary>
        /// Tạo level với collect block goal.
        /// </summary>
        public Level CreateCollectLevel(int levelId, BlockType targetBlock, int targetCount, int moves = 25)
        {
            var level = CreateDefaultLevel(levelId);
            level.limit = moves;
            level.goals = new List<Goal>
            {
                new CollectBlockGoal(targetBlock, targetCount)
            };
            return level;
        }

        /// <summary>
        /// Tạo level time-based.
        /// </summary>
        public Level CreateTimedLevel(int levelId, int timeSeconds = 60, int targetScore = 1000)
        {
            var level = CreateDefaultLevel(levelId);
            level.limitType = LimitType.Time;
            level.limit = timeSeconds;
            level.goals = new List<Goal>
            {
                new ReachScoreGoal(targetScore)
            };
            return level;
        }

        /// <summary>
        /// Kiểm tra level data có tồn tại trong Resources không.
        /// </summary>
        public bool LevelExists(int levelId)
        {
            string path = $"Levels/level_{levelId}";
            return FileUtils.FileExists(path);
        }

        public void ClearCache()
        {
            _cachedLevels.Clear();
        }
    }
}