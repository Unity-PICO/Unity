using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HttpImagePuller : MonoBehaviour
{
    [Header("服务端地址，例如 http://your.server:8000/latest.jpg")]
    public string latestUrl = "http://127.0.0.1:8000/latest.jpg";

    [Header("刷新间隔(秒)")]
    public float interval = 0.1f;

    [Header("目标：二选一")]
    public RawImage targetRawImage;
    public Renderer targetRenderer;

    private Texture2D currentTex;

    void Start() => StartCoroutine(PullLoop());

    IEnumerator PullLoop()
    {
        var wait = new WaitForSeconds(interval);
        while (true)
        {
            string url = latestUrl + "?t=" + Time.realtimeSinceStartup; // 防缓存
            using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url, nonReadable:false))
            {
                req.timeout = 3;
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success)
                {
                    var tex = DownloadHandlerTexture.GetContent(req); // Texture2D
                    // 替换显示
                    if (targetRawImage) targetRawImage.texture = tex;
                    if (targetRenderer) targetRenderer.material.mainTexture = tex;
                    // 释放旧纹理避免内存涨
                    if (currentTex && currentTex != tex) Destroy(currentTex);
                    currentTex = tex;
                }
            }
            yield return wait;
        }
    }
}
