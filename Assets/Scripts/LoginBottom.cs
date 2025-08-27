using System;
using UnityEngine;
using TMPro;

public class LoginBottom : MonoBehaviour
{
    public SupabaseClient supabaseClient;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    // �ⲿ�������������
    public GameObject uiPanel1;
    public GameObject uiPanel2;
    public GameObject uiPanel3;
    public GameObject uiPanel4;

    // ��¼�ɹ�ʱ��Ҫ���صĽ���
    public GameObject hideOnLoginPanel;
    public GameObject hideOnLoginPanel2;

    // �ⲿ��ק��״̬���ı�
    public TMP_Text statusBar;

    // ����״̬���ı��Ĺ�������
    public void SetStatusBar(string msg)
    {
        if (statusBar != null)
            statusBar.text = msg;
    }

    // ��¶���޲�����¼����
    public void DoLogin()
    {
        string email = emailInput != null ? emailInput.text : "";
        string password = passwordInput != null ? passwordInput.text : "";

        if (supabaseClient != null)
        {
            SetStatusBar("���ڵ�¼...");
            StartCoroutine(supabaseClient.SignInWithPassword(
                email, password,
                OnLoginSuccess,
                OnLoginError
            ));
        }
        else
        {
            Debug.LogError("SupabaseClient δ��ֵ��");
            SetStatusBar("SupabaseClient δ��ֵ��");
        }
    }

    // ��¼�ɹ��ص�
    private void OnLoginSuccess(string msg)
    {
        SetStatusBar("��¼�ɹ�");
        if (uiPanel1 != null) uiPanel1.SetActive(true);
        if (uiPanel2 != null) uiPanel2.SetActive(true);
        if (uiPanel3 != null) uiPanel3.SetActive(true);
        if (uiPanel4 != null) uiPanel4.SetActive(true);
        if (hideOnLoginPanel != null) hideOnLoginPanel.SetActive(false);
        if (hideOnLoginPanel2 != null) hideOnLoginPanel2.SetActive(false);
        Debug.Log("��¼�ɹ�������ʾ�������沢����һ������");
    }

    // ��¼ʧ�ܻص�
    private void OnLoginError(SupabaseClient.SupabaseError error)
    {
        string errorMsg = "��¼ʧ��: " + (error != null ? error.msg : "δ֪����");
        SetStatusBar(errorMsg);
        Debug.LogError(errorMsg);
    }
}
