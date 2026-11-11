using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButton : MonoBehaviour
{
    public void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }
    public string SceneName;
    public void OpenScene()
    {
        SceneManager.LoadScene(SceneName);
    }
}
