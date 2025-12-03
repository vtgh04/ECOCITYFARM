using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    
    // Toggles (Checkboxes) or Buttons
    public void ToggleSettingsPanel()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void SetSound(bool isOn)
    {
        // Implement simple AudioListener volume or specific mixer groups
        Debug.Log("Sound: " + isOn);
    }

    public void SetMusic(bool isOn)
    {
        Debug.Log("Music: " + isOn);
    }
}