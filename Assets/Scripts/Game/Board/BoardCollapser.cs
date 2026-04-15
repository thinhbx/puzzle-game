using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.Game.Config;

namespace PuzzleGame.Game.Board
{
    /// <summary>
    /// Xử lý gravity - cho các block rơi xuống khi có khoảng trống bên dưới.
    /// </summary>
    public class BoardCollapser
    {
        private readonly GameBoard _board;
        private readonly GameConfig _config;

        public BoardCollapser(GameBoard board, GameConfig config)
        {
            _board = board;
            _config = config;
        }

        /// <summary>
        /// Di chuyển tất cả blocks rơi xuống để fill khoảng trống.
        /// </summary>
        public IEnumerator CollapseBoard()
        {
            var movingPieces = new List<(BlockPiece piece, Vector3 target)>();

            for (int x = 0; x < _board.Width; x++)
            {
                int emptyY = -1;

                for (int y = 0; y < _board.Height; y++)
                {
                    var cell = _board.GetCell(x, y);
                    if (cell.IsEmpty) continue;

                    if (cell.Piece == null)
                    {
                        if (emptyY < 0) emptyY = y;
                    }
                    else if (emptyY >= 0)
                    {
                        // Di chuyển piece xuống vị trí trống
                        var emptyCell = _board.GetCell(x, emptyY);
                        emptyCell.Piece = cell.Piece;
                        cell.Piece.SetCell(emptyCell);
                        cell.Piece = null;

                        movingPieces.Add((emptyCell.Piece, emptyCell.WorldPosition));
                        emptyY++;
                    }
                }
            }

            if (movingPieces.Count > 0)
            {
                yield return AnimateCollapse(movingPieces);
            }
        }

        private IEnumerator AnimateCollapse(List<(BlockPiece piece, Vector3 target)> movingPieces)
        {
            float elapsed = 0f;
            var startPositions = new List<Vector3>();
            foreach (var (piece, _) in movingPieces)
            {
                startPositions.Add(piece.transform.localPosition);
            }

            while (elapsed < _config.collapseDuration)
            {
                elapsed += Time.deltaTime;
                float t = _config.collapseCurve.Evaluate(elapsed / _config.collapseDuration);

                for (int i = 0; i < movingPieces.Count; i++)
                {
                    var (piece, target) = movingPieces[i];
                    if (piece != null)
                    {
                        piece.transform.localPosition = Vector3.Lerp(startPositions[i], target, t);
                    }
                }
                yield return null;
            }

            // Snap to final position
            foreach (var (piece, target) in movingPieces)
            {
                if (piece != null)
                {
                    piece.transform.localPosition = target;
                }
            }
        }
    }
}