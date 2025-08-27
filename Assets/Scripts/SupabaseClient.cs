using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class SupabaseClient : MonoBehaviour
{
    [Header("Supabase Settings")]
    public string supabaseUrl = "https://vbzpukdepfdipvfyetvx.supabase.co";
    public string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InZienB1a2RlcGZkaXB2ZnlldHZ4Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3Mzk3ODgwMjIsImV4cCI6MjA1NTM2NDAyMn0.mJfBm81w0ZWnCz0a08NMe4X8nuzk2rohtTLOXC9qAW4";

    [Header("Login Settings")]
    public string email = "";
    public string password = "";
    
    private string accessToken;
    private string refreshToken;

    public string AccessToken => accessToken;
    public string RefreshToken => refreshToken;

    /// <summary>
    /// 是否已登录
    /// </summary>
    public bool IsLoggedIn => !string.IsNullOrEmpty(accessToken);

    // 查询参数
    [Header("Quary Settings")]
    public string queryTableName = "users";
    public string queryString = "?select=*";

    private Coroutine refreshCoroutine;

    [Header("UI")]
    public TMP_Text StatusBar; // 外部可拖拽赋值

    [Serializable]
    private class SignInPayload
    {
        public string email;
        public string password;
    }

    [Serializable]
    private class RefreshPayload
    {
        public string refresh_token;
    }

    [Serializable]
    public class SupabaseError
    {
        public int code;
        public string error_code;
        public string msg;
    }

    [Serializable]
    public class SupabaseAuthResponse
    {
        public string access_token;
        public string token_type;
        public int expires_in;
        public string refresh_token;
        public string user;
    }

    private void SetStatusBar(string msg)
    {
        if (StatusBar != null)
            StatusBar.text = msg;
    }

    // 新增：启动时检查网络连接
    private void Start()
    {
        Application.targetFrameRate = 72; // 设置帧率为72
        CheckNetworkStatus();
    }

    private void CheckNetworkStatus()
    {
        string status = Application.internetReachability switch
        {
            NetworkReachability.NotReachable => "网络不可用，请检查连接",
            NetworkReachability.ReachableViaCarrierDataNetwork => "已连接移动数据网络",
            NetworkReachability.ReachableViaLocalAreaNetwork => "已连接WiFi",
            _ => "网络状态未知",
        };
        SetStatusBar(status);
        Debug.Log("网络状态：" + status);
    }

    private bool hasInspectorLoginTriggered = false;

    // Inspector菜单调用：登录并启动自动刷新
    [ContextMenu("Sign In With Password")]
    public void SignInWithPasswordInspector()
    {
        if (!hasInspectorLoginTriggered)
        {
            hasInspectorLoginTriggered = true;
            return;
        }

        SetStatusBar("正在登录...");
        StartCoroutine(SignInWithPassword(email, password,
            (result) => {
                Debug.Log("登录成功");
                SetStatusBar("登录成功！");
                SupabaseAuthResponse auth = JsonUtility.FromJson<SupabaseAuthResponse>(result);
                accessToken = auth.access_token;
                refreshToken = auth.refresh_token;
                // 启动定时刷新协程
                if (refreshCoroutine != null)
                {
                    StopCoroutine(refreshCoroutine);
                }
                refreshCoroutine = StartCoroutine(AutoRefreshTokenCoroutine());
            },
            (error) => {
                Debug.LogError($"登录失败,请重试\n错误类型: {error.error_code},\n描述: {error.msg}");
                SetStatusBar($"登录失败: {error.msg}");
            }
        ));
    }

    // Inspector菜单调用：终止自动刷新
    [ContextMenu("Stop Auto Refresh Token")]
    public void StopAutoRefreshTokenInspector()
    {
        if (refreshCoroutine != null)
        {
            StopCoroutine(refreshCoroutine);
            refreshCoroutine = null;
            Debug.Log("自动刷新已终止");
        }
        else
        {
            Debug.Log("没有正在运行的自动刷新协程");
        }
    }

    // Inspector菜单：表查询
    [ContextMenu("Query Table")]
    public void QueryTableInspector()
    {
        StartCoroutine(QueryTable(
            queryTableName,
            queryString,
            (result) => { Debug.Log($"表查询成功: {result}"); },
            (error) => { Debug.LogError($"表查询失败: {error.msg}"); }
        ));
    }

    public IEnumerator SignInWithPassword(string email, string password, Action<string> onSuccess, Action<SupabaseError> onError)
    {
        // 新增：账号或密码为空校验
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            SetStatusBar("账号和密码不能为空！");
            onError?.Invoke(new SupabaseError
            {
                code = 400,
                error_code = "empty_credentials",
                msg = "账号和密码不能为空！"
            });
            yield break;
        }

        // 防止重复登录
        if (IsLoggedIn)
        {
            var err = new SupabaseError
            {
                code = 409,
                error_code = "already_logged_in",
                msg = "已登录，无需重复登录。"
            };
            SetStatusBar(err.msg);
            onError?.Invoke(err);
            yield break;
        }

        SetStatusBar("正在登录...");
        string url = $"{supabaseUrl}/auth/v1/token?grant_type=password";

        var payload = new SignInPayload
        {
            email = email,
            password = password
        };

        string jsonData = JsonUtility.ToJson(payload);
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("apikey", supabaseAnonKey);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            SetStatusBar("登录成功！");
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            SupabaseError supabaseError = JsonUtility.FromJson<SupabaseError>(request.downloadHandler.text);
            SetStatusBar($"登录失败: {supabaseError.msg}");
            onError?.Invoke(supabaseError);
        }
    }

    // 刷新token方法
    public IEnumerator RefreshAccessToken(Action<string> onSuccess, Action<SupabaseError> onError)
    {
        Debug.Log("正在刷新Token...");
        string url = $"{supabaseUrl}/auth/v1/token?grant_type=refresh_token";
        var payload = new RefreshPayload { refresh_token = refreshToken };
        string jsonData = JsonUtility.ToJson(payload);

        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("apikey", supabaseAnonKey);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            SupabaseAuthResponse authResponse = JsonUtility.FromJson<SupabaseAuthResponse>(request.downloadHandler.text);
            accessToken = authResponse.access_token;
            refreshToken = authResponse.refresh_token;
            Debug.Log("access_token已刷新: " + accessToken);
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            SupabaseError supabaseError = JsonUtility.FromJson<SupabaseError>(request.downloadHandler.text);
            Debug.LogError($"刷新access_token失败\n错误类型: {supabaseError.error_code},\n描述: {supabaseError.msg}");
            onError?.Invoke(supabaseError);
        }
    }

    // 定时自动刷新协程
    private IEnumerator AutoRefreshTokenCoroutine()
    {
        while (true)
        {
            Debug.Log("等待30分钟后刷新access_token...");
            yield return new WaitForSeconds(1800f); // 30分钟
            Debug.Log("开始刷新access_token...");
            yield return RefreshAccessToken(
                (result) => { /* 可选：刷新成功处理 */ },
                (error) => { /* 可选：刷新失败处理 */ }
            );
        }
    }

    public IEnumerator QueryTable(
        string tableName,
        string queryString, // 例如："?select=*" 或 "?id=eq.1"
        Action<string> onSuccess,
        Action<SupabaseError> onError)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            SupabaseError error = new()
            {
                code = 401,
                error_code = "no_access_token",
                msg = "accessToken不存在，无法进行查询。请先登录。"
            };
            SetStatusBar(error.msg);
            onError?.Invoke(error);
            yield break;
        }

        SetStatusBar("正在查询...");
        // 构建请求URL
        string url = $"{supabaseUrl}/rest/v1/{tableName}{queryString}";

        var request = new UnityWebRequest(url, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("apikey", supabaseAnonKey);
        request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            SetStatusBar("查询成功！");
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            SupabaseError supabaseError = JsonUtility.FromJson<SupabaseError>(request.downloadHandler.text);
            SetStatusBar($"查询失败: {supabaseError.msg}");
            onError?.Invoke(supabaseError);
        }
    }
}
