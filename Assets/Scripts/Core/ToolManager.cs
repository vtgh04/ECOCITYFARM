using UnityEngine;

public enum ToolType { None = 0, Harvest = 1, Remove = 2 }

public class ToolManager : MonoBehaviour
{
    public static ToolManager Instance { get; private set; }

    [Header("Cursor Settings")]
    [SerializeField] private Texture2D harvestCursorIcon; 
    [SerializeField] private Texture2D removeCursorIcon;

    public ToolType CurrentTool { get; private set; } = ToolType.None;

    private void Awake() 
    { 
        if (Instance == null) Instance = this; 
    }

    public void SelectTool(int toolIndex)
    {
        CurrentTool = (ToolType)toolIndex;
        UpdateCursor();

        // Disable building mode if using tools
        if (CurrentTool != ToolType.None)
            BuildingPlacementSystem.Instance.CancelPlacement();
            
        Debug.Log($"Tool Selected: {CurrentTool}");
    }

    public void DeselectTool()
    {
        SelectTool(0);
    }

    private void UpdateCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // Reset

        if (CurrentTool == ToolType.Harvest && harvestCursorIcon != null)
            Cursor.SetCursor(harvestCursorIcon, new Vector2(16, 16), CursorMode.Auto);
        
        else if (CurrentTool == ToolType.Remove && removeCursorIcon != null)
            Cursor.SetCursor(removeCursorIcon, new Vector2(16, 16), CursorMode.Auto);
    }
}