using UnityEngine;
using UnityEngine.UI;

public class TMP_ToggleControl : MonoBehaviour
{
    [Header("Toggle 开关")]
    public Toggle toggle;

    [Header("目标对象（显示/隐藏）")]
    public GameObject targetObject;

    [Header("文字 (Text)")]
    public Text toggleText; // 改为原生Text

    [Header("文字内容")]
    public string textWhenOn = "关闭";
    public string textWhenOff = "打开";

    void Start()
    {
        if (toggle == null || toggleText == null || targetObject == null)
        {
            Debug.LogError("Toggle、Text 或目标对象未绑定！");
            return;
        }

        // 初始化状态
        ApplyToggleState(toggle.isOn);

        // 监听 Toggle 状态变化
        toggle.onValueChanged.AddListener(ApplyToggleState);
    }

    void ApplyToggleState(bool isOn)
    {
        targetObject.SetActive(isOn);
        toggleText.text = isOn ? textWhenOn : textWhenOff;
    }
}
