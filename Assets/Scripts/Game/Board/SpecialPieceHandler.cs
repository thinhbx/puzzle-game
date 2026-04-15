using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.Game.Common;
using PuzzleGame.Game.Config;

namespace PuzzleGame.Game.Board
{
    /// <summary>
    /// Xử lý tạo và kích hoạt special pieces (boosters) khi match đặc biệt.
    /// </summary>
    public class SpecialPieceHandler
    {
        private readonly GameBoard _board;
        private readonly GameConfig _config;

        public SpecialPieceHandler(GameBoard board, GameConfig config)
        {
            _board = board;
            _config = config;
        }

        /// <summary>
        /// Kiểm tra match có đủ điều kiện tạo special piece không.
        /// Nếu match >= 5: ColorBomb, match == 4 ngang: HorizontalBomb, match == 4 dọc: VerticalBomb.
        /// </summary>
        public void CheckAndCreateSpecial(List<BoardCell> match)
        {
            if (match == null || match.Count < _config.horizontalBombMatchCount)
                return;

            // Xác định cell tạo special (ưu tiên cell ở giữa match)
            int midIndex = match.Count / 2;
            var targetCell = match[midIndex];
            int typeIndex = targetCell.Piece != null ? targetCell.Piece.TypeIndex : 0;

            BoosterType boosterType;

            if (match.Count >= _config.colorBombMatchCount)
            {
                boosterType = BoosterType.ColorBomb;
            }
            else if (IsHorizontalMatch(match))
            {
                boosterType = BoosterType.HorizontalBomb;
            }
            else if (IsVerticalMatch(match))
            {
                boosterType = BoosterType.VerticalBomb;
            }
            else
            {
                boosterType = BoosterType.DynamicBomb;
            }

            // Mark cell này để không bị destroy trong match, sẽ được replace bằng special piece
            if (targetCell.Piece != null)
            {
                Object.Destroy(targetCell.Piece.gameObject);
                targetCell.Piece = null;
            }

            // Remove target cell from match list so it won't be destroyed
            match.Remove(targetCell);

            _board.CreateSpecialPieceAt(targetCell.X, targetCell.Y, typeIndex, boosterType);
        }

        /// <summary>
        /// Kích hoạt booster effect.
        /// </summary>
        public IEnumerator ActivateBooster(BoardCell cell)
        {
            if (cell.Piece == null || !cell.Piece.IsSpecial) yield break;

            var boosterType = cell.Piece.BoosterType;

            switch (boosterType)
            {
                case BoosterType.HorizontalBomb:
                    yield return ActivateHorizontalBomb(cell);
                    break;
                case BoosterType.VerticalBomb:
                    yield return ActivateVerticalBomb(cell);
                    break;
                case BoosterType.ColorBomb:
                    yield return ActivateColorBomb(cell);
                    break;
                case BoosterType.DynamicBomb:
                    yield return ActivateDynamicBomb(cell);
                    break;
            }
        }

        private IEnumerator ActivateHorizontalBomb(BoardCell cell)
        {
            var cellsToDestroy = new List<BoardCell>();
            for (int x = 0; x < _board.Width; x++)
            {
                var c = _board.GetCell(x, cell.Y);
                if (c != null && !c.IsEmpty && c.Piece != null)
                {
                    cellsToDestroy.Add(c);
                }
            }
            yield return DestroyBoostedCells(cellsToDestroy);
        }

        private IEnumerator ActivateVerticalBomb(BoardCell cell)
        {
            var cellsToDestroy = new List<BoardCell>();
            for (int y = 0; y < _board.Height; y++)
            {
                var c = _board.GetCell(cell.X, y);
                if (c != null && !c.IsEmpty && c.Piece != null)
                {
                    cellsToDestroy.Add(c);
                }
            }
            yield return DestroyBoostedCells(cellsToDestroy);
        }

        private IEnumerator ActivateColorBomb(BoardCell cell)
        {
            if (cell.Piece == null) yield break;
            int targetType = cell.Piece.TypeIndex;

            var cellsToDestroy = new List<BoardCell>();
            for (int x = 0; x < _board.Width; x++)
            {
                for (int y = 0; y < _board.Height; y++)
                {
                    var c = _board.GetCell(x, y);
                    if (c != null && !c.IsEmpty && c.Piece != null && c.Piece.TypeIndex == targetType)
                    {
                        cellsToDestroy.Add(c);
                    }
                }
            }
            yield return DestroyBoostedCells(cellsToDestroy);
        }

        private IEnumerator ActivateDynamicBomb(BoardCell cell)
        {
            var cellsToDestroy = new List<BoardCell>();
            // Phá 3x3 area xung quanh
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    var c = _board.GetCell(cell.X + dx, cell.Y + dy);
                    if (c != null && !c.IsEmpty && c.Piece != null)
                    {
                        cellsToDestroy.Add(c);
                    }
                }
            }
            yield return DestroyBoostedCells(cellsToDestroy);
        }

        private IEnumerator DestroyBoostedCells(List<BoardCell> cells)
        {
            foreach (var cell in cells)
            {
                if (cell.Piece != null)
                {
                    cell.Piece.PlayDestroyAnimation();

                    if (_config.boosterEffectPrefab != null)
                    {
                        var fx = Object.Instantiate(_config.boosterEffectPrefab, cell.WorldPosition, Quaternion.identity);
                        Object.Destroy(fx, 1f);
                    }
                }
            }

            yield return new WaitForSeconds(_config.matchDestroyDuration);

            foreach (var cell in cells)
            {
                if (cell.Piece != null)
                {
                    // Nếu piece cũng là special, chain activate
                    if (cell.Piece.IsSpecial)
                    {
                        // Mark as non-special to avoid infinite loop
                        var piece = cell.Piece;
                        Object.Destroy(piece.gameObject);
                        cell.Piece = null;
                    }
                    else
                    {
                        Object.Destroy(cell.Piece.gameObject);
                        cell.Piece = null;
                    }
                }
            }
        }

        private bool IsHorizontalMatch(List<BoardCell> match)
        {
            if (match.Count < 2) return false;
            int y = match[0].Y;
            foreach (var cell in match)
            {
                if (cell.Y != y) return false;
            }
            return true;
        }

        private bool IsVerticalMatch(List<BoardCell> match)
        {
            if (match.Count < 2) return false;
            int x = match[0].X;
            foreach (var cell in match)
            {
                if (cell.X != x) return false;
            }
            return true;
        }
    }
}