using UnityEngine;

namespace Project_Files.Scripts.UI
{
  public class SceneLoader : MonoBehaviour
  {
    [SerializeField] private GameSceneManager _sceneManager;
    
    public void LoadNextScene() {
      _sceneManager.LoadNextScene();
    }
  }
}