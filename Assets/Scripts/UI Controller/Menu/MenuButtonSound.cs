using UnityEngine;
using UnityEngine.EventSystems; // Cần thư viện này để bắt sự kiện chuột

// Script này giúp nút bấm nhận biết chuột đi vào (Hover) và Click
public class MenuButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    // Khi chuột đi vào nút
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (MainMenuController.Instance != null)
        {
            MainMenuController.Instance.PlayHoverSound();
        }
    }

    // Khi click vào nút (Để chắc chắn âm thanh luôn phát)
    public void OnPointerClick(PointerEventData eventData)
    {
        // Ta đã gọi PlayClickSound trong hàm OnPlayClicked rồi, 
        // nhưng để chắc ăn cho các nút khác sau này, ta có thể để ở đây.
        // Tạm thời chỉ dùng Hover ở đây cho đỡ bị lặp tiếng.
    }
}