using UnityEngine;
using PuzzleGame.Game.Common;

namespace PuzzleGame.Game.Board
{
    /// <summary>
    /// Component gắn trên mỗi block piece trên board.
    /// </summary>
    public class BlockPiece : MonoBehaviour
    {
        public int TypeIndex { get; private set; }
        public BoardCell Cell { get; private set; }
        public bool IsSpecial { get; private set; }
        public BoosterType BoosterType { get; private set; }

        private SpriteRenderer _spriteRenderer;
        private Animator _animator;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
        }

        public void Initialize(int typeIndex, BoardCell cell)
        {
            TypeIndex = typeIndex;
            Cell = cell;
            IsSpecial = false;
            gameObject.name = $"Block_{typeIndex}_{cell.X}_{cell.Y}";
        }

        public void SetCell(BoardCell cell)
        {
            Cell = cell;
        }

        public void SetSpecial(BoosterType boosterType)
        {
            IsSpecial = true;
            BoosterType = boosterType;
        }

        public void PlayDestroyAnimation()
        {
            if (_animator != null)
            {
                _animator.SetTrigger("Destroy");
            }
            else
            {
                // Fallback: simple scale animation
                StartCoroutine(ScaleDown());
            }
        }

        public void PlaySelectAnimation()
        {
            if (_animator != null)
            {
                _animator.SetTrigger("Select");
            }
            else
            {
                transform.localScale = Vector3.one * 1.1f;
            }
        }

        public void PlayDeselectAnimation()
        {
            if (_animator != null)
            {
                _animator.SetTrigger("Deselect");
            }
            else
            {
                transform.localScale = Vector3.one;
            }
        }

        private System.Collections.IEnumerator ScaleDown()
        {
            float duration = 0.15f;
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                yield return null;
            }
            transform.localScale = Vector3.zero;
        }
    }
}