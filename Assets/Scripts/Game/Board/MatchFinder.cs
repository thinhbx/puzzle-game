using System.Collections.Generic;
using PuzzleGame.Game.Config;

namespace PuzzleGame.Game.Board
{
    /// <summary>
    /// Tìm tất cả các matches trên board (horizontal + vertical, >= minMatchCount).
    /// </summary>
    public class MatchFinder
    {
        private readonly GameBoard _board;
        private readonly GameConfig _config;

        public MatchFinder(GameBoard board, GameConfig config)
        {
            _board = board;
            _config = config;
        }

        /// <summary>
        /// Tìm tất cả matches hiện tại trên board.
        /// </summary>
        public List<List<BoardCell>> FindAllMatches()
        {
            var allMatches = new List<List<BoardCell>>();
            var matched = new HashSet<BoardCell>();

            // Horizontal matches
            for (int y = 0; y < _board.Height; y++)
            {
                for (int x = 0; x < _board.Width - 2; x++)
                {
                    var match = GetHorizontalMatch(x, y);
                    if (match != null && match.Count >= _config.minMatchCount)
                    {
                        bool isNew = false;
                        foreach (var cell in match)
                        {
                            if (!matched.Contains(cell))
                            {
                                isNew = true;
                                matched.Add(cell);
                            }
                        }
                        if (isNew) allMatches.Add(match);
                    }
                }
            }

            // Vertical matches
            for (int x = 0; x < _board.Width; x++)
            {
                for (int y = 0; y < _board.Height - 2; y++)
                {
                    var match = GetVerticalMatch(x, y);
                    if (match != null && match.Count >= _config.minMatchCount)
                    {
                        bool isNew = false;
                        foreach (var cell in match)
                        {
                            if (!matched.Contains(cell))
                            {
                                isNew = true;
                                matched.Add(cell);
                            }
                        }
                        if (isNew) allMatches.Add(match);
                    }
                }
            }

            return allMatches;
        }

        /// <summary>
        /// Tìm match theo hàng ngang bắt đầu từ (startX, y).
        /// </summary>
        private List<BoardCell> GetHorizontalMatch(int startX, int y)
        {
            var cell = _board.GetCell(startX, y);
            if (cell == null || cell.IsEmpty || cell.Piece == null) return null;

            int typeIndex = cell.Piece.TypeIndex;
            var match = new List<BoardCell> { cell };

            for (int x = startX + 1; x < _board.Width; x++)
            {
                var next = _board.GetCell(x, y);
                if (next == null || next.IsEmpty || next.Piece == null || next.Piece.TypeIndex != typeIndex)
                    break;
                match.Add(next);
            }

            return match.Count >= _config.minMatchCount ? match : null;
        }

        /// <summary>
        /// Tìm match theo cột dọc bắt đầu từ (x, startY).
        /// </summary>
        private List<BoardCell> GetVerticalMatch(int x, int startY)
        {
            var cell = _board.GetCell(x, startY);
            if (cell == null || cell.IsEmpty || cell.Piece == null) return null;

            int typeIndex = cell.Piece.TypeIndex;
            var match = new List<BoardCell> { cell };

            for (int y = startY + 1; y < _board.Height; y++)
            {
                var next = _board.GetCell(x, y);
                if (next == null || next.IsEmpty || next.Piece == null || next.Piece.TypeIndex != typeIndex)
                    break;
                match.Add(next);
            }

            return match.Count >= _config.minMatchCount ? match : null;
        }

        /// <summary>
        /// Tìm matches tại một cell cụ thể (sau khi swap).
        /// </summary>
        public List<BoardCell> FindMatchesAt(int x, int y)
        {
            var result = new List<BoardCell>();
            var cell = _board.GetCell(x, y);
            if (cell == null || cell.IsEmpty || cell.Piece == null) return result;

            int typeIndex = cell.Piece.TypeIndex;

            // Horizontal
            var hMatch = new List<BoardCell> { cell };
            // Left
            for (int i = x - 1; i >= 0; i--)
            {
                var c = _board.GetCell(i, y);
                if (c == null || c.IsEmpty || c.Piece == null || c.Piece.TypeIndex != typeIndex) break;
                hMatch.Add(c);
            }
            // Right
            for (int i = x + 1; i < _board.Width; i++)
            {
                var c = _board.GetCell(i, y);
                if (c == null || c.IsEmpty || c.Piece == null || c.Piece.TypeIndex != typeIndex) break;
                hMatch.Add(c);
            }
            if (hMatch.Count >= _config.minMatchCount)
                result.AddRange(hMatch);

            // Vertical
            var vMatch = new List<BoardCell> { cell };
            // Down
            for (int j = y - 1; j >= 0; j--)
            {
                var c = _board.GetCell(x, j);
                if (c == null || c.IsEmpty || c.Piece == null || c.Piece.TypeIndex != typeIndex) break;
                vMatch.Add(c);
            }
            // Up
            for (int j = y + 1; j < _board.Height; j++)
            {
                var c = _board.GetCell(x, j);
                if (c == null || c.IsEmpty || c.Piece == null || c.Piece.TypeIndex != typeIndex) break;
                vMatch.Add(c);
            }
            if (vMatch.Count >= _config.minMatchCount)
                result.AddRange(vMatch);

            return result;
        }
    }
}