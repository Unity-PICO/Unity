using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class NativeCheck : MonoBehaviour
{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    const string LIB = "ByteRTCCWrapper";
#elif UNITY_ANDROID
    const string LIB = "ByteRTCCWrapper"; // Android ʵ�ʻ���� libByteRTCCWrapper.so
#endif

    [DllImport(LIB)]
    private static extern int /* or the correct signature */ ByteRTC_GetVersion(); // ��һ����֪�����ĺ�����

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
            Debug.LogError("EntryPointNotFound: " + e.Message + "������ǩ���򵼳������ԣ�");
        }
    }
}