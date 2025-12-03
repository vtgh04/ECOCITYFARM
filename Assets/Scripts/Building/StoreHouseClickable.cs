using UnityEngine;

public class StoreHouseClickable : MonoBehaviour
{
    private float _creationTime;
    private const float COOLDOWN = 0.5f;

    private void Awake()
    {
        _creationTime = Time.time;
    }

    public void OnClick()
    {
        if (Time.time < _creationTime + COOLDOWN) return;

        // Chỉ mở UI, không cần zoom nữa → Camera tự lo
        StoreHouseUI.Instance?.ShowPanel();
    }
}