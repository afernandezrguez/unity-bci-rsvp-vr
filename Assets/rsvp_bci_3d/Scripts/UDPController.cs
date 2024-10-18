using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class UDPController : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private Button returnButton;
    [SerializeField] private Button setConfigButton;
    private UdpClient udpClient;
    private int stimulusNumberInt, phaseInSequenceInt, selectedStimulusInt;
    private Boolean runStart, trialRun, stimulusPresented, allowNextTarget, showNextTarget, selectedStimulusPresented, blockCompleted, allowFinishing;
    private Boolean feedbackModeUDP;
    public ProcessMainMenu feedbackMode;
    private GameObject[] stimuliArray;

    private GameObject[] stimuliRememberArray;

    public GameObject BackgroundRun, StartButton, StopButton, ReturnButton, BlockCompleted;
    public GameObject HappyFace, SadFace;
    public GameObject Canvas_bci_run, Canvas_bci_participant;
    public GameObject FocusOnText, SelectedStimulusText;
    public GameObject leftHandController, rightHandController;
    public AudioClip focusOnSound;
    private AudioSource audioSource;

    public LineRenderer lineRenderer;       // Referencia al rayo rojo (LineRenderer)
    public MeshRenderer controllerMesh;     // Referencia al modelo del controlador (MeshRenderer)

    private readonly int port = 12345;
    private readonly int[] stimulusTargetOrder = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };      // El "ToBeCopied" de BCI2000.
    private readonly int numberOfCommands = 10;
    private int trial = 0;
    private bool resetTrial = true;

    void Start()
    {
        OnDestroy();
        Canvas_bci_run.SetActive(false);
        Canvas_bci_participant.SetActive(false);
        FocusOnText.SetActive(false);
        SelectedStimulusText.SetActive(false);
        HappyFace.SetActive(false);
        SadFace.SetActive(false);
        stimuliArray = new GameObject[numberOfCommands];
        InitializeStimuliArray();
        
        stimuliRememberArray = new GameObject[numberOfCommands];
        InitializeStimuliRememberArray();

        audioSource = GetComponent<AudioSource>();
        audioSource.clip = focusOnSound;

        startButton.onClick.AddListener(StartRun);
        stopButton.onClick.AddListener(StopRun);
        returnButton.onClick.AddListener(ReturnMainMenu);
        setConfigButton.onClick.AddListener(OpenRunMenu);
    }

    void Update()
    {
        if (trialRun)
        {
            int targetStimulusR = stimulusTargetOrder[trial];
            if (targetStimulusR >= 1 && targetStimulusR <= numberOfCommands)
            {
                stimuliRememberArray[targetStimulusR - 1].SetActive(true);
            }
        }
        else
        {
            foreach(GameObject stimulusR in stimuliRememberArray)
            {
                stimulusR.SetActive(false);
            }
        }

        if (stimulusPresented)
        {
            switch (stimulusNumberInt)
            {
                case int n when n >= 1 && n <= numberOfCommands:
                    stimuliArray[stimulusNumberInt - 1].SetActive(true);
                    break;
            }
        }
        else
        {
            DeactivateStimulus();
        }

        if (showNextTarget)
        {
            int targetStimulus = stimulusTargetOrder[trial];
            if (targetStimulus >= 1 && targetStimulus <= numberOfCommands)
            {
                stimuliArray[targetStimulus - 1].SetActive(true);
                FocusOnText.SetActive(true);
                if (resetTrial)
                {
                    audioSource.PlayOneShot(focusOnSound, 1.0f);
                    resetTrial = false;
                }
            }
            Invoke(nameof(DeactivateStimulusTarget), 1.0f);
        }

        if (selectedStimulusPresented)
        {
            stimuliArray[selectedStimulusInt - 1].SetActive(true);

            if (selectedStimulusInt == stimulusTargetOrder[trial - 1])
            {
                HappyFace.SetActive(true);
            }
            else
            {
                SadFace.SetActive(true);
            }

            SelectedStimulusText.SetActive(true);
            Invoke(nameof(DeactivateSelectedStimulus), 1.0f);
        }

        if (blockCompleted)
        {
            Canvas_bci_run.SetActive(true);
            StopButton.SetActive(false);
            BackgroundRun.SetActive(true);
            StartButton.SetActive(true);
            BlockCompleted.SetActive(true);
            ReturnButton.SetActive(true);
            stimulusPresented = false;
            resetTrial = true;
        }
    }

    private void OpenRunMenu()
    {
        SetupUDPClient();
        StopButton.SetActive(false);
        BlockCompleted.SetActive(false);
        blockCompleted = false;
    }

    private void SetupUDPClient()
    {
        udpClient = new UdpClient(port);
        udpClient.BeginReceive(ReceiveCallback, null);
    }

    private void StartRun()
    {
        blockCompleted = false;
        BackgroundRun.SetActive(false);
        StartButton.SetActive(false);
        ReturnButton.SetActive(false);
        BlockCompleted.SetActive(false);
        StopButton.SetActive(true);
        feedbackModeUDP = GetComponent<ProcessMainMenu>().feedbackMode;

        string workingDirectory = "C:\\BCI2000_v3_6\\prog";
        string command = "/C BCI2000Command Start";
        CreateProcess(workingDirectory, command);
    }

    private void StopRun()
    {
        trialRun = false;
        blockCompleted = true;
        trial = 0;
        allowFinishing = false;

        string workingDirectory = "C:\\BCI2000_v3_6\\prog";
        string command = "/C BCI2000Command Stop";
        CreateProcess(workingDirectory, command);
    }

    private void ReturnMainMenu()
    {
        udpClient?.Close();

        string workingDirectory = "C:\\BCI2000_v3_6\\prog";
        string command = "/C BCI2000Command Quit";
        CreateProcess(workingDirectory, command);
        CloseAllCmdWindows();
    }

    private void CreateProcess(string workingDirectory, string command)
    {
        var processInfo = new ProcessStartInfo
        {
            WorkingDirectory = workingDirectory,
            FileName = "cmd.exe",
            Arguments = command,
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = false,
            //RedirectStandardOutput = false,
            CreateNoWindow = true
        };
        var process = Process.Start(processInfo);
        process?.WaitForExit();
    }

    private void CloseAllCmdWindows()
    {
        Process[] processes = Process.GetProcessesByName("cmd");
        foreach (Process process in processes)
        {
            process.Kill();
        }
    }

    private void InitializeStimuliArray()                       // Assigns GameObjects to the array elements
    {
        for (int i = 0; i < numberOfCommands; i++)
        {
            string stimulusName = "Stimulus" + (i + 1);         // Generates the name of the GameObject
            stimuliArray[i] = GameObject.Find(stimulusName);    // Finds the GameObject by its name and assigns it to the array
        }
    }

    private void InitializeStimuliRememberArray()                       // Assigns GameObjects to the array elements
    {
        for (int i = 0; i < numberOfCommands; i++)
        {
            string stimulusName = "StimulusR" + (i + 1);         // Generates the name of the GameObject
            stimuliRememberArray[i] = GameObject.Find(stimulusName);    // Finds the GameObject by its name and assigns it to the array
            stimuliRememberArray[i].SetActive(false);
            //Debug.Log("El estímulo remember es: " + stimuliRememberArray[i]);
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
        byte[] bytes = udpClient.EndReceive(ar, ref ip);
        string message = Encoding.ASCII.GetString(bytes);
        ParseUDPMessage(message);
        udpClient.BeginReceive(ReceiveCallback, null);
    }

    private void ParseUDPMessage(string message)
    {
        var messageArray = message.Split('\t');
        stimulusNumberInt = int.Parse(messageArray[1]);
        phaseInSequenceInt = int.Parse(messageArray[3]);
        selectedStimulusInt = int.Parse(messageArray[4]);

        stimulusPresented = stimulusNumberInt != 0;
        HandlePhaseSequence();
    }

    private void HandlePhaseSequence()
    {
        switch (phaseInSequenceInt)
        {
            case 1:
                runStart = true;
                trialRun = true;
                allowNextTarget = true;
                showNextTarget = true;
                allowFinishing = true;
                break;
            case 2:
                showNextTarget = false;
                break;
            case 3:
                if (allowNextTarget)
                {
                    trial++;
                    allowNextTarget = false;
                }
                resetTrial = true;
                trialRun = false;
                break;
            case 0 when allowFinishing:
                blockCompleted = true;
                trial = 0;
                runStart = false;
                break;
        }

        //Debug.Log($"El valor de la phase es: " + phaseInSequenceInt);

        if (feedbackModeUDP && selectedStimulusInt != 0)
        {
            selectedStimulusPresented = true;
        }
        else
        {
            selectedStimulusPresented = false;
        }
    }

    void DeactivateStimulusTarget()
    {
        showNextTarget = false;
        FocusOnText.SetActive(false);
    }

    void DeactivateStimulus()
    {
        foreach (GameObject stimulus in stimuliArray)
        {
            stimulus.SetActive(false);
        }
        HappyFace.SetActive(false);
        SadFace.SetActive(false);
    }

    void DeactivateSelectedStimulus()
    {
        selectedStimulusPresented = false;
        SelectedStimulusText.SetActive(false);
    }

    void OnDestroy()
    {
        udpClient?.Close();
    }

    void OnApplicationQuit()
    {
        udpClient?.Close();
    }
}