using UnityEngine;
using UnityEngine.UI;

public class ConditionController : MonoBehaviour
{
    public Toggle condition1Toggle;
    public Toggle condition2Toggle;

    void Start()
    {
        // Aseg�rate de que ambos toggles est�n conectados a un Toggle Group en el inspector.
        condition1Toggle.onValueChanged.AddListener(delegate { OnToggleChanged(condition1Toggle); });
        condition2Toggle.onValueChanged.AddListener(delegate { OnToggleChanged(condition2Toggle); });
    }

    void OnToggleChanged(Toggle changedToggle)
    {
        // Puedes realizar cualquier acci�n adicional cuando se cambia un Toggle
        if (changedToggle.isOn)
        {
            Debug.Log(changedToggle.name + " is selected");
        }
    }
}
