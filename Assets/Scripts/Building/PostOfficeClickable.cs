using UnityEngine;

public class PostOfficeClickable : MonoBehaviour
{
    private float _creationTime;
    private const float PREVENT_CLICK_DURATION = 0.5f; // Wait 0.5s before allowing clicks

    private void Awake()
    {
        // Awake runs INSTANTLY when instantiated, unlike Start which can be delayed slightly.
        _creationTime = Time.time;
    }

    public void OnClick()
    {
        // Check: Is the current time less than Creation Time + 0.5 seconds?
        // If yes, it means we just placed it. IGNORE the click.
        if (Time.time < _creationTime + PREVENT_CLICK_DURATION)
        {
            return;
        }

        // Debug.Log("Open Post Office Panel");
        
        if (PostOfficeUI.Instance != null)
        {
            PostOfficeUI.Instance.TogglePanel();
        }
    }
    
}
