using PuzzleGame.Core;
using UnityEngine;

public class SceneTransition : MonoBehaviour
{
   [SerializeField] private string sceneName;
   [SerializeField] private float fadeDuration = 1f;
   [SerializeField] private Color fadeColor = Color.black;

   public void ChangeScene()
   {
       Transition.LoadLevel(sceneName, fadeDuration, fadeColor);
   }
}
