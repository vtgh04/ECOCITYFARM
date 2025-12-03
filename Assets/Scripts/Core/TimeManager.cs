using UnityEngine;
using System;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }
    
    // Event to tell crops to grow
    public static event Action OnDayChanged;

    [Header("Time Settings")]
    [Tooltip("How many real seconds = 1 game day (24 hours)?")]
    public float realSecondsPerGameDay = 120f; // Slower day for better testing

    // FIXED START TIME: 6 AM
    private const float START_HOUR = 6f; 

    private int _currentDay = 1;
    private float _dayProgress = 0f; // 0.0 to 1.0
    public float DayProgress => _dayProgress; 

    public int CurrentDay => _currentDay;
    
    // Returns current hour (0.0 to 24.0)
    public float CurrentHour => _dayProgress * 24f; 

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        // Force start at 6:00 AM (6/24 = 0.25)
        _dayProgress = START_HOUR / 24f;
    }

    private void Update()
    {
        _dayProgress += Time.deltaTime / realSecondsPerGameDay;

        if (_dayProgress >= 1f)
        {
            _dayProgress = 0f;
            _currentDay++;
            OnDayChanged?.Invoke();
        }
    }

    public string GetFormattedTime()
    {
        float totalSeconds = _dayProgress * 86400f;
        int hours = Mathf.FloorToInt(totalSeconds / 3600f);
        int minutes = Mathf.FloorToInt((totalSeconds % 3600f) / 60f);
        return $"{hours:D2}:{minutes:D2}  Day {_currentDay}";
    }
    public void AdvanceDay()
    {
        _dayProgress = 0f;
        _currentDay++;
        OnDayChanged?.Invoke();
    }
   public void SetTimeData(int day, float progress)
    {
        _currentDay = day;
        _dayProgress = progress;
        // Optionally update UI immediately here
           Debug.Log($"Time Loaded: Day {day}, Progress {progress}");
    }
}