using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    

    public void ToggleSettingsPanel()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void SetSound(bool isOn)
    {
        Debug.Log("Sound: " + isOn);
    }

    public void SetMusic(bool isOn)
    {
        Debug.Log("Music: " + isOn);
    }
}