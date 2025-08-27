using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DropdownTextSwitcher : MonoBehaviour
{
    [Header("�������")]
    public Dropdown dropdown; // using UnityEngine.UI;

    [Header("�ı���ʾĿ��")]
    public TMP_Text targetText;

    [Header("ѡ���Ӧ�ı�")]
    [TextArea]
    public string[] optionTexts;

    void Start()
    {
        if (dropdown == null || targetText == null)
        {
            Debug.LogError("Dropdown �� Text δ�󶨣�");
            return;
        }

        dropdown.onValueChanged.AddListener(OnDropdownChanged);
        OnDropdownChanged(dropdown.value); // ��ʼ��ʱ�����ı�
    }

    void OnDropdownChanged(int index)
    {
        if (index >= 0 && index < optionTexts.Length)
        {
            targetText.text = optionTexts[index];
        }
        else
        {
            targetText.text = "δ֪ѡ��";
        }
    }
}
