using UnityEngine;
using TMPro;

public class MoneyPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmpText;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float disappearTimer = 1f;
    
    private Color textColor;

    public void Setup(int amount)
    {
        if (amount < 0)
        {
            // TIÊU TIỀN: Màu Đỏ, thêm dấu trừ
            tmpText.text = $"-${Mathf.Abs(amount)}";
            tmpText.color = Color.red;
        }
        else
        {
            // THU HOẠCH: Màu Xanh, thêm dấu cộng
            tmpText.text = $"+${amount}";
            tmpText.color = Color.green; 
        }

        // Lưu màu gốc để làm hiệu ứng mờ dần
        textColor = tmpText.color;
    }

    private void Update()
    {
        // 1. Bay lên
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        // 2. Mờ dần
        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float fadeSpeed = 3f;
            textColor.a -= fadeSpeed * Time.deltaTime;
            tmpText.color = textColor;

            if (textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
        
        // 3. Luôn quay mặt về phía Camera
        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }
    }
}