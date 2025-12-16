using UnityEngine;

public class PostOfficeClickable : MonoBehaviour
{
    private float _creationTime;
    private const float PREVENT_CLICK_DURATION = 0.5f; 

    private void Awake()
    {
       
        _creationTime = Time.time;
    }

    public void OnClick()
    {
 
        if (Time.time < _creationTime + PREVENT_CLICK_DURATION)
        {
            return;
        }

        
        if (PostOfficeUI.Instance != null)
        {
            PostOfficeUI.Instance.TogglePanel();
        }
    }
    
}
