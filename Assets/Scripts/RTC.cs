using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pico.Platform;
using Pico.Platform.Models;
using UnityEngine.Networking;   // for UnityWebRequest
using System.Text;

public class RTC : MonoBehaviour
{
    [Header("Coze API Settings")]
    [Tooltip("在Inspector里填入Coze鉴权Token")]
    public string cozeAuthToken;       // Bearer Token
    [Tooltip("在Inspector里填入Bot ID")]
    public string botId;               // Bot ID

    [Header("UI References")]
    public Button buttonConnect;       // 单按钮连接/断开
    public Text logText;

    [Header("Audio Controls")]
    public Toggle toggleCapture;            // 开关麦克风
    public Slider sliderMicVolume;          // 本地麦克风音量
    public Slider sliderPlaybackVolume;     // 对方声音播放音量

    private string currentRoomId;
    private string userId = Guid.NewGuid().ToString(); // 随机生成UserId

    void Start()
    {
        InitPlatformAndRtc();

        buttonConnect.onClick.AddListener(OnConnectClicked);

        // Audio Controls 为空时，默认打开麦克风，音量默认
        if (toggleCapture == null)
        {
            Debug.LogWarning("toggleCapture 未绑定，默认打开麦克风");
            RtcService.StartAudioCapture();
        }
        else
        {
            toggleCapture.isOn = true;
            toggleCapture.onValueChanged.AddListener(OnMicToggle);
        }

        if (sliderMicVolume == null)
        {
            Debug.LogWarning("sliderMicVolume 未绑定，默认音量 80");
            RtcService.SetCaptureVolume(80);
        }
        else
        {
            sliderMicVolume.value = 80;
            sliderMicVolume.onValueChanged.AddListener(v => RtcService.SetCaptureVolume((int)v));
        }

        if (sliderPlaybackVolume == null)
        {
            Debug.LogWarning("sliderPlaybackVolume 未绑定，默认音量 80");
            RtcService.SetPlaybackVolume(80);
        }
        else
        {
            sliderPlaybackVolume.value = 80;
            sliderPlaybackVolume.onValueChanged.AddListener(v => RtcService.SetPlaybackVolume((int)v));
        }

        // 注册回调
        RtcService.SetOnJoinRoomResultCallback(OnJoinRoom);
        RtcService.SetOnLeaveRoomResultCallback(OnLeaveRoom);
        RtcService.SetOnUserJoinRoomResultCallback(OnUserJoinRoom);
        RtcService.SetOnUserLeaveRoomResultCallback(OnUserLeaveRoom);

        UpdateButtonText();
    }

    void InitPlatformAndRtc()
    {
        try
        {
            if (!CoreService.Initialized)
            {
                // 使用异步初始化（推荐方式）
                CoreService.AsyncInitialize().OnComplete(result =>
                {
                    if (!result.IsError &&
                        (result.Data == PlatformInitializeResult.Success || result.Data == PlatformInitializeResult.AlreadyInitialized))
                    {
                        Log("Platform SDK 初始化成功");
                        InitRtc();
                    }
                    else
                    {
                        Log("Init Platform SDK failed: " + result.Error?.Message);
                    }
                });
            }
            else
            {
                InitRtc();
            }
        }
        catch (Exception e)
        {
            Log("Init failed: " + e.Message);
        }
    }

    void InitRtc()
    {
        var res = RtcService.InitRtcEngine();
        if (res != RtcEngineInitResult.Success)
        {
            Log($"Init RTC Engine Failed {res}");
        }
        else
        {
            Log("RTC Engine initialized");
            RtcService.EnableAudioPropertiesReport(2000);
        }
    }

    void OnConnectClicked()
    {
        if (string.IsNullOrEmpty(currentRoomId))
        {
            // 当前没连 → 发起连接
            if (string.IsNullOrEmpty(cozeAuthToken) || string.IsNullOrEmpty(botId))
            {
                Log("请在Inspector中填入 cozeAuthToken 和 botId");
                return;
            }
            StartCoroutine(RequestRoomFromCoze());
        }
        else
        {
            // 已经在房间 → 执行断开
            LeaveRoom();
        }
    }

    System.Collections.IEnumerator RequestRoomFromCoze()
    {
        string url = "https://api.coze.cn/v1/audio/rooms";
        string jsonBody = "{\"bot_id\":\"" + botId + "\"}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + cozeAuthToken);

            Log("请求创建房间中...");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Log("请求失败: " + request.error);
            }
            else
            {
                try
                {
                    var response = request.downloadHandler.text;
                    Log("房间请求返回: " + response);

                    // 解析JSON，提取 room_id
                    var json = JsonUtility.FromJson<RoomResponseWrapper>(response);
                    string roomId = json.data.room_id;

                    if (!string.IsNullOrEmpty(roomId))
                    {
                        Log("获取到 RoomId: " + roomId);
                        JoinRoom(roomId);
                    }
                    else
                    {
                        Log("RoomId 为空，无法加入房间");
                    }
                }
                catch (Exception e)
                {
                    Log("解析返回JSON失败: " + e.Message);
                }
            }
        }
    }

    void JoinRoom(string roomId)
    {
        var privilege = new Dictionary<RtcPrivilege, int>
        {
            { RtcPrivilege.PublishStream, 3600 },
            { RtcPrivilege.SubscribeStream, 3600 }
        };

        // 这里用 roomId + userId + privilege 获取 token
        RtcService.GetToken(roomId, userId, 3600, privilege).OnComplete(msg =>
        {
            if (msg.IsError)
            {
                Log("GetToken failed: " + msg.Error.Message);
                return;
            }

            string token = msg.Data;
            Log("Got RTC Token");
            int result = RtcService.JoinRoom(roomId, userId, token, RtcRoomProfileType.Communication, true);
            Log($"Join Room Result={result}");
            currentRoomId = roomId;
            UpdateButtonText();
        });
    }

    void LeaveRoom()
    {
        if (!string.IsNullOrEmpty(currentRoomId))
        {
            RtcService.LeaveRoom(currentRoomId);
        }
    }

    void OnMicToggle(bool isOn)
    {
        if (isOn)
        {
            RtcService.StartAudioCapture();
            Log("Start Mic");
        }
        else
        {
            RtcService.StopAudioCapture();
            Log("Stop Mic");
        }
    }

    #region RTC Callbacks
    void OnJoinRoom(Message<RtcJoinRoomResult> msg)
    {
        if (msg.IsError || msg.Data.ErrorCode != 0)
        {
            Log("Join Room Failed");
            currentRoomId = null;
        }
        else
        {
            Log($"Joined Room: {msg.Data.RoomId}, User: {msg.Data.UserId}");
        }
        UpdateButtonText();
    }

    void OnLeaveRoom(Message<RtcLeaveRoomResult> msg)
    {
        if (!msg.IsError)
        {
            Log($"Left Room: {msg.Data.RoomId}");
            currentRoomId = null;
        }
        UpdateButtonText();
    }

    void OnUserJoinRoom(Message<RtcUserJoinInfo> msg)
    {
        Log($"User {msg.Data.UserId} joined {msg.Data.RoomId}");
    }

    void OnUserLeaveRoom(Message<RtcUserLeaveInfo> msg)
    {
        Log($"User {msg.Data.UserId} left {msg.Data.RoomId}");
    }
    #endregion

    void UpdateButtonText()
    {
        if (buttonConnect != null)
        {
            var txt = buttonConnect.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.text = string.IsNullOrEmpty(currentRoomId) ? "连接" : "断开";
            }
        }
    }

    void Log(string s)
    {
        Debug.Log("[RTC] " + s);
        if (logText != null) logText.text = s + "\n" + logText.text;
    }

    // 用于JSON解析的包装类
    [Serializable]
    private class RoomResponseWrapper
    {
        public RoomData data;
    }

    [Serializable]
    private class RoomData
    {
        public string room_id;
    }
}
