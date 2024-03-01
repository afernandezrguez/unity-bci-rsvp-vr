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
    private int stimulusNumberInt, phaseInSequenceInt;
    private Boolean stimulusPresented, showNextTarget, blockCompleted, allowFinishing;
    private GameObject[] stimuliArray;
    public GameObject StartButton, ReturnButton, BlockCompleted;
    public GameObject Canvas_bci_run, Canvas_bci_participant;

    [SerializeField] private GameObject RunMenuObjects;


    private AudioSource playerAudio;
    //public AudioClip jumpSound;

    private readonly int port = 12345;
    private readonly int[] stimulusTargetOrder = { 1, 2 };      // El "ToBeCopied" de BCI2000.
    private readonly int numberOfCommands = 10;

    private int trial = 0;
    private bool resetTrial = true;

    void Start()
    {
        OnDestroy();

        Canvas_bci_run.SetActive(false);
        Canvas_bci_participant.SetActive(false);

        // Todo esto tienes que ponerlo cuando se le dé a SetConfig, no en el Start de la única escena.

        //startButton.onClick.AddListener(CreateProcessStart);
        //returnButton.onClick.AddListener(CreateProcessReturn);
        stimuliArray = new GameObject[numberOfCommands];
        //udpClient = new UdpClient(port);
        //udpClient.BeginReceive(ReceiveCallback, null);
        //BlockCompleted.SetActive(false);
        InitializeStimuliArray();
        //playerAudio = GetComponent<AudioSource>();
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

                if (resetTrial)
                {
                    //playerAudio.PlayOneShot(jumpSound, 1.0f);
                    resetTrial = false;
                }

            }
            Invoke(nameof(DeactivateStimulusTarget), 1.0f);
        }

        if (blockCompleted)
        {
            RunMenuObjects.SetActive(true);
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
        //stimuliArray = new GameObject[numberOfCommands];
        udpClient = new UdpClient(port); // esto tienes que cerrarlo, para que no se cree un puerto cada vez que entre aquí.
        udpClient.BeginReceive(ReceiveCallback, null);
        BlockCompleted.SetActive(false);
        blockCompleted = false;
        //InitializeStimuliArray();
        //playerAudio = GetComponent<AudioSource>();
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
        //string stimulusNumberString = message.Substring(3, 2);
        int firstTabIndex = message.IndexOf('\t');
        string stimulusNumberString = message.Substring(firstTabIndex + 1, 2);
        stimulusNumberInt = int.Parse(stimulusNumberString);
        string phaseInSequenceString = message.Substring(message.Length - 4);
        phaseInSequenceInt = int.Parse(phaseInSequenceString);

        //Debug.Log("Array de bytes: " + message);
        //Debug.Log("stimulusNumberInt: " + stimulusNumberInt);
        //Debug.Log("phaseInSequenceInt: " + phaseInSequenceInt);

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
                showNextTarget = true;
                allowFinishing = true;
                break;
            case 2:
                showNextTarget = false;
                break;
            case 3:
                trial++;
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
        udpClient.BeginReceive(ReceiveCallback, null);         // Contin�a escuchando para m�s datos
    }

    void CleanScreen()
    {
        blockCompleted = false;
        RunMenuObjects.SetActive(false);
        //StartButton.SetActive(false);
        //BlockCompleted.SetActive(false);
        //ReturnButton.SetActive(false);
    }

    void DeactivateStimulusTarget()
    {
        showNextTarget = false;
    }

    void DeactivateStimulus()
    {
        foreach (GameObject stimulus in stimuliArray)
        {
            stimulus.SetActive(false);
        }
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
