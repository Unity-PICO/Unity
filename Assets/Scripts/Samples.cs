using MotionverseSDK;
using UnityEngine;
using UnityEngine.UI; // ����Button���
using System.Collections; // ����Э��

public class Samples : MonoBehaviour
{
    public Player player;
    public Button asrButton; // ��Inspector�����밴ť

    private Coroutine textDriveCoroutine;
    private bool isButtonLocked = false; // ��ť������־

    private void Start()
    {
        player.voiceVolume = 0; // ��������Ϊ0
        player.OnPlayComplete += OnPlayComplete;
        player.OnPlayStart += OnPlayStart;
        player.OnPlayError += OnPlayError;

        // �󶨰�ť����¼�
        if (asrButton != null)
        {
            asrButton.onClick.AddListener(OnAsrButtonClick);
        }
    }

    // ��ť�����ÿ�������һ��OnTextDrive("Hello")
    private void OnAsrButtonClick()
    {
        if (isButtonLocked)
        {
            // ������ʱ���ظ������Ͽ���ֹͣѭ����
            if (textDriveCoroutine != null)
            {
                StopCoroutine(textDriveCoroutine);
                textDriveCoroutine = null;
                isButtonLocked = false;
                Debug.Log("�ѶϿ���ֹͣѭ�����á�");
            }
            return;
        }

        if (textDriveCoroutine == null)
        {
            textDriveCoroutine = StartCoroutine(TextDriveLoop());
            isButtonLocked = true;
            Debug.Log("��ʼ���������˶���");
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

        // ���ť�¼�
        if (asrButton != null)
        {
            asrButton.onClick.RemoveListener(OnAsrButtonClick);
        }
        // ֹͣЭ��
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
