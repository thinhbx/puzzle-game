using UnityEngine;

namespace PuzzleGame.Game.Common
{
    public abstract class Goal 
    {
        public abstract bool IsCompleted(GameState gameState);

    }
    public class ReachScoreGoal : Goal
    {
        public int targetScore;

        public ReachScoreGoal(int targetScore)
        {
            this.targetScore = targetScore;
        }

        public override bool IsCompleted(GameState gameState)
        {
            return gameState.score >= targetScore;
        }

        public override string ToString()
        {
            return $"Reach Score: {targetScore}";
        }
    }
    public class CollectBlockGoal : Goal
    {
        public BlockType blockType;
        public int targetCount;

        public CollectBlockGoal(BlockType blockType, int targetCount)
        {
            this.blockType = blockType;
            this.targetCount = targetCount;
        }

        public override bool IsCompleted(GameState gameState)
        {
            return gameState.collectedBlocks[blockType] >= targetCount;
        }

        public override string ToString()
        {
            return $"Collect {targetCount} of {blockType}";
        }
    }
    public class CollectBlockerGoal : Goal
    {
        public BlockerType blockerType;
        public int targetCount;

        public CollectBlockerGoal(BlockerType blockerType, int targetCount)
        {
            this.blockerType = blockerType;
            this.targetCount = targetCount;
        }

        public override bool IsCompleted(GameState gameState)
        {
            return gameState.collectedBlockers[blockerType] >= targetCount;
        }

        public override string ToString()
        {
            return $"Collect {targetCount} of {blockerType}";
        }
    }
}

