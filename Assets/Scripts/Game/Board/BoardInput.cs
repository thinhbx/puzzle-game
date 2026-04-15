using UnityEngine;

namespace PuzzleGame.Game.Board
{
    /// <summary>
    /// Xử lý input của player trên board - tap/drag để swap pieces.
    /// </summary>
    public class BoardInput : MonoBehaviour
    {
        [SerializeField] private GameBoard board;
        [SerializeField] private Camera boardCamera;
        [SerializeField] private float swipeThreshold = 0.3f;

        private BoardCell _selectedCell;
        private Vector3 _touchStart;
        private bool _isDragging;

        private void Update()
        {
            if (board == null || board.State != BoardState.PlayerInput) return;

            HandleInput();
        }

        private void HandleInput()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouseInput();
#else
            HandleTouchInput();
#endif
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnTouchBegan(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0) && _isDragging)
            {
                OnTouchMoved(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                OnTouchEnded();
            }
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount == 0) return;

            var touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnTouchBegan(touch.position);
                    break;
                case TouchPhase.Moved:
                    OnTouchMoved(touch.position);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    OnTouchEnded();
                    break;
            }
        }

        private void OnTouchBegan(Vector3 screenPos)
        {
            var cam = boardCamera != null ? boardCamera : Camera.main;
            if (cam == null) return;

            Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;

            _selectedCell = GetCellAtPosition(worldPos);
            _touchStart = screenPos;
            _isDragging = _selectedCell != null;

            if (_selectedCell?.Piece != null)
            {
                _selectedCell.Piece.PlaySelectAnimation();
            }
        }

        private void OnTouchMoved(Vector3 screenPos)
        {
            if (_selectedCell == null) return;

            Vector3 delta = screenPos - _touchStart;
            if (delta.magnitude < swipeThreshold * Screen.dpi / 160f * 50f) return;

            // Determine swipe direction
            BoardCell targetCell = null;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                // Horizontal swipe
                int dx = delta.x > 0 ? 1 : -1;
                targetCell = board.GetCell(_selectedCell.X + dx, _selectedCell.Y);
            }
            else
            {
                // Vertical swipe
                int dy = delta.y > 0 ? 1 : -1;
                targetCell = board.GetCell(_selectedCell.X, _selectedCell.Y + dy);
            }

            if (targetCell != null && !targetCell.IsEmpty)
            {
                if (_selectedCell.Piece != null)
                    _selectedCell.Piece.PlayDeselectAnimation();

                board.TrySwap(_selectedCell, targetCell);
                _selectedCell = null;
                _isDragging = false;
            }
        }

        private void OnTouchEnded()
        {
            if (_selectedCell?.Piece != null)
            {
                _selectedCell.Piece.PlayDeselectAnimation();
            }
            _selectedCell = null;
            _isDragging = false;
        }

        private BoardCell GetCellAtPosition(Vector3 worldPos)
        {
            // Chuyển world position sang board local position
            Vector3 localPos = board.transform.InverseTransformPoint(worldPos);

            float halfSize = (board.Config.cellSize + board.Config.cellSpacing) / 2f;

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    var cell = board.GetCell(x, y);
                    if (cell == null || cell.IsEmpty) continue;

                    if (Mathf.Abs(localPos.x - cell.WorldPosition.x) < halfSize &&
                        Mathf.Abs(localPos.y - cell.WorldPosition.y) < halfSize)
                    {
                        return cell;
                    }
                }
            }
            return null;
        }
    }
}