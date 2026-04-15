using UnityEngine;
using PuzzleGame.Game.Common;

namespace PuzzleGame.Game.Board
{
    /// <summary>
    /// Data class cho mỗi ô trên board.
    /// </summary>
    public class BoardCell
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public Vector3 WorldPosition { get; private set; }
        public BlockPiece Piece { get; set; }
        public bool IsEmpty { get; set; }
        public BlockerType BlockerType { get; set; } = BlockerType.None;
        public int BlockerHp { get; set; } = 1;

        public BoardCell(int x, int y, Vector3 worldPos)
        {
            X = x;
            Y = y;
            WorldPosition = worldPos;
        }

        public bool HasPiece => Piece != null && !IsEmpty;

        public bool IsMovable => !IsEmpty && Piece != null && BlockerType == BlockerType.None;
    }
}