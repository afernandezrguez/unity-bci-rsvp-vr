using UnityEngine;
using UnityEngine.UI;

public class SequenceController : MonoBehaviour
{
    public InputField inputField; // Referencia a la casilla de texto
    public int currentNumber = 0; // N�mero actual
    public int fontSize = 70; // Tama�o de la fuente
    public string currentNumberString;
    private const string NumberKey = "CurrentNumber"; // Clave para PlayerPrefs

    private void Start()
    {
        // Cargar el valor guardado de la �ltima sesi�n
        currentNumber = PlayerPrefs.GetInt(NumberKey, 0); // 0 es el valor predeterminado si no hay nada guardado
        inputField.textComponent.fontSize = fontSize;
        UpdateInputField();
    }

    public void IncreaseNumber()
    {
        currentNumber++;
        UpdateInputField();
        SaveNumber(); // Guardar el n�mero actualizado
    }

    public void DecreaseNumber()
    {
        currentNumber--;
        UpdateInputField();
        SaveNumber(); // Guardar el n�mero actualizado
    }

    private void UpdateInputField()
    {
        inputField.text = currentNumber.ToString();

        currentNumberString = currentNumber.ToString();

    }

    private void SaveNumber()
    {
        // Guardar el n�mero en PlayerPrefs
        PlayerPrefs.SetInt(NumberKey, currentNumber);
        PlayerPrefs.Save(); // Asegura que se guarden los datos inmediatamente
    }
}
