using UnityEngine;
using TMPro;

public class DayNightLightController : MonoBehaviour
{
    [Header("References")]
    public Light sunLight;
    public Transform rotationPivot; // The Parent of the Sun
    public TextMeshProUGUI timeText;

    [Header("Skybox Materials")]
    public Material skyboxDaybreak;  // 6-8
    public Material skyboxMidday;    // 8-15
    public Material skyboxAfternoon; // 15-18
    public Material skyboxNight;     // 18-6

    [System.Serializable]
    public struct LightPhase
    {
        public string name;
        public Color ambientColor; // Color of the shadows/ground
        public Color sunColor;     // Color of the direct light
        public float sunIntensity; 
        public Color fogColor;
        public float fogDensity;
    }

    [Header("Phase Settings")]
    public LightPhase daybreakSettings;  // 6:00
    public LightPhase middaySettings;    // 8:00
    public LightPhase afternoonSettings; // 15:00
    public LightPhase nightSettings;     // 18:00

    private float _currentHour;

    void Update()
    {
        if (TimeManager.Instance == null) return;

        // 1. Get Time
        _currentHour = TimeManager.Instance.CurrentHour;

        // 2. Update Text
        if (timeText != null) timeText.text = TimeManager.Instance.GetFormattedTime();

        // 3. Update Visuals
        UpdateSkybox();
        UpdateLighting();
    }

    private void UpdateSkybox()
    {
        Material targetMat = skyboxNight; // Default

        // 6-8: Daybreak
        if (_currentHour >= 6f && _currentHour < 9f) targetMat = skyboxDaybreak;
        
        // 8-15: Midday
        else if (_currentHour >= 9f && _currentHour < 15f) targetMat = skyboxMidday;
        
        // 15-18: Afternoon
        else if (_currentHour >= 15f && _currentHour < 18f) targetMat = skyboxAfternoon;
        
        // 18-6: Night (Already default)

        // Apply only if changed
        if (RenderSettings.skybox != targetMat)
        {
            RenderSettings.skybox = targetMat;
            DynamicGI.UpdateEnvironment(); // Important for reflections
        }
    }

    private void UpdateLighting()
    {
        LightPhase fromPhase, toPhase;
        float blendFactor;

        // --- CALCULATE BLENDING ---
        
        // 06:00 to 08:00 (Daybreak -> Midday)
        if (_currentHour >= 6f && _currentHour < 9f)
        {
            fromPhase = daybreakSettings;
            toPhase = middaySettings;
            blendFactor = (_currentHour - 6f) / 3f; // Duration 3 hours
        }
        // 08:00 to 15:00 (Midday -> Afternoon)
        else if (_currentHour >= 9f && _currentHour < 15f)
        {
            fromPhase = middaySettings;
            toPhase = afternoonSettings;
            blendFactor = (_currentHour - 9f) / 6f; // Duration 6 hours
        }
        // 15:00 to 18:00 (Afternoon -> Night)
        else if (_currentHour >= 15f && _currentHour < 18f)
        {
            fromPhase = afternoonSettings;
            toPhase = nightSettings;
            blendFactor = (_currentHour - 15f) / 3f; // Duration 3 hours
        }
        // 18:00 to 06:00 (Night -> Daybreak)
        else
        {
            fromPhase = nightSettings;
            toPhase = daybreakSettings;
            
            // Handle math for overnight wrapping
            if (_currentHour >= 18f) blendFactor = (_currentHour - 18f) / 12f;
            else blendFactor = (_currentHour + 6f) / 12f; // For 00:00 to 06:00
        }

        // --- APPLY VALUES ---

        // 1. Ambient Light (This paints the objects/houses)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat; // Force Color Mode
        RenderSettings.ambientLight = Color.Lerp(fromPhase.ambientColor, toPhase.ambientColor, blendFactor);

        // 2. Sun Settings
        if (sunLight != null)
        {
            sunLight.color = Color.Lerp(fromPhase.sunColor, toPhase.sunColor, blendFactor);
            sunLight.intensity = Mathf.Lerp(fromPhase.sunIntensity, toPhase.sunIntensity, blendFactor);
        }

        // 3. Fog Settings
        if (RenderSettings.fog)
        {
            RenderSettings.fogColor = Color.Lerp(fromPhase.fogColor, toPhase.fogColor, blendFactor);
            RenderSettings.fogDensity = Mathf.Lerp(fromPhase.fogDensity, toPhase.fogDensity, blendFactor);
        }

        // 4. Sun Rotation (06:00 = 0 deg, 18:00 = 180 deg)
        // We map 6am-6pm to 0-180 degrees. Night can just stay rotated or reset.
        if (rotationPivot != null)
        {
            float rotX = 0f;
            if (_currentHour >= 6f && _currentHour <= 18f)
            {
                // Day time: 0 to 180
                rotX = ((_currentHour - 6f) / 12f) * 180f;
            }
            else
            {
                // Night time: 180 to 360
                if(_currentHour > 18f) rotX = 180f + ((_currentHour - 18f) / 12f) * 180f;
                else rotX = 180f + ((_currentHour + 6f) / 12f) * 180f;
            }
            
            rotationPivot.localRotation = Quaternion.Euler(rotX, -30f, 0f);
        }
    }
}