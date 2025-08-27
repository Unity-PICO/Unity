using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DropdownTextSwitcher : MonoBehaviour
{
    [Header("下拉组件")]
    public Dropdown dropdown; // using UnityEngine.UI;

    [Header("文本显示目标")]
    public TMP_Text targetText;

    [Header("选项对应文本")]
    [TextArea]
    public string[] optionTexts;

    void Start()
    {
        if (dropdown == null || targetText == null)
        {
            Debug.LogError("Dropdown 或 Text 未绑定！");
            return;
        }

        dropdown.onValueChanged.AddListener(OnDropdownChanged);
        OnDropdownChanged(dropdown.value); // 初始化时更新文本
    }

    void OnDropdownChanged(int index)
    {
        if (index >= 0 && index < optionTexts.Length)
        {
            targetText.text = optionTexts[index];
        }
        else
        {
            targetText.text = "未知选项";
        }
    }
}
