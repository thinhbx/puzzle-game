namespace PuzzleGame.Game.Common
{
     public class LevelTile
    {
        public BlockerType blockerType;
    }

  
    public class BlockTile : LevelTile
    {
        public BlockType type;
    }

   
    public class BoosterTile : LevelTile
    {
        public BoosterType type;
    }
}
