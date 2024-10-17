using UnityEngine;
using UnityEngine.UI;

public class VirtualKeyboard : MonoBehaviour
{
    public Text participantNumberText;
    public string participantNumber = "";

    // Referencias a los botones del teclado virtual
    public Button[] digitButtons;
    public Button deleteButton;

    //[SerializeField] private GameObject participantCodeInput;

    private const string PlayerPrefsKey = "ParticipantNumber";

    void Start()
    {
        // Asigna funciones a los botones
        for (int i = 0; i < digitButtons.Length; i++)
        {
            int digit = i;
            digitButtons[i].onClick.AddListener(() => AddDigit(digit.ToString()));
        }
        deleteButton.onClick.AddListener(DeleteDigit);

        // Cargar el número de participante guardado previamente
        if (PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            participantNumber = PlayerPrefs.GetString(PlayerPrefsKey);
            UpdateParticipantNumberText();
        }
    }

    public void AddDigit(string digit)
    {
        // Agrega el dígito presionado al número de participante
        if (participantNumber.Length < 2)
        {
            participantNumber += digit;
            UpdateParticipantNumberText();
        }
    }

    public void DeleteDigit()
    {
        // Borra el último dígito del número de participante
        if (participantNumber.Length > 0)
        {
            participantNumber = participantNumber.Substring(0, participantNumber.Length - 1);
            UpdateParticipantNumberText();
        }
    }

    void UpdateParticipantNumberText()
    {
        // Actualiza el elemento de la interfaz de usuario para mostrar el número de participante
        participantNumberText.text = participantNumber;
        //Debug.Log("El nombre del participante es: " + participantNumber);

        // Guardar el número de participante cuando se actualiza
        PlayerPrefs.SetString(PlayerPrefsKey, participantNumber);
        PlayerPrefs.Save();
    }
}
