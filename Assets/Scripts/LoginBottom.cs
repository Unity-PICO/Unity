using System;
using UnityEngine;
using TMPro;

public class LoginBottom : MonoBehaviour
{
    public SupabaseClient supabaseClient;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    // 外部输入的两个界面
    public GameObject uiPanel1;
    public GameObject uiPanel2;
    public GameObject uiPanel3;
    public GameObject uiPanel4;

    // 登录成功时需要隐藏的界面
    public GameObject hideOnLoginPanel;
    public GameObject hideOnLoginPanel2;

    // 外部拖拽的状态栏文本
    public TMP_Text statusBar;

    // 设置状态栏文本的公共方法
    public void SetStatusBar(string msg)
    {
        if (statusBar != null)
            statusBar.text = msg;
    }

    // 暴露的无参数登录方法
    public void DoLogin()
    {
        string email = emailInput != null ? emailInput.text : "";
        string password = passwordInput != null ? passwordInput.text : "";

        if (supabaseClient != null)
        {
            SetStatusBar("正在登录...");
            StartCoroutine(supabaseClient.SignInWithPassword(
                email, password,
                OnLoginSuccess,
                OnLoginError
            ));
        }
        else
        {
            Debug.LogError("SupabaseClient 未赋值！");
            SetStatusBar("SupabaseClient 未赋值！");
        }
    }

    // 登录成功回调
    private void OnLoginSuccess(string msg)
    {
        SetStatusBar("登录成功");
        if (uiPanel1 != null) uiPanel1.SetActive(true);
        if (uiPanel2 != null) uiPanel2.SetActive(true);
        if (uiPanel3 != null) uiPanel3.SetActive(true);
        if (uiPanel4 != null) uiPanel4.SetActive(true);
        if (hideOnLoginPanel != null) hideOnLoginPanel.SetActive(false);
        if (hideOnLoginPanel2 != null) hideOnLoginPanel2.SetActive(false);
        Debug.Log("登录成功，已显示两个界面并隐藏一个界面");
    }

    // 登录失败回调
    private void OnLoginError(SupabaseClient.SupabaseError error)
    {
        string errorMsg = "登录失败: " + (error != null ? error.msg : "未知错误");
        SetStatusBar(errorMsg);
        Debug.LogError(errorMsg);
    }
}
