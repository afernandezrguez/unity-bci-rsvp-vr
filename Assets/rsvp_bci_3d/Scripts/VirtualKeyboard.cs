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

        // Cargar el n�mero de participante guardado previamente
        if (PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            participantNumber = PlayerPrefs.GetString(PlayerPrefsKey);
            UpdateParticipantNumberText();
        }
    }

    public void AddDigit(string digit)
    {
        // Agrega el d�gito presionado al n�mero de participante
        if (participantNumber.Length < 2)
        {
            participantNumber += digit;
            UpdateParticipantNumberText();
        }
    }

    public void DeleteDigit()
    {
        // Borra el �ltimo d�gito del n�mero de participante
        if (participantNumber.Length > 0)
        {
            participantNumber = participantNumber.Substring(0, participantNumber.Length - 1);
            UpdateParticipantNumberText();
        }
    }

    void UpdateParticipantNumberText()
    {
        // Actualiza el elemento de la interfaz de usuario para mostrar el n�mero de participante
        participantNumberText.text = participantNumber;
        //Debug.Log("El nombre del participante es: " + participantNumber);

        // Guardar el n�mero de participante cuando se actualiza
        PlayerPrefs.SetString(PlayerPrefsKey, participantNumber);
        PlayerPrefs.Save();
    }
}
