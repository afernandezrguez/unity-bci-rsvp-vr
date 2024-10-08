using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System;

public class ProcessMainMenu : MonoBehaviour
{
    [SerializeField] private Button[] conditionButtons;
    [SerializeField] private Button participantButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button setConfigButton;
    [SerializeField] private GameObject participantNumber;
    [SerializeField] private Toggle feedbackToggle, testingToggle;
    public VirtualKeyboard virtualKeyboard;
    public SequenceController sequenceController;
    public Boolean feedbackMode, testingMode;
    private string participantCode, numberOfSequences;
    private string conditionSelected;

    private void Start()
    {
        CloseBCI2000();
        CloseAllCmdWindows();
        foreach (Button button in conditionButtons)
        {
            button.onClick.AddListener(() => CreateProcessCondition(button));
        }
        participantButton.onClick.AddListener(ParticipantPannel);
        setConfigButton.onClick.AddListener(CreateProcessSetConfig);
        quitButton.onClick.AddListener(ExitApplication);
    }

    private void Update()
    {

    }

    private void ParticipantPannel()
    {
        participantNumber.GetComponent<Text>().text = ""; // CUIDADO que siguen estando los dos dígitos, creo.
        virtualKeyboard.participantNumber = "";
    }

    private void CreateProcessCondition(Button button)
    {
        CloseBCI2000();
        CloseAllCmdWindows();
        conditionSelected = button.name.Replace("Condition ", "");
        string workingDirectory = "C:/BCI2000_v3_6/batch/rsvp_vr";
        string command;
        if (testingToggle.GetComponent<Toggle>().isOn)
        {
            command = $"/C start signalGenerator_rsvp_vr.bat";
            testingMode = true;
        }
        else
        {
            command = $"/C start actichamp_rsvp_vr.bat";
            testingMode = false;
        }
        ExecuteCommand(workingDirectory, command);
    }

    private void CreateProcessSetConfig()
    {
        if (testingMode)
        {
            participantCode = "RV_Test";
        }
        else
        {
            participantCode = "RV" + virtualKeyboard.participantNumber;
        }
        numberOfSequences = sequenceController.currentNumber.ToString();
        string workingDirectory = "C:/BCI2000_v3_6/prog";
        string command;
        if (feedbackToggle.GetComponent<Toggle>().isOn)
        {
            command = $"/C BCI2000Command SetParameter SubjectName {participantCode} && BCI2000Command SetParameter SubjectSession {conditionSelected} && BCI2000Command SetParameter NumberOfSequences {numberOfSequences} && BCI2000Command SetParameter DisplayResults 1 && BCI2000Command SetConfig";
            feedbackMode = true;
        }
        else
        {
            command = $"/C BCI2000Command SetParameter SubjectName {participantCode} && BCI2000Command SetParameter SubjectSession {conditionSelected} && BCI2000Command SetParameter NumberOfSequences {numberOfSequences} && BCI2000Command SetConfig";
            feedbackMode = false;
        }
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