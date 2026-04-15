using UnityEngine;

namespace PuzzleGame.Game.Config
{
    /// <summary>
    /// ScriptableObject chứa các cấu hình chung cho game puzzle match.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "PuzzleGame/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Board Settings")]
        public int defaultBoardWidth = 7;
        public int defaultBoardHeight = 9;
        public float cellSize = 1.0f;
        public float cellSpacing = 0.1f;

        [Header("Block Settings")]
        public int numberOfBlockTypes = 5;
        public Sprite[] blockSprites;
        public Color[] blockColors;

        [Header("Animation")]
        public float swapDuration = 0.2f;
        public float collapseDuration = 0.15f;
        public float fillDuration = 0.2f;
        public float matchDestroyDuration = 0.15f;
        public float cascadeDelay = 0.1f;
        public AnimationCurve swapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public AnimationCurve collapseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Match Settings")]
        public int minMatchCount = 3;
        public int matchScoreBase = 10;
        public int comboScoreMultiplier = 5;

        [Header("Booster Settings")]
        public int horizontalBombMatchCount = 4;
        public int verticalBombMatchCount = 4;
        public int colorBombMatchCount = 5;

        [Header("Effects")]
        public GameObject matchEffectPrefab;
        public GameObject boosterEffectPrefab;
        public GameObject comboTextPrefab;

        [Header("Audio")]
        public AudioClip swapSound;
        public AudioClip matchSound;
        public AudioClip comboSound;
        public AudioClip boosterSound;
        public AudioClip winSound;
        public AudioClip loseSound;
    }
}