using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Android;
using Pico.Platform;
using TMPro;

public class SpeechDemo : MonoBehaviour
{
    public TMP_Text textAsrResult;

    private bool inited = false;
    private bool started = false;

    private InputDevice rightHandDevice;
    private bool prevTriggerState = false;

    void Start()
    {
        // PICO 核心服务初始化
        CoreService.Initialize();

        // 请求权限
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            Permission.RequestUserPermission(Permission.Microphone);
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);

        // 识别结果回调
        SpeechService.SetOnAsrResultCallback(msg =>
        {
            var m = msg.Data;
            Debug.Log($"text={m.Text} isFinal={m.IsFinalResult}");
            textAsrResult.SetText($"[{m.IsFinalResult}]{m.Text}");
        });

        // 错误回调，使用 Unity 自带 JsonUtility
        SpeechService.SetOnSpeechErrorCallback(msg =>
        {
            var m = msg.Data;
            Debug.Log($"SpeechError :{JsonUtility.ToJson(m)}");
        });

        // 尝试获取右手控制器实例
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
        if (devices.Count > 0)
            rightHandDevice = devices[0];
    }

    void Update()
    {
        // 控制器重连时重新获取
        if (!rightHandDevice.isValid)
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
            if (devices.Count > 0)
                rightHandDevice = devices[0];
        }

        // 读取扳机键按下的瞬时边沿
        bool triggerPressed = false;
        if (rightHandDevice.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed) 
            && triggerPressed && !prevTriggerState)
        {
            // 第一次按：初始化 ASR 引擎
            if (!inited)
            {
                var res = SpeechService.InitAsrEngine();
                if (res != AsrEngineInitResult.Success)
                {
                    Debug.Log($"Init ASR Engine failed :{res}");
                    textAsrResult.SetText($"init failed {res}");
                }
                else
                {
                    inited = true;
                    Debug.Log("Init engine successfully.");
                    textAsrResult.SetText("Init successfully");
                }
            }
            // 第二次按：启动 ASR（autoStop=true, showPunctual=true, maxDuration=10）
            else if (!started)
            {
                SpeechService.StartAsr(true, true, 10);
                started = true;
                Debug.Log("ASR engine started (autoStop: true, showPunctual: true, maxDuration:10)");
                textAsrResult.SetText("ASR started");
            }
            // 第三次及以后按：停止 ASR
            else
            {
                SpeechService.StopAsr();
                started = false;
                Debug.Log("ASR engine stopped");
                textAsrResult.SetText("ASR stopped");
            }
        }

        prevTriggerState = triggerPressed;
    }
}
