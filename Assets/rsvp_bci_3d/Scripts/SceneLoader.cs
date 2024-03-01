using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string pannelToLoad;
    [SerializeField] private Button goMainMenuButton;

    void Start()
    {
        if (goMainMenuButton != null)
        {
            goMainMenuButton.onClick.AddListener(LoadMainMenu);
        }
    }

    public void LoadMainMenu()
    {
        if (!string.IsNullOrEmpty(pannelToLoad))
        {
            SceneManager.LoadScene(pannelToLoad);
        }
        if (pannelToLoad == "MainMenu")
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
