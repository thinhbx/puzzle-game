using System.Collections.Generic;
using UnityEngine;
namespace PuzzleGame.Game.Common
{
    public class GameState
    {
       public int score;
       public Dictionary<BlockerType, int> collectedBlockers = new Dictionary<BlockerType, int>();
         public Dictionary<BlockType, int> collectedBlocks = new Dictionary<BlockType, int>();

         public void Reset()
        {
            score = 0;
            collectedBlockers.Clear();
            collectedBlocks.Clear();
            foreach(var blocker in System.Enum.GetValues(typeof(BlockerType)))
            {
                collectedBlockers.Add((BlockerType) blocker, 0);
            }
            foreach(var block in System.Enum.GetValues(typeof(BlockType)))
            {
                collectedBlocks.Add((BlockType) block, 0);
            }
        }
    }
}


