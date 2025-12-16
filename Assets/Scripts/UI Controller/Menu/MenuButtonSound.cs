using UnityEngine;
using UnityEngine.EventSystems;


public class MenuButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (MainMenuController.Instance != null)
        {
            MainMenuController.Instance.PlayHoverSound();
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
    }
}