using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class NativeCheck : MonoBehaviour
{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    const string LIB = "ByteRTCCWrapper";
#elif UNITY_ANDROID
    const string LIB = "ByteRTCCWrapper"; // Android 实际会加载 libByteRTCCWrapper.so
#endif

    [DllImport(LIB)]
    private static extern int /* or the correct signature */ ByteRTC_GetVersion(); // 用一个已知导出的函数名

    void Start()
    {
        Debug.Log($"Is64BitProcess: {Environment.Is64BitProcess}, Platform: {Application.platform}");
        try
        {
            var v = ByteRTC_GetVersion();
            Debug.Log("Load success, version: " + v);
        }
        catch (DllNotFoundException e)
        {
            Debug.LogError("DllNotFoundException: " + e.Message);
        }
        catch (EntryPointNotFoundException e)
        {
            Debug.LogError("EntryPointNotFound: " + e.Message + "（可能签名或导出名不对）");
        }
    }
}