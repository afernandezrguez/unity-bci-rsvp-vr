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
    private Boolean runStart, stimulusPresented, allowNextTarget, showNextTarget, selectedStimulusPresented, blockCompleted, allowFinishing;
    private Boolean feedbackModeUDP;
    public ProcessMainMenu feedbackMode;
    private GameObject[] stimuliArray;
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
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = focusOnSound;
    }

    void Update()
    {
        startButton.onClick.AddListener(CleanScreen);
        stopButton.onClick.AddListener(StopRun);
        returnButton.onClick.AddListener(OnDestroy);
        setConfigButton.onClick.AddListener(RunMenu);

        if (runStart)
        {
            //leftHandController.SetActive(false);
            //rightHandController.SetActive(false);
            //controllerMesh.enabled = false;     // Desactiva el modelo del controlador
            //lineRenderer.enabled = false;       // Desactiva el rayo rojo
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


            if (selectedStimulusInt == stimulusTargetOrder[trial-1])
            {
                HappyFace.SetActive(true);
                Debug.Log("Correcto");
                Debug.Log("Estímulo objetivo: " + stimulusTargetOrder[trial-1]);
                Debug.Log("Estímulo seleccionado: " + selectedStimulusInt);
            }
            else
            {
                SadFace.SetActive(true);
                Debug.Log("Incorrecto");
                Debug.Log("Estímulo objetivo: " + stimulusTargetOrder[trial-1]);
                Debug.Log("Estímulo seleccionado: " + selectedStimulusInt);
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
            //leftHandController.SetActive(true);
            //rightHandController.SetActive(true);
            //controllerMesh.enabled = true;      // Activa el modelo del controlador
            //lineRenderer.enabled = true;        // Activa el rayo rojo
        }

    }

    private void RunMenu()
    {
        startButton.onClick.AddListener(CreateProcessStart);
        stopButton.onClick.AddListener(CreateProcessStop);
        returnButton.onClick.AddListener(CreateProcessReturn);
        udpClient = new UdpClient(port); // esto tienes que cerrarlo, para que no se cree un puerto cada vez que entre aquí.
        udpClient.BeginReceive(ReceiveCallback, null);
        StopButton.SetActive(false);
        BlockCompleted.SetActive(false);
        blockCompleted = false;
        //audioSource = GetComponent<AudioSource>();
    }


    private void CreateProcessStart()
    {
        string workingDirectory = "C:\\BCI2000_v3_6\\prog";
        string command = "/C BCI2000Command Start";
        CreateProcess(workingDirectory, command);
    }

    private void CreateProcessStop()
    {
        string workingDirectory = "C:\\BCI2000_v3_6\\prog";
        string command = "/C BCI2000Command Stop";
        CreateProcess(workingDirectory, command);
    }

    private void CreateProcessReturn()
    {
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
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "cmd.exe",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = false,
            Arguments = command
        };
        var process = Process.Start(processInfo);
        process.WaitForExit();
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

    private void ReceiveCallback(IAsyncResult ar)
    {
        IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
        byte[] bytes = udpClient.EndReceive(ar, ref ip);
        string message = Encoding.ASCII.GetString(bytes);

        string[] messageArray = message.Split('\t');
        string stimulusNumberString = messageArray[1];
        stimulusNumberInt = int.Parse(stimulusNumberString);
        string phaseInSequenceString = messageArray[3];
        phaseInSequenceInt = int.Parse(phaseInSequenceString);
        string selectedStimulusString = messageArray[4];
        selectedStimulusInt = int.Parse(selectedStimulusString);

        //Debug.Log("Array de bytes: " + message);
        //Debug.Log("stimulusNumberInt: " + stimulusNumberInt);
        //Debug.Log("phaseInSequenceInt: " + phaseInSequenceInt);
        //Debug.Log("selectedStimulusInt: " + selectedStimulusInt);

        if (stimulusNumberInt != 0)
        {
            //stimulusNumberInt = 1; // para testear la velocidad de presentaci�n
            stimulusPresented = true;
        }
        else if (stimulusNumberInt == 0 && phaseInSequenceInt == 2)
        {
            stimulusPresented = false;
        }

        switch (phaseInSequenceInt)
        {
            case 1:
                runStart = true;
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
                break;
            case 0:
                if (allowFinishing)
                {
                    blockCompleted = true;
                    trial = 0;
                    runStart = false;
                }
                break;
        }

        if (feedbackModeUDP && selectedStimulusInt != 0)
        {
            selectedStimulusPresented = true;
        }
        else
        {
            selectedStimulusPresented = false;
        }

        udpClient.BeginReceive(ReceiveCallback, null);         // Continue listening for more data
    }

    void CleanScreen()
    {
        blockCompleted = false;
        BackgroundRun.SetActive(false);
        StartButton.SetActive(false);
        ReturnButton.SetActive(false);
        BlockCompleted.SetActive(false);
        StopButton.SetActive(true);

        feedbackModeUDP = GetComponent<ProcessMainMenu>().feedbackMode;
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

    void StopRun()
    {
        blockCompleted = true;
    }

    void OnDestroy()
    {
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }

    void OnApplicationQuit()
    {
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }
}
