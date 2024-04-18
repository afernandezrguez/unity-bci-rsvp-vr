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
    [SerializeField] private Button returnButton;
    [SerializeField] private Button setConfigButton;

    private UdpClient udpClient;
    private int stimulusNumberInt, phaseInSequenceInt, selectedStimulusInt;
    private Boolean stimulusPresented, allowNextTarget, showNextTarget, selectedStimulusPresented, blockCompleted, allowFinishing;

    private Boolean feedbackModeUDP;

    public ProcessMainMenu feedbackMode;

    private GameObject[] stimuliArray;
    public GameObject StartButton, ReturnButton, BlockCompleted;
    public GameObject Canvas_bci_run, Canvas_bci_participant;
    public GameObject FocusOnText, SelectedStimulusText;

    //[SerializeField] private GameObject RunMenuObjects;

    public AudioClip focusOnSound;
    private AudioSource audioSource;

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

        stimuliArray = new GameObject[numberOfCommands];
        InitializeStimuliArray();

        audioSource = GetComponent<AudioSource>();
        audioSource.clip = focusOnSound;
    }

    void Update()
    {
        startButton.onClick.AddListener(CleanScreen);
        returnButton.onClick.AddListener(OnDestroy);
        setConfigButton.onClick.AddListener(RunMenu);

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
                    //audioSource.Play();
                    resetTrial = false;
                }

            }
            Invoke(nameof(DeactivateStimulusTarget), 1.0f);
        }

        if (selectedStimulusPresented)
        {
            stimuliArray[selectedStimulusInt - 1].SetActive(true);
            SelectedStimulusText.SetActive(true);

            Invoke(nameof(DeactivateSelectedStimulus), 1.0f);
        }

        if (blockCompleted)
        {
            //RunMenuObjects.SetActive(true);
            Canvas_bci_run.SetActive(true);

            StartButton.SetActive(true);
            BlockCompleted.SetActive(true);
            ReturnButton.SetActive(true);
            resetTrial = true;
        }

        //if (Input.GetKey(KeyCode.Backspace))
        //{
        //    string workingDirectory = "C:\\BCI2000_v3_6\\prog";
        //    string command = "/C BCI2000Command Stop";
        //    CreateProcess(workingDirectory, command);
        //    stimulusPresented = false;
        //    blockCompleted = true;
        //}
    }

    private void RunMenu()
    {
        startButton.onClick.AddListener(CreateProcessStart);
        returnButton.onClick.AddListener(CreateProcessReturn);
        udpClient = new UdpClient(port); // esto tienes que cerrarlo, para que no se cree un puerto cada vez que entre aquí.
        udpClient.BeginReceive(ReceiveCallback, null);
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

    private void InitializeStimuliArray()               // Asigna los GameObjects a los elementos del array
    {
        for (int i = 0; i < numberOfCommands; i++)
        {
            string stimulusName = "Stimulus" + (i + 1); // Genera el nombre del GameObject
            stimuliArray[i] = GameObject.Find(stimulusName); // Encuentra el GameObject por su nombre y lo asigna al array
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

        udpClient.BeginReceive(ReceiveCallback, null);         // Contin�a escuchando para m�s datos
    }

    void CleanScreen()
    {
        blockCompleted = false;
        //RunMenuObjects.SetActive(false);
        Canvas_bci_run.SetActive(false);


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
    }

    void DeactivateSelectedStimulus()
    {
        selectedStimulusPresented = false;
        SelectedStimulusText.SetActive(false);
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
