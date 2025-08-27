using MotionverseSDK;
using UnityEngine;
using UnityEngine.UI; // 用于Button组件
using System.Collections; // 用于协程

public class Samples : MonoBehaviour
{
    public Player player;
    public Button asrButton; // 在Inspector中拖入按钮

    private Coroutine textDriveCoroutine;
    private bool isButtonLocked = false; // 按钮锁定标志

    private void Start()
    {
        player.voiceVolume = 0; // 设置音量为0
        player.OnPlayComplete += OnPlayComplete;
        player.OnPlayStart += OnPlayStart;
        player.OnPlayError += OnPlayError;

        // 绑定按钮点击事件
        if (asrButton != null)
        {
            asrButton.onClick.AddListener(OnAsrButtonClick);
        }
    }

    // 按钮点击后每三秒调用一次OnTextDrive("Hello")
    private void OnAsrButtonClick()
    {
        if (isButtonLocked)
        {
            // 已锁定时，重复点击则断开（停止循环）
            if (textDriveCoroutine != null)
            {
                StopCoroutine(textDriveCoroutine);
                textDriveCoroutine = null;
                isButtonLocked = false;
                Debug.Log("已断开，停止循环调用。");
            }
            return;
        }

        if (textDriveCoroutine == null)
        {
            textDriveCoroutine = StartCoroutine(TextDriveLoop());
            isButtonLocked = true;
            Debug.Log("开始调用数字人动作");
        }
    }

    private IEnumerator TextDriveLoop()
    {
        while (true)
        {
            OnTextDrive("Hello,are you ready for the English speaking test now?");
            yield return new WaitForSeconds(3f);
        }
    }

    public void OnTextDrive(string text)
    {
        DriveTask driveTask = new DriveTask() { player = player, text = text };
        TextDrive.GetDrive(driveTask, false);
    }

    public void OnAudioUrlDrive(string url)
    {
        DriveTask driveTask = new DriveTask() { player = player, text = url };
        AudioUrlDrive.GetDrive(driveTask, false);
    }

    public void OnNLPDrive(string text)
    {
        DriveTask driveTask = new DriveTask() { player = player, text = text };
        NLPDrive.GetDrive(driveTask, false);
    }

    public void OnTest()
    {
        player.StopPlay();
    }
    public void OnPlayComplete()
    {
        Debug.Log("OnPlayComplete");
    }
    public void OnPlayStart()
    {
        Debug.Log("OnPlayStart");
    }

    private void OnDestroy()
    {
        player.OnPlayComplete -= OnPlayComplete;
        player.OnPlayStart -= OnPlayStart;
        player.OnPlayError -= OnPlayError;

        // 解绑按钮事件
        if (asrButton != null)
        {
            asrButton.onClick.RemoveListener(OnAsrButtonClick);
        }
        // 停止协程
        if (textDriveCoroutine != null)
        {
            StopCoroutine(textDriveCoroutine);
            textDriveCoroutine = null;
            isButtonLocked = false;
        }
    }
    public void OnPlayError(string msg)
    {
        Debug.LogError($"OnPlayError:{msg}");
    }
}
