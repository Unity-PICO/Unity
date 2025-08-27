using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor; // 仅用于检测当前活动构建目标
#endif

namespace bytertc
{
    [Serializable]
    public class RoomApiResponse
    {
        public RoomData data;
        public int code;
        public string msg;
        public ResponseDetail detail;
    }

    [Serializable]
    public class RoomData
    {
        public string token;
        public string uid;
        public string room_id;
        public string app_id;
    }

    [Serializable]
    public class ResponseDetail
    {
        public string logid;
    }

    [Serializable]
    public class RequestBody
    {
        // 根据你的实际 bot_id 调整
        public string bot_id = "7471861030994346021";
    }

    public class AutoJoinDriver : MonoBehaviour
    {
        [Header("UI References")]
        public Button autoJoinButton;
        public Text statusText;
        public LoginRTC loginRTC;

        private const string API_URL   = "https://api.coze.cn/v1/audio/rooms";
        // 仅示例：真实项目建议服务端下发，不要硬编码到客户端
        private const string AUTH_TOKEN = "Bearer pat_12djgGZVWctpZHxyz0G4PR9TlB907tbmA8rYHg68zc0MjTzKG3EVNA1lnZeStIum";

        private void Start()
        {
            if (autoJoinButton != null)
                autoJoinButton.onClick.AddListener(OnAutoJoinButtonClick);

            if (statusText != null)
                statusText.text = "点击按钮自动加入房间";

            if (loginRTC == null)
                loginRTC = FindObjectOfType<LoginRTC>();
        }

        public void OnAutoJoinButtonClick()
        {
            // 平台安全检查
            if (!CanSafelyJoinOnThisPlatform(out var reason))
            {
                Debug.LogWarning(reason);
                if (statusText) statusText.text = reason;
                return;
            }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            if (!PreflightCheckWindows(out var hint))
            {
                Debug.LogError(hint);
                if (statusText) statusText.text = hint;
                return;
            }
#endif

            if (statusText != null)
                statusText.text = "正在获取房间信息...";

            StartCoroutine(GetRoomInfoAndJoin());
        }

        private IEnumerator GetRoomInfoAndJoin()
        {
            var requestBody = new RequestBody();
            string jsonBody = JsonUtility.ToJson(requestBody);

            using (UnityWebRequest request = new UnityWebRequest(API_URL, UnityWebRequest.kHttpVerbPOST))
            {
                request.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonBody));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = 15;

                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json");
                request.SetRequestHeader("Authorization", AUTH_TOKEN);

                yield return request.SendWebRequest();

                long httpCode = request.responseCode;
                string body = request.downloadHandler != null ? request.downloadHandler.text : "";

                if (request.result != UnityWebRequest.Result.Success || httpCode < 200 || httpCode >= 300)
                {
                    string err = $"HTTP {(int)httpCode} {request.result}\nBody: {body}";
                    Debug.LogError(err);
                    if (statusText) statusText.text = "获取房间失败：" + err;
                    yield break;
                }

                yield return StartCoroutine(ProcessApiResponse(request));
            }
        }

        private IEnumerator ProcessApiResponse(UnityWebRequest request)
        {
            long httpCode = request.responseCode;
            string responseText = request.downloadHandler != null ? request.downloadHandler.text : "";

            if (request.result == UnityWebRequest.Result.Success && httpCode >= 200 && httpCode < 300)
            {
                RoomApiResponse resp = null;
                try
                {
                    resp = JsonUtility.FromJson<RoomApiResponse>(responseText);
                }
                catch (Exception e)
                {
                    var msg = "解析API响应失败: " + e.Message;
                    Debug.LogError(msg + "\nRaw: " + responseText);
                    if (statusText) statusText.text = msg;
                    yield break;
                }

                if (resp != null && resp.code == 0 && resp.data != null)
                {
                    // 写入 ByteRTC 所需的全局常量
                    Constants.APP_ID = resp.data.app_id;
                    Constants.TOKEN  = resp.data.token;

                    if (statusText) statusText.text = "房间信息获取成功，准备加入...";
                    Debug.Log($"Auto-filled: APP_ID={resp.data.app_id}, TOKEN(len)={resp.data.token?.Length}");
                    Debug.Log($"Room ID: {resp.data.room_id}, User ID: {resp.data.uid}");

                    // 驱动 LoginRTC 的手动路径
                    yield return StartCoroutine(DriveLoginRTC(resp.data.room_id, resp.data.uid));
                }
                else
                {
                    string msg = $"API返回错误: code={resp?.code}, msg={resp?.msg}";
                    Debug.LogError(msg + "\nRaw: " + responseText);
                    if (statusText) statusText.text = msg;
                }
            }
            else
            {
                string msg = $"网络请求失败/未授权: HTTP {(int)httpCode}, {request.error}\nBody: {responseText}";
                Debug.LogError(msg);
                if (statusText) statusText.text = msg;
            }
        }

        /// <summary>
        /// 复用 LoginRTC 的手动进房路径（填输入框 + 触发按钮），
        /// 并延迟 1~2 帧以避开组件/引擎初始化时序问题。
        /// 不修改 LoginRTC。
        /// </summary>
        private IEnumerator DriveLoginRTC(string roomId, string userId)
        {
            if (loginRTC == null)
            {
                Debug.LogError("LoginRTC reference is null!");
                if (statusText) statusText.text = "LoginRTC组件未找到";
                yield break;
            }

            // 等待 LoginRTC 的 Start() 与 UI 赋值完成
            yield return new WaitForEndOfFrame();
            yield return null;

            // 保险：等待关键 UI 不为空（最多 2 秒）
            float waitSec = 0f;
            while ((loginRTC.roomInput == null || loginRTC.userIdInput == null) && waitSec < 2f)
            {
                waitSec += Time.unscaledDeltaTime;
                yield return null;
            }

            if (loginRTC.roomInput != null)   loginRTC.roomInput.text   = roomId;
            else                               Debug.LogWarning("LoginRTC.roomInput is null");

            if (loginRTC.userIdInput != null) loginRTC.userIdInput.text = userId;
            else                               Debug.LogWarning("LoginRTC.userIdInput is null");

            // 再等一帧让 InputField 刷新
            yield return null;

            try
            {
                // 更贴近“用户点击”的触发
                if (loginRTC.enterRoomBtn != null)
                {
                    loginRTC.enterRoomBtn.onClick.Invoke();
                }
                else
                {
                    // 兜底：直接调用公开方法
                    loginRTC.EnterRoomClick();
                }

                if (statusText) statusText.text = $"已加入房间: {roomId}";
                Debug.Log($"Successfully joined room via manual path: {roomId} with user: {userId}");
            }
            catch (Exception e)
{
    string msg = "加入房间失败: " + e;
    Debug.LogError(msg); // 注意这里打印 e（包含类型与堆栈）
    if (statusText) statusText.text = msg;
}

        }

        /// <summary>
        /// 在当前运行/构建环境下，是否“安全地”发起进房。
        /// 目的：避免在 Editor + Android 构建目标时触发对 currentActivity 的访问。
        /// </summary>
        private bool CanSafelyJoinOnThisPlatform(out string reason)
        {
#if UNITY_EDITOR
            BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;
            if (activeTarget == BuildTarget.Android)
            {
                reason = "当前在 Unity Editor 且构建目标为 Android：底层会访问 Android 的 currentActivity，自动进房将报错。请改为真机(Android)运行，或切换构建目标为 Windows/Mac 并确保导入对应PC库后再在 Editor 内测试。";
                return false;
            }
#endif
            reason = null;
            return true;
        }

        /// <summary>
        /// Windows/Editor 下的预检：尝试读取 SDK 版本，能更快暴露缺 DLL 或运行库的问题。
        /// </summary>
        private bool PreflightCheckWindows(out string hint)
        {
            hint = null;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            try
            {
                var v = EngineManager.instance.GetSDKVersion();
                Debug.Log("RTC SDK Version: " + v);
                return true;
            }
            catch (DllNotFoundException e)
            {
                hint = "缺少 RTC 原生 DLL（Windows x86_64）。请确认 Assets/Plugins/x86_64 下存在 ByteRTC 的 Windows 原生库，并安装 VC++ 2015-2022 x64 运行库。原始错误: " + e.Message;
                return false;
            }
            catch (TypeLoadException e)
            {
                hint = "找不到 ByteRTCCWrapper（托管包装器）。请确认 ByteRTCCWrapper.dll 的插件设置勾选 Editor/Standalone，并与 .NET 4.x 兼容。原始错误: " + e.Message;
                return false;
            }
            catch (Exception e)
            {
                hint = "RTC 预检失败：" + e;
                return false;
            }
#else
            return true;
#endif
        }
    }
}
