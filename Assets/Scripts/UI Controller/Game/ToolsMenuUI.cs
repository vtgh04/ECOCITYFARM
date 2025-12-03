using UnityEngine;

public class ToolsMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject toolsPanel;

    private bool _isOpen = false;

    private void Start()
    {
        if(toolsPanel) toolsPanel.SetActive(false);
    }

    public void ToggleToolsMenu()
    {
        _isOpen = !_isOpen;
        if(toolsPanel) toolsPanel.SetActive(_isOpen);
    }

    public void SelectHarvestTool()
    {
        if (ToolManager.Instance == null) return;

        // TOGGLE LOGIC: If we are already holding the Harvest tool, drop it.
        if (ToolManager.Instance.CurrentTool == ToolType.Harvest)
        {
            ToolManager.Instance.DeselectTool();
        }
        else
        {
            ToolManager.Instance.SelectTool(1); // 1 = Harvest
        }
    }

    public void SelectRemoveTool()
    {
        if (ToolManager.Instance == null) return;

        // TOGGLE LOGIC
        if (ToolManager.Instance.CurrentTool == ToolType.Remove)
        {
            ToolManager.Instance.DeselectTool();
        }
        else
        {
            ToolManager.Instance.SelectTool(2); // 2 = Remove
        }
    }
     public void CloseToolsMenu()
    {
        _isOpen = false;
        if(toolsPanel) toolsPanel.SetActive(false);
    }
   public bool IsToolsMenuOpen() => toolsPanel.activeSelf;

}