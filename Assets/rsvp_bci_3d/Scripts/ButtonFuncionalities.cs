//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Net.Sockets;
//using Unity.VisualScripting;
//using UnityEditor.Experimental.GraphView;
//using UnityEngine;
//using UnityEngine.UI;

//public class ButtonFuncionalities : MonoBehaviour
//{

//    [SerializeField] private Button[] conditionButtons;
//    [SerializeField] private Button setConfigButton;
//    [SerializeField] private Button quitButton;
//    [SerializeField] private Button startButton;
//    // AÑADIR UN BOTÓN DE STOP // [SerializeField] private Button stopButton;
//    [SerializeField] private Button returnButton;
//    [SerializeField] private GameObject participantCodeInput;
//    [SerializeField] private GameObject RunMenuObjects;
//    private string participantCode;


//    private UdpClient udpClient;
//    //private int stimulusNumberInt, phaseInSequenceInt;
//    //private Boolean stimulusPresented, showNextTarget, blockCompleted, allowFinishing;
//    Boolean blockCompleted;

//    //private GameObject[] stimuliArray;
//    //public GameObject StartButton, ReturnButton, BlockCompleted;
//    public GameObject BlockCompleted;

//    //public GameObject Canvas_bci_run, Canvas_bci_participant;
//    //private AudioSource playerAudio;

//    private readonly int port = 12345;
//    //private readonly int[] stimulusTargetOrder = { 1, 2 };      // El "ToBeCopied" de BCI2000.
//    //private readonly int numberOfCommands = 10;

//    //private int trial = 0;
//    //private bool resetTrial = true;


//    // Start is called before the first frame update
//    void Start()
//    {
//        setConfigButton.onClick.AddListener(CreateProcessSetConfig);
//        quitButton.onClick.AddListener(ExitApplication);

//        startButton.onClick.AddListener(CleanScreen);
//        returnButton.onClick.AddListener(OnDestroy);
//        setConfigButton.onClick.AddListener(RunMenu);
//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }


//    private void CreateProcessCondition(Button button)
//    {
//        CloseBCI2000();
//        CloseAllCmdWindows();
//        string condition = button.name.Replace("Condition ", "");
//        string workingDirectory = "C:/BCI2000_v3_6/batch/rsvp_unity";
//        string command = $"/C start signalGenerator_c{condition}.bat";
//        ExecuteCommand(workingDirectory, command);
//    }

//    private void CreateProcessSetConfig()
//    {
//        UpdateParticipantCode();
//        SaveParticipantCode();
//        string workingDirectory = "C:/BCI2000_v3_6/prog";
//        string command = $"/C BCI2000Command SetParameter SubjectName {participantCode} && BCI2000Command SetConfig";
//        ExecuteCommand(workingDirectory, command);
//    }

//    private void ExecuteCommand(string workingDirectory, string command)
//    {
//        ProcessStartInfo processInfo = new ProcessStartInfo
//        {
//            WorkingDirectory = workingDirectory,
//            WindowStyle = ProcessWindowStyle.Hidden,
//            FileName = "cmd.exe",
//            UseShellExecute = false,
//            CreateNoWindow = true,
//            RedirectStandardOutput = false,
//            Arguments = command
//        };

//        Process process = Process.Start(processInfo);
//        process.WaitForExit();
//    }

//    public void UpdateParticipantCode()
//    {
//        participantCode = participantCodeInput.GetComponent<InputField>().text;
//    }

//    public void SaveParticipantCode()
//    {
//        PlayerPrefs.SetString("ParticipantCode", participantCodeInput.GetComponent<InputField>().text);
//        PlayerPrefs.Save();
//    }




//    private void RunMenu()
//    {
//        startButton.onClick.AddListener(CreateProcessStart);
//        returnButton.onClick.AddListener(CreateProcessReturn);
//        //stimuliArray = new GameObject[numberOfCommands];
//        udpClient = new UdpClient(port); // esto tienes que cerrarlo, para que no se cree un puerto cada vez que entre aquí.
//        //udpClient.BeginReceive(ReceiveCallback, null); // creo que esto debería llevármelo a otro script individual.
//        BlockCompleted.SetActive(false);
//        blockCompleted = false;
//        //InitializeStimuliArray();
//        //playerAudio = GetComponent<AudioSource>();
//    }

//    private void CreateProcessStart()
//    {
//        string workingDirectory = "C:\\BCI2000_v3_6\\prog";
//        string command = "/C BCI2000Command Start";
//        CreateProcess(workingDirectory, command);
//    }

//    private void CreateProcessReturn()
//    {
//        string workingDirectory = "C:\\BCI2000_v3_6\\prog";
//        string command = "/C BCI2000Command Quit";
//        CreateProcess(workingDirectory, command);
//        CloseAllCmdWindows();
//    }

//    private void CreateProcess(string workingDirectory, string command)
//    {
//        var processInfo = new ProcessStartInfo
//        {
//            WorkingDirectory = workingDirectory,
//            WindowStyle = ProcessWindowStyle.Hidden,
//            FileName = "cmd.exe",
//            UseShellExecute = false,
//            CreateNoWindow = true,
//            RedirectStandardOutput = false,
//            Arguments = command
//        };
//        var process = Process.Start(processInfo);
//        process.WaitForExit();
//    }




//    void CleanScreen()
//    {
//        blockCompleted = false;
//        RunMenuObjects.SetActive(false);
//    }

//    void OnDestroy()
//    {
//        if (udpClient != null)
//        {
//            udpClient.Close();
//        }
//    }

//    private void CloseBCI2000()
//    {
//        string workingDirectory = "C:/BCI2000_v3_6/prog";
//        string command = "/C BCI2000Command Quit";
//        ExecuteCommand(workingDirectory, command);
//    }

//    private void CloseAllCmdWindows()
//    {
//        Process[] processes = Process.GetProcessesByName("cmd");
//        foreach (Process process in processes)
//        {
//            process.Kill();
//        }
//    }

//    private void ExitApplication()
//    {
//        Application.Quit();
//    }

//}
