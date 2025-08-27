using UnityEngine;

public class PanelToggleController : MonoBehaviour
{
    // 需要显示/隐藏的目标面板
    public GameObject targetPanel;

    // 点击按钮时调用的方法
    public void TogglePanel()
    {
        if (targetPanel != null)
        {
            bool isActive = targetPanel.activeSelf;
            targetPanel.SetActive(!isActive);
        }
    }
}
