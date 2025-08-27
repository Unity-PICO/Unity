using UnityEngine;
using UnityEngine.UI;

public class TMP_ToggleControl : MonoBehaviour
{
    [Header("Toggle ����")]
    public Toggle toggle;

    [Header("Ŀ�������ʾ/���أ�")]
    public GameObject targetObject;

    [Header("���� (Text)")]
    public Text toggleText; // ��Ϊԭ��Text

    [Header("��������")]
    public string textWhenOn = "�ر�";
    public string textWhenOff = "��";

    void Start()
    {
        if (toggle == null || toggleText == null || targetObject == null)
        {
            Debug.LogError("Toggle��Text ��Ŀ�����δ�󶨣�");
            return;
        }

        // ��ʼ��״̬
        ApplyToggleState(toggle.isOn);

        // ���� Toggle ״̬�仯
        toggle.onValueChanged.AddListener(ApplyToggleState);
    }

    void ApplyToggleState(bool isOn)
    {
        targetObject.SetActive(isOn);
        toggleText.text = isOn ? textWhenOn : textWhenOff;
    }
}
