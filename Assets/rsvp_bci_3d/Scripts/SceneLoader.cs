using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private Button loadSceneButton;

    void Start()
    {
        if (loadSceneButton != null)
        {
            loadSceneButton.onClick.AddListener(LoadScene);
        }
    }

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        if (sceneToLoad == "MainMenu")
        {
            CreateProcess("BCI2000Command Quit");
        }
    }

    private void CreateProcess(string command)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/C {command}",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Process.Start(processInfo);
    }
}
