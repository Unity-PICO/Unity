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
    /// �Ƿ��ѵ�¼
    /// </summary>
    public bool IsLoggedIn => !string.IsNullOrEmpty(accessToken);

    // ��ѯ����
    [Header("Quary Settings")]
    public string queryTableName = "users";
    public string queryString = "?select=*";

    private Coroutine refreshCoroutine;

    [Header("UI")]
    public TMP_Text StatusBar; // �ⲿ����ק��ֵ

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

    // ����������ʱ�����������
    private void Start()
    {
        Application.targetFrameRate = 72; // ����֡��Ϊ72
        CheckNetworkStatus();
    }

    private void CheckNetworkStatus()
    {
        string status = Application.internetReachability switch
        {
            NetworkReachability.NotReachable => "���粻���ã���������",
            NetworkReachability.ReachableViaCarrierDataNetwork => "�������ƶ���������",
            NetworkReachability.ReachableViaLocalAreaNetwork => "������WiFi",
            _ => "����״̬δ֪",
        };
        SetStatusBar(status);
        Debug.Log("����״̬��" + status);
    }

    private bool hasInspectorLoginTriggered = false;

    // Inspector�˵����ã���¼�������Զ�ˢ��
    [ContextMenu("Sign In With Password")]
    public void SignInWithPasswordInspector()
    {
        if (!hasInspectorLoginTriggered)
        {
            hasInspectorLoginTriggered = true;
            return;
        }

        SetStatusBar("���ڵ�¼...");
        StartCoroutine(SignInWithPassword(email, password,
            (result) => {
                Debug.Log("��¼�ɹ�");
                SetStatusBar("��¼�ɹ���");
                SupabaseAuthResponse auth = JsonUtility.FromJson<SupabaseAuthResponse>(result);
                accessToken = auth.access_token;
                refreshToken = auth.refresh_token;
                // ������ʱˢ��Э��
                if (refreshCoroutine != null)
                {
                    StopCoroutine(refreshCoroutine);
                }
                refreshCoroutine = StartCoroutine(AutoRefreshTokenCoroutine());
            },
            (error) => {
                Debug.LogError($"��¼ʧ��,������\n��������: {error.error_code},\n����: {error.msg}");
                SetStatusBar($"��¼ʧ��: {error.msg}");
            }
        ));
    }

    // Inspector�˵����ã���ֹ�Զ�ˢ��
    [ContextMenu("Stop Auto Refresh Token")]
    public void StopAutoRefreshTokenInspector()
    {
        if (refreshCoroutine != null)
        {
            StopCoroutine(refreshCoroutine);
            refreshCoroutine = null;
            Debug.Log("�Զ�ˢ������ֹ");
        }
        else
        {
            Debug.Log("û���������е��Զ�ˢ��Э��");
        }
    }

    // Inspector�˵������ѯ
    [ContextMenu("Query Table")]
    public void QueryTableInspector()
    {
        StartCoroutine(QueryTable(
            queryTableName,
            queryString,
            (result) => { Debug.Log($"���ѯ�ɹ�: {result}"); },
            (error) => { Debug.LogError($"���ѯʧ��: {error.msg}"); }
        ));
    }

    public IEnumerator SignInWithPassword(string email, string password, Action<string> onSuccess, Action<SupabaseError> onError)
    {
        // �������˺Ż�����Ϊ��У��
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            SetStatusBar("�˺ź����벻��Ϊ�գ�");
            onError?.Invoke(new SupabaseError
            {
                code = 400,
                error_code = "empty_credentials",
                msg = "�˺ź����벻��Ϊ�գ�"
            });
            yield break;
        }

        // ��ֹ�ظ���¼
        if (IsLoggedIn)
        {
            var err = new SupabaseError
            {
                code = 409,
                error_code = "already_logged_in",
                msg = "�ѵ�¼�������ظ���¼��"
            };
            SetStatusBar(err.msg);
            onError?.Invoke(err);
            yield break;
        }

        SetStatusBar("���ڵ�¼...");
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
            SetStatusBar("��¼�ɹ���");
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            SupabaseError supabaseError = JsonUtility.FromJson<SupabaseError>(request.downloadHandler.text);
            SetStatusBar($"��¼ʧ��: {supabaseError.msg}");
            onError?.Invoke(supabaseError);
        }
    }

    // ˢ��token����
    public IEnumerator RefreshAccessToken(Action<string> onSuccess, Action<SupabaseError> onError)
    {
        Debug.Log("����ˢ��Token...");
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
            Debug.Log("access_token��ˢ��: " + accessToken);
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            SupabaseError supabaseError = JsonUtility.FromJson<SupabaseError>(request.downloadHandler.text);
            Debug.LogError($"ˢ��access_tokenʧ��\n��������: {supabaseError.error_code},\n����: {supabaseError.msg}");
            onError?.Invoke(supabaseError);
        }
    }

    // ��ʱ�Զ�ˢ��Э��
    private IEnumerator AutoRefreshTokenCoroutine()
    {
        while (true)
        {
            Debug.Log("�ȴ�30���Ӻ�ˢ��access_token...");
            yield return new WaitForSeconds(1800f); // 30����
            Debug.Log("��ʼˢ��access_token...");
            yield return RefreshAccessToken(
                (result) => { /* ��ѡ��ˢ�³ɹ����� */ },
                (error) => { /* ��ѡ��ˢ��ʧ�ܴ��� */ }
            );
        }
    }

    public IEnumerator QueryTable(
        string tableName,
        string queryString, // ���磺"?select=*" �� "?id=eq.1"
        Action<string> onSuccess,
        Action<SupabaseError> onError)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            SupabaseError error = new()
            {
                code = 401,
                error_code = "no_access_token",
                msg = "accessToken�����ڣ��޷����в�ѯ�����ȵ�¼��"
            };
            SetStatusBar(error.msg);
            onError?.Invoke(error);
            yield break;
        }

        SetStatusBar("���ڲ�ѯ...");
        // ��������URL
        string url = $"{supabaseUrl}/rest/v1/{tableName}{queryString}";

        var request = new UnityWebRequest(url, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("apikey", supabaseAnonKey);
        request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            SetStatusBar("��ѯ�ɹ���");
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            SupabaseError supabaseError = JsonUtility.FromJson<SupabaseError>(request.downloadHandler.text);
            SetStatusBar($"��ѯʧ��: {supabaseError.msg}");
            onError?.Invoke(supabaseError);
        }
    }
}
