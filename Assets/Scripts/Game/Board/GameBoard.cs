using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.Core;
using PuzzleGame.Game.Common;
using PuzzleGame.Game.Config;

namespace PuzzleGame.Game.Board
{
    /// <summary>
    /// Enum trạng thái của board.
    /// </summary>
    public enum BoardState
    {
        Idle,
        PlayerInput,
        Swapping,
        Matching,
        Collapsing,
        Filling,
        Animating,
        WaitingForAction,
        GameOver
    }

    /// <summary>
    /// Controller chính của puzzle board - quản lý grid, logic gameplay, và flow.
    /// </summary>
    public class GameBoard : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GameConfig config;
        [SerializeField] private Transform boardContainer;
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private GameObject[] blockPrefabs;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public BoardState State { get; private set; } = BoardState.Idle;

        public BoardCell[,] Cells { get; private set; }
        public GameConfig Config => config;

        public event Action<List<BoardCell>> OnMatchFound;
        public event Action<int> OnCombo;
        public event Action OnBoardStable;
        public event Action OnNoMatchesAvailable;

        private MatchFinder _matchFinder;
        private BoardCollapser _collapser;
        private BoardFiller _filler;
        private SpecialPieceHandler _specialHandler;
        private int _comboCount;

        public void Initialize(int width, int height, Level level = null)
        {
            Width = width;
            Height = height;

            _matchFinder = new MatchFinder(this, config);
            _collapser = new BoardCollapser(this, config);
            _filler = new BoardFiller(this, config);
            _specialHandler = new SpecialPieceHandler(this, config);

            CreateBoard(level);
            EnsureNoInitialMatches();
            State = BoardState.PlayerInput;
        }

        public void Clear()
        {
            if (Cells == null) return;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (Cells[x, y] != null && Cells[x, y].Piece != null)
                    {
                        Destroy(Cells[x, y].Piece.gameObject);
                    }
                }
            }
            foreach (Transform child in boardContainer)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Tạo board grid và fill initial pieces.
        /// </summary>
        private void CreateBoard(Level level)
        {
            Cells = new BoardCell[Width, Height];
            float startX = -(Width - 1) * (config.cellSize + config.cellSpacing) / 2f;
            float startY = -(Height - 1) * (config.cellSize + config.cellSpacing) / 2f;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Vector3 pos = new Vector3(
                        startX + x * (config.cellSize + config.cellSpacing),
                        startY + y * (config.cellSize + config.cellSpacing),
                        0);

                    // Tạo cell background
                    if (cellPrefab != null)
                    {
                        var cellObj = Instantiate(cellPrefab, boardContainer);
                        cellObj.transform.localPosition = pos;
                        cellObj.name = $"Cell_{x}_{y}";
                    }

                    // Tạo board cell data
                    var cell = new BoardCell(x, y, pos);
                    Cells[x, y] = cell;

                    // Xử lý level data nếu có
                    if (level != null && level.tiles.Count > 0)
                    {
                        int tileIndex = y * Width + x;
                        if (tileIndex < level.tiles.Count)
                        {
                            var tile = level.tiles[tileIndex];
                            cell.BlockerType = tile.blockerType;

                            if (tile is BlockTile blockTile)
                            {
                                if (blockTile.type == BlockType.Empty)
                                {
                                    cell.IsEmpty = true;
                                    continue;
                                }
                                if (blockTile.type != BlockType.RandomBlock)
                                {
                                    CreatePieceAt(x, y, (int)blockTile.type);
                                    continue;
                                }
                            }
                        }
                    }

                    // Random block
                    CreateRandomPieceAt(x, y);
                }
            }
        }

        /// <summary>
        /// Đảm bảo không có match nào khi board mới tạo.
        /// </summary>
        private void EnsureNoInitialMatches()
        {
            int maxAttempts = 100;
            int attempt = 0;
            while (attempt < maxAttempts)
            {
                var matches = _matchFinder.FindAllMatches();
                if (matches.Count == 0) break;

                foreach (var match in matches)
                {
                    foreach (var cell in match)
                    {
                        if (cell.Piece != null)
                        {
                            Destroy(cell.Piece.gameObject);
                            cell.Piece = null;
                        }
                        CreateRandomPieceAt(cell.X, cell.Y);
                    }
                }
                attempt++;
            }
        }

        /// <summary>
        /// Xử lý swap giữa 2 cells.
        /// </summary>
        public void TrySwap(BoardCell cell1, BoardCell cell2)
        {
            if (State != BoardState.PlayerInput) return;
            if (cell1 == null || cell2 == null) return;
            if (cell1.Piece == null || cell2.Piece == null) return;
            if (!AreAdjacent(cell1, cell2)) return;

            StartCoroutine(SwapAndCheck(cell1, cell2));
        }

        private IEnumerator SwapAndCheck(BoardCell cell1, BoardCell cell2)
        {
            State = BoardState.Swapping;

            // Swap pieces
            yield return StartCoroutine(AnimateSwap(cell1, cell2));
            SwapPieces(cell1, cell2);

            // Check for matches
            var matches = _matchFinder.FindAllMatches();
            if (matches.Count > 0)
            {
                _comboCount = 0;
                yield return StartCoroutine(ProcessMatches(matches));
            }
            else
            {
                // No match - swap back
                yield return StartCoroutine(AnimateSwap(cell1, cell2));
                SwapPieces(cell1, cell2);
            }

            if (State != BoardState.GameOver)
            {
                State = BoardState.PlayerInput;
                OnBoardStable?.Invoke();
            }
        }

        private IEnumerator ProcessMatches(List<List<BoardCell>> matches)
        {
            while (matches.Count > 0)
            {
                _comboCount++;
                State = BoardState.Matching;

                // Xử lý special pieces được tạo từ match lớn
                foreach (var match in matches)
                {
                    _specialHandler.CheckAndCreateSpecial(match);
                    OnMatchFound?.Invoke(match);
                }

                if (_comboCount > 1)
                {
                    OnCombo?.Invoke(_comboCount);
                }

                // Destroy matched pieces
                yield return StartCoroutine(DestroyMatches(matches));

                // Collapse (gravity)
                State = BoardState.Collapsing;
                yield return StartCoroutine(_collapser.CollapseBoard());

                // Fill empty spaces
                State = BoardState.Filling;
                yield return StartCoroutine(_filler.FillBoard());

                // Check for new matches (cascade)
                matches = _matchFinder.FindAllMatches();
            }

            // Check if there are still possible moves
            if (!HasPossibleMoves())
            {
                OnNoMatchesAvailable?.Invoke();
            }
        }

        private IEnumerator DestroyMatches(List<List<BoardCell>> matches)
        {
            var cellsToDestroy = new HashSet<BoardCell>();
            foreach (var match in matches)
            {
                foreach (var cell in match)
                {
                    cellsToDestroy.Add(cell);
                }
            }

            foreach (var cell in cellsToDestroy)
            {
                if (cell.Piece != null)
                {
                    // Handle blocker
                    if (cell.BlockerType != BlockerType.None)
                    {
                        cell.BlockerHp--;
                        if (cell.BlockerHp > 0) continue;
                        cell.BlockerType = BlockerType.None;
                    }

                    cell.Piece.PlayDestroyAnimation();

                    // Spawn effect
                    if (config.matchEffectPrefab != null)
                    {
                        var fx = Instantiate(config.matchEffectPrefab, cell.WorldPosition, Quaternion.identity, boardContainer);
                        Destroy(fx, 1f);
                    }
                }
            }

            yield return new WaitForSeconds(config.matchDestroyDuration);

            foreach (var cell in cellsToDestroy)
            {
                if (cell.Piece != null && cell.BlockerType == BlockerType.None)
                {
                    Destroy(cell.Piece.gameObject);
                    cell.Piece = null;
                }
            }
        }

        private IEnumerator AnimateSwap(BoardCell cell1, BoardCell cell2)
        {
            if (cell1.Piece == null || cell2.Piece == null) yield break;

            var piece1 = cell1.Piece.transform;
            var piece2 = cell2.Piece.transform;
            var pos1 = cell1.WorldPosition;
            var pos2 = cell2.WorldPosition;
            float elapsed = 0f;

            while (elapsed < config.swapDuration)
            {
                elapsed += Time.deltaTime;
                float t = config.swapCurve.Evaluate(elapsed / config.swapDuration);
                piece1.localPosition = Vector3.Lerp(pos1, pos2, t);
                piece2.localPosition = Vector3.Lerp(pos2, pos1, t);
                yield return null;
            }

            piece1.localPosition = pos2;
            piece2.localPosition = pos1;
        }

        /// <summary>
        /// Swap piece data giữa 2 cells.
        /// </summary>
        public void SwapPieces(BoardCell cell1, BoardCell cell2)
        {
            var temp = cell1.Piece;
            cell1.Piece = cell2.Piece;
            cell2.Piece = temp;

            if (cell1.Piece != null) cell1.Piece.SetCell(cell1);
            if (cell2.Piece != null) cell2.Piece.SetCell(cell2);
        }

        public bool AreAdjacent(BoardCell cell1, BoardCell cell2)
        {
            return (Mathf.Abs(cell1.X - cell2.X) + Mathf.Abs(cell1.Y - cell2.Y)) == 1;
        }

        /// <summary>
        /// Kiểm tra xem còn move hợp lệ không.
        /// </summary>
        public bool HasPossibleMoves()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (Cells[x, y].IsEmpty || Cells[x, y].Piece == null) continue;

                    // Check swap right
                    if (x < Width - 1 && !Cells[x + 1, y].IsEmpty && Cells[x + 1, y].Piece != null)
                    {
                        SwapPieces(Cells[x, y], Cells[x + 1, y]);
                        bool hasMatch = _matchFinder.FindAllMatches().Count > 0;
                        SwapPieces(Cells[x, y], Cells[x + 1, y]);
                        if (hasMatch) return true;
                    }

                    // Check swap up
                    if (y < Height - 1 && !Cells[x, y + 1].IsEmpty && Cells[x, y + 1].Piece != null)
                    {
                        SwapPieces(Cells[x, y], Cells[x, y + 1]);
                        bool hasMatch = _matchFinder.FindAllMatches().Count > 0;
                        SwapPieces(Cells[x, y], Cells[x, y + 1]);
                        if (hasMatch) return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Tạo một piece ngẫu nhiên tại vị trí (x, y).
        /// </summary>
        public BlockPiece CreateRandomPieceAt(int x, int y)
        {
            int typeIndex = UnityEngine.Random.Range(0, config.numberOfBlockTypes);
            return CreatePieceAt(x, y, typeIndex);
        }

        /// <summary>
        /// Tạo một piece cụ thể tại vị trí (x, y).
        /// </summary>
        public BlockPiece CreatePieceAt(int x, int y, int typeIndex)
        {
            if (Cells[x, y].IsEmpty) return null;
            if (blockPrefabs == null || blockPrefabs.Length == 0) return null;

            int prefabIndex = Mathf.Clamp(typeIndex, 0, blockPrefabs.Length - 1);
            var obj = Instantiate(blockPrefabs[prefabIndex], boardContainer);
            obj.transform.localPosition = Cells[x, y].WorldPosition;

            var piece = obj.GetComponent<BlockPiece>();
            if (piece == null) piece = obj.AddComponent<BlockPiece>();

            piece.Initialize(typeIndex, Cells[x, y]);
            Cells[x, y].Piece = piece;

            // Apply sprite/color from config
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (config.blockSprites != null && typeIndex < config.blockSprites.Length && config.blockSprites[typeIndex] != null)
                {
                    sr.sprite = config.blockSprites[typeIndex];
                }
                if (config.blockColors != null && typeIndex < config.blockColors.Length)
                {
                    sr.color = config.blockColors[typeIndex];
                }
            }

            return piece;
        }

        /// <summary>
        /// Tạo special piece (booster) tại vị trí (x, y).
        /// </summary>
        public BlockPiece CreateSpecialPieceAt(int x, int y, int typeIndex, BoosterType boosterType)
        {
            var piece = CreatePieceAt(x, y, typeIndex);
            if (piece != null)
            {
                piece.SetSpecial(boosterType);
            }
            return piece;
        }

        public BoardCell GetCell(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return null;
            return Cells[x, y];
        }

        public void SetState(BoardState state)
        {
            State = state;
        }

        /// <summary>
        /// Shuffle toàn bộ board khi không còn move hợp lệ.
        /// </summary>
        public void ShuffleBoard()
        {
            var pieces = new List<BlockPiece>();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (Cells[x, y].Piece != null && !Cells[x, y].IsEmpty)
                    {
                        pieces.Add(Cells[x, y].Piece);
                        Cells[x, y].Piece = null;
                    }
                }
            }

            // Fisher-Yates shuffle
            for (int i = pieces.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (pieces[i], pieces[j]) = (pieces[j], pieces[i]);
            }

            int idx = 0;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (!Cells[x, y].IsEmpty && idx < pieces.Count)
                    {
                        Cells[x, y].Piece = pieces[idx];
                        pieces[idx].SetCell(Cells[x, y]);
                        pieces[idx].transform.localPosition = Cells[x, y].WorldPosition;
                        idx++;
                    }
                }
            }

            // Ensure no matches after shuffle
            EnsureNoInitialMatches();

            if (!HasPossibleMoves())
            {
                ShuffleBoard(); // Recursive until valid
            }
        }
    }
}