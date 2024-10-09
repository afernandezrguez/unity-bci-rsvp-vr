using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System;

public class ProcessMainMenu : MonoBehaviour
{
    [SerializeField] private Button[] signalButtons;
    [SerializeField] private Button signal1Button, signal2Button;
    [SerializeField] private Button participantButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button setConfigButton;
    [SerializeField] private GameObject participantNumber;
    [SerializeField] private Toggle feedbackToggle, testingToggle;
    public Toggle condition1Toggle, condition2Toggle;
    public VirtualKeyboard virtualKeyboard;
    public SequenceController sequenceController;
    public Boolean feedbackMode, testingMode;
    private string participantCode, numberOfSequences;
    private string conditionSelected;

    private void Start()
    {
        CloseBCI2000();
        CloseAllCmdWindows();
        condition1Toggle.onValueChanged.AddListener(delegate { OnToggleChanged(condition1Toggle); });
        condition2Toggle.onValueChanged.AddListener(delegate { OnToggleChanged(condition2Toggle); });
        signal1Button.onClick.AddListener(ActionForSignal1);
        signal2Button.onClick.AddListener(ActionForSignal2);
        participantButton.onClick.AddListener(ParticipantPannel);
        setConfigButton.onClick.AddListener(CreateProcessSetConfig);
        quitButton.onClick.AddListener(ExitApplication);
    }

    void OnToggleChanged(Toggle changedToggle)
    {
        if (condition1Toggle.isOn)
        {
            conditionSelected = "1";
        }
        else if (condition2Toggle.isOn)
        {
            conditionSelected = "2";
        }
    }

    private void Update()
    {
        //// Captura del movimiento del ratón
        //float mouseX = Input.GetAxis("Mouse X");
        //float mouseY = Input.GetAxis("Mouse Y");

        //// Usa los movimientos del ratón para rotar el objeto (o la cámara)
        //transform.Rotate(mouseY, mouseX, 0);

        //// Detectar clic del ratón
        //if (Input.GetMouseButtonDown(0)) // Botón izquierdo
        //{
        //    Debug.Log("Botón izquierdo del ratón presionado");
        //}
    }

    private void ParticipantPannel()
    {
        participantNumber.GetComponent<Text>().text = ""; // CUIDADO que siguen estando los dos dígitos, creo.
        virtualKeyboard.participantNumber = "";
    }

    void ActionForSignal1()
    {
        CloseBCI2000();
        CloseAllCmdWindows();
        string workingDirectory = "C:/BCI2000_v3_6/batch/rsvp_vr";
        string command = $"/C start signalGenerator_rsvp_vr.bat";
        ExecuteCommand(workingDirectory, command);
        testingMode = true;
    }

    void ActionForSignal2()
    {
        CloseBCI2000();
        CloseAllCmdWindows();
        string workingDirectory = "C:/BCI2000_v3_6/batch/rsvp_vr";
        string command = $"/C start actichamp_rsvp_vr.bat";
        ExecuteCommand(workingDirectory, command);
        testingMode = false;
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

        Debug.Log("The current number of sequences is " + numberOfSequences);

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