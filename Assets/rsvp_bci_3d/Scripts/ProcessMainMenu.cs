using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class ProcessMainMenu : MonoBehaviour
{
    [SerializeField] private Button[] conditionButtons;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button setConfigButton;
    [SerializeField] private GameObject participantCodeInput;

    private string participantCode;

    private void Start()
    {
        CloseBCI2000();
        CloseAllCmdWindows();

        participantCodeInput.GetComponent<InputField>().text = PlayerPrefs.GetString("ParticipantCode");

        foreach (Button button in conditionButtons)
        {
            button.onClick.AddListener(() => CreateProcessCondition(button));
        }

        setConfigButton.onClick.AddListener(CreateProcessSetConfig);
        quitButton.onClick.AddListener(ExitApplication);
    }

    private void Update()
    {

    }

    private void CreateProcessCondition(Button button)
    {
        CloseBCI2000();
        CloseAllCmdWindows();
        string condition = button.name.Replace("Condition ", "");
        string workingDirectory = "C:/BCI2000_v3_6/batch/rsvp_unity";
        string command = $"/C start signalGenerator_c{condition}.bat";
        ExecuteCommand(workingDirectory, command);
    }

    private void CreateProcessSetConfig()
    {
        UpdateParticipantCode();
        SaveParticipantCode();
        string workingDirectory = "C:/BCI2000_v3_6/prog";
        string command = $"/C BCI2000Command SetParameter SubjectName {participantCode} && BCI2000Command SetConfig";
        ExecuteCommand(workingDirectory, command);
    }

    private void ExecuteCommand(string workingDirectory, string command)
    {
        ProcessStartInfo processInfo = new ProcessStartInfo
        {
            WorkingDirectory = workingDirectory,
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "cmd.exe",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = false,
            Arguments = command
        };

        Process process = Process.Start(processInfo);
        process.WaitForExit();
    }

    public void UpdateParticipantCode()
    {
        participantCode = participantCodeInput.GetComponent<InputField>().text;
    }

    public void SaveParticipantCode()
    {
        PlayerPrefs.SetString("ParticipantCode", participantCodeInput.GetComponent<InputField>().text);
        PlayerPrefs.Save();
    }

    private void CloseBCI2000()
    {
        string workingDirectory = "C:/BCI2000_v3_6/prog";
        string command = "/C BCI2000Command Quit";
        ExecuteCommand(workingDirectory, command);
    }

    private void CloseAllCmdWindows()
    {
        Process[] processes = Process.GetProcessesByName("cmd");
        foreach (Process process in processes)
        {
            process.Kill();
        }
    }

    private void ExitApplication()
    {
        Application.Quit();
    }
}