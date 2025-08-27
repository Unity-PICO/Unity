using UnityEngine;

public class MicRecorder : MonoBehaviour
{
    public int durationSeconds = 5;       // 录音时长（秒）
    public int sampleRate = 16000;        // 采样率
    private AudioClip recordedClip;
    private bool isRecording = false;

    void Start()
    {
        Debug.Log("点击空格开始录音...");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isRecording)
        {
            StartCoroutine(StartRecording());
        }
    }

    System.Collections.IEnumerator StartRecording()
    {
        isRecording = true;

        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("未检测到麦克风设备！");
            yield break;
        }

        string deviceName = Microphone.devices[0];
        Debug.Log("开始录音... 使用设备：" + deviceName);

        recordedClip = Microphone.Start(deviceName, false, durationSeconds, sampleRate);

        yield return new WaitForSeconds(durationSeconds);

        Microphone.End(deviceName);
        Debug.Log("录音结束，开始播放...");
        PlayRecordedAudio();
        isRecording = false;
    }

    void PlayRecordedAudio()
    {
        if (recordedClip != null)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.clip = recordedClip;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("没有录到音频。");
        }
    }
}
