using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using Pico.Platform;
using Pico.Platform.Models;
using TMPro;

public class PicoSpeechTriggerRecognizer : MonoBehaviour
{
    public TMP_Text uiPromptText; // 用于提示“开始识别 / 结束识别 / 错误等”
    public TMP_Text resultText;   // 展示识别文本

    private bool isRecognizing = false;
    private bool triggerPressedLastFrame = false;

    void Start()
    {
        // 初始化语音识别引擎
        var initResult = SpeechService.InitAsrEngine();
        Debug.Log($"[Speech] InitAsrEngine: {initResult}");

        // 设置回调
        SpeechService.SetOnAsrResultCallback(OnAsrResult);
        SpeechService.SetOnSpeechErrorCallback(OnAsrError);

        ShowPrompt("按下扳机键开始语音识别");
    }

    void Update()
    {
        bool triggerPressed = false;
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed))
        {
            triggerPressed = pressed;
        }

        if (triggerPressed && !triggerPressedLastFrame)
        {
            ToggleSpeech();
        }

        triggerPressedLastFrame = triggerPressed;
    }

    void ToggleSpeech()
    {
        if (!isRecognizing)
        {
            int result = SpeechService.StartAsr(true, true, 10);
            Debug.Log($"[Speech] StartAsr: {result}");
            isRecognizing = true;
            ShowPrompt("🎙️ 开始语音识别，请讲话...");
        }
        else
        {
            SpeechService.StopAsr();
            Debug.Log("[Speech] StopAsr called");
            isRecognizing = false;
            ShowPrompt("🛑 已停止识别，等待结果...");
        }
    }

    void OnAsrError(Message<SpeechError> msg)
{
    Debug.LogError($"[Speech] 识别出错，原始信息: {msg.Data}");
    ShowPrompt("❌ 识别发生错误（请检查权限）");
}

void OnAsrResult(Message<AsrResult> msg)
{
    Debug.Log("[Speech] 收到语音识别结果：");
    Debug.Log(msg.Data.ToString()); // 打印整个结构体内容

    // 你可以尝试猜字段名，如 Text、Result 等：
    // string result = msg.Data.Text;
    // resultText.text = result;
}


    void ShowPrompt(string message)
    {
        if (uiPromptText != null)
        {
            uiPromptText.text = message;
        }
    }
}
