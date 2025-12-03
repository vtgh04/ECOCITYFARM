using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Attach this to ANY Button you want to have sound
public class UISoundTrigger : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private Button _btn;

    private void Awake()
    {
        _btn = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Only play hover if the button is interactable
        if (_btn != null && !_btn.interactable) return;
        // Debug.Log("Mouse Entered Button: " + gameObject.name); // 

        if (GameSoundController.Instance != null)
            GameSoundController.Instance.PlayHover();
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_btn != null && !_btn.interactable) return;

        if (GameSoundController.Instance != null)
            GameSoundController.Instance.PlayClick();
    }
    
}