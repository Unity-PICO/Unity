using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;

public class ControllerDataSender : MonoBehaviour
{
    [Header("服务器配置")]
    public string serverUrl = "http://localhost:8080/controller";
    public float sendInterval = 0.1f; // 发送间隔（秒）
    
    [Header("调试信息")]
    public bool enableDebugLog = true;
    
    [System.Serializable]
    public class ControllerData
    {
        public string controllerId; // "left" 或 "right"
        public float joystickX;
        public float joystickY;
        public float timestamp;
        
        public ControllerData(string id, Vector2 joystick, float time)
        {
            controllerId = id;
            joystickX = joystick.x;
            joystickY = joystick.y;
            timestamp = time;
        }
    }
    
    private InputDevice leftController;
    private InputDevice rightController;
    private bool controllersInitialized = false;
    
    void Start()
    {
        StartCoroutine(InitializeControllers());
        StartCoroutine(SendControllerDataRoutine());
    }
    
    IEnumerator InitializeControllers()
    {
        // 等待XR系统初始化
        while (!XRSettings.isDeviceActive)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        // 查找左右手柄
        var inputDevices = new System.Collections.Generic.List<InputDevice>();
        InputDevices.GetDevices(inputDevices);
        
        foreach (var device in inputDevices)
        {
            if (device.characteristics.HasFlag(InputDeviceCharacteristics.Left) && 
                device.characteristics.HasFlag(InputDeviceCharacteristics.Controller))
            {
                leftController = device;
                if (enableDebugLog)
                    Debug.Log($"找到左手柄: {device.name}");
            }
            else if (device.characteristics.HasFlag(InputDeviceCharacteristics.Right) && 
                     device.characteristics.HasFlag(InputDeviceCharacteristics.Controller))
            {
                rightController = device;
                if (enableDebugLog)
                    Debug.Log($"找到右手柄: {device.name}");
            }
        }
        
        controllersInitialized = true;
        
        if (enableDebugLog)
        {
            Debug.Log($"手柄初始化完成 - 左手柄: {leftController.isValid}, 右手柄: {rightController.isValid}");
        }
    }
    
    IEnumerator SendControllerDataRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(sendInterval);
            
            if (controllersInitialized)
            {
                // 发送左手柄数据
                if (leftController.isValid)
                {
                    Vector2 leftJoystick = GetJoystickInput(leftController);
                    if (leftJoystick.magnitude > 0.01f) // 只在有输入时发送
                    {
                        StartCoroutine(SendControllerData("left", leftJoystick));
                    }
                }
                
                // 发送右手柄数据
                if (rightController.isValid)
                {
                    Vector2 rightJoystick = GetJoystickInput(rightController);
                    if (rightJoystick.magnitude > 0.01f) // 只在有输入时发送
                    {
                        StartCoroutine(SendControllerData("right", rightJoystick));
                    }
                }
            }
        }
    }
    
    Vector2 GetJoystickInput(InputDevice controller)
    {
        Vector2 joystickValue = Vector2.zero;
        
        // 尝试获取主摇杆输入
        if (controller.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 primary2D))
        {
            joystickValue = primary2D;
        }
        // 如果主摇杆没有输入，尝试获取副摇杆输入
        else if (controller.TryGetFeatureValue(CommonUsages.secondary2DAxis, out Vector2 secondary2D))
        {
            joystickValue = secondary2D;
        }
        
        return joystickValue;
    }
    
    IEnumerator SendControllerData(string controllerId, Vector2 joystickInput)
    {
        ControllerData data = new ControllerData(controllerId, joystickInput, Time.time);
        string jsonData = JsonUtility.ToJson(data);
        
        if (enableDebugLog)
        {
            Debug.Log($"发送{controllerId}手柄数据: X={joystickInput.x:F3}, Y={joystickInput.y:F3}");
        }
        
        using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                if (enableDebugLog)
                {
                    Debug.LogError($"发送{controllerId}手柄数据失败: {request.error}");
                }
            }
            else if (enableDebugLog)
            {
                Debug.Log($"{controllerId}手柄数据发送成功");
            }
        }
    }
    
    // 手动测试方法（在没有VR设备时使用）
    void Update()
    {
        // 如果没有VR设备，使用键盘模拟
        if (!XRSettings.isDeviceActive)
        {
            // 使用WASD模拟左手柄（修复：使用GetKey替代GetAxis避免平滑过渡）
            Vector2 leftSimulated = Vector2.zero;
            if (Input.GetKey(KeyCode.W)) leftSimulated.y = 1;
            if (Input.GetKey(KeyCode.S)) leftSimulated.y = -1;
            if (Input.GetKey(KeyCode.A)) leftSimulated.x = -1;
            if (Input.GetKey(KeyCode.D)) leftSimulated.x = 1;
            
            // 使用方向键模拟右手柄
            Vector2 rightSimulated = Vector2.zero;
            if (Input.GetKey(KeyCode.LeftArrow)) rightSimulated.x = -1;
            if (Input.GetKey(KeyCode.RightArrow)) rightSimulated.x = 1;
            if (Input.GetKey(KeyCode.DownArrow)) rightSimulated.y = -1;
            if (Input.GetKey(KeyCode.UpArrow)) rightSimulated.y = 1;
            
            // 发送模拟数据（只在有按键输入时发送）
            if (leftSimulated.magnitude > 0.01f)
            {
                StartCoroutine(SendControllerData("left", leftSimulated));
            }
            
            if (rightSimulated.magnitude > 0.01f)
            {
                StartCoroutine(SendControllerData("right", rightSimulated));
            }
        }
    }
}