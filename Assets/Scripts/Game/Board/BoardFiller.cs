using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.Game.Config;

namespace PuzzleGame.Game.Board
{
    /// <summary>
    /// Fill các ô trống sau khi collapse bằng các block mới từ trên rơi xuống.
    /// </summary>
    public class BoardFiller
    {
        private readonly GameBoard _board;
        private readonly GameConfig _config;

        public BoardFiller(GameBoard board, GameConfig config)
        {
            _board = board;
            _config = config;
        }

        /// <summary>
        /// Fill tất cả ô trống trên board.
        /// </summary>
        public IEnumerator FillBoard()
        {
            var newPieces = new List<(BlockPiece piece, Vector3 startPos, Vector3 targetPos)>();

            for (int x = 0; x < _board.Width; x++)
            {
                int emptyCount = 0;
                for (int y = _board.Height - 1; y >= 0; y--)
                {
                    var cell = _board.GetCell(x, y);
                    if (cell.IsEmpty) continue;

                    if (cell.Piece == null)
                    {
                        emptyCount++;
                        var piece = _board.CreateRandomPieceAt(x, y);
                        if (piece != null)
                        {
                            // Bắt đầu từ trên board rơi xuống
                            Vector3 startPos = cell.WorldPosition + Vector3.up * (emptyCount + 1) * (_board.Config.cellSize + _board.Config.cellSpacing);
                            piece.transform.localPosition = startPos;
                            newPieces.Add((piece, startPos, cell.WorldPosition));
                        }
                    }
                }
            }

            if (newPieces.Count > 0)
            {
                yield return AnimateFill(newPieces);
            }
        }

        private IEnumerator AnimateFill(List<(BlockPiece piece, Vector3 startPos, Vector3 targetPos)> newPieces)
        {
            float elapsed = 0f;

            while (elapsed < _config.fillDuration)
            {
                elapsed += Time.deltaTime;
                float t = _config.collapseCurve.Evaluate(elapsed / _config.fillDuration);

                foreach (var (piece, startPos, targetPos) in newPieces)
                {
                    if (piece != null)
                    {
                        piece.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
                    }
                }
                yield return null;
            }

            // Snap to final position
            foreach (var (piece, _, targetPos) in newPieces)
            {
                if (piece != null)
                {
                    piece.transform.localPosition = targetPos;
                }
            }
        }
    }
}