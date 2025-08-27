using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace bytertc
{
    public class LoginRTC : MonoBehaviour
    {
        public Text sdkVersionTxt;
        public Text roomIDTxt;

        public InputField roomInput;
        public InputField userIdInput;
        public Button enterRoomBtn;

        public GameObject loginPanel;
        public GameObject videoPreviewPanel;
        public GameObject menuPanel;

        public Button hangUpBtn;
        public Image hangUpImage;

        public Button flipCameraBtn;

        private VideoPreviewController   videoLocalPreviewController;
        private VideoPreviewController[] videoRemotePreviewControllers = new VideoPreviewController[3];

        private Dictionary<string, VideoPreviewController> remoteUsersPreviews = new Dictionary<string, VideoPreviewController>();
        private Dictionary<Image, bool> imageMap = new Dictionary<Image, bool>();
        private string roomID;
        private int currentCameraIndex = 0;

        void Start()
        {
            sdkVersionTxt = transform.Find("Tip/Version")?.GetComponent<Text>();
            roomIDTxt     = transform.Find("Tip/RoomID")?.GetComponent<Text>();
            roomInput     = transform.Find("LoginPanel/RoomID")?.GetComponent<InputField>();
            userIdInput   = transform.Find("LoginPanel/UserID")?.GetComponent<InputField>();
            enterRoomBtn  = transform.Find("LoginPanel/EnterRoomBtn")?.GetComponent<Button>();
            if (enterRoomBtn != null) enterRoomBtn.onClick.AddListener(EnterRoomClick);

            loginPanel        = transform.Find("LoginPanel").gameObject;
            videoPreviewPanel = transform.Find("VideoPreviewPanel").gameObject;

            videoLocalPreviewController = videoPreviewPanel.transform.Find("VideoPreviewLocal")?.GetComponent<VideoPreviewController>();

            VideoPreviewController videoRemotePreviewController1 = videoPreviewPanel.transform.Find("VideoPreviewRemote1")?.GetComponent<VideoPreviewController>();
            VideoPreviewController videoRemotePreviewController2 = videoPreviewPanel.transform.Find("VideoPreviewRemote2")?.GetComponent<VideoPreviewController>();
            VideoPreviewController videoRemotePreviewController3 = videoPreviewPanel.transform.Find("VideoPreviewRemote3")?.GetComponent<VideoPreviewController>();
            videoRemotePreviewControllers[0] = videoRemotePreviewController1;
            videoRemotePreviewControllers[1] = videoRemotePreviewController2;
            videoRemotePreviewControllers[2] = videoRemotePreviewController3;

            menuPanel  = transform.Find("Menu").gameObject;
            hangUpBtn  = transform.Find("Menu/HangUpBtn")?.GetComponent<Button>();
            if (hangUpBtn != null) hangUpBtn.onClick.AddListener(HangUpClick);
            hangUpImage = transform.Find("Menu/HangUpBtn")?.GetComponent<Image>();

            flipCameraBtn = transform.Find("Tip/FlipCameraBtn")?.GetComponent<Button>();
            if (flipCameraBtn != null)
            {
                flipCameraBtn.onClick.AddListener(FlipCameraClick);
                flipCameraBtn.gameObject.SetActive(false);
            }
#if UNITY_ANDROID || UNITY_IOS
            if (flipCameraBtn != null) flipCameraBtn.gameObject.SetActive(true);
#endif

            SwitchScene(0);
            RegisterCallback();
        }

        public void HangUpClick()
        {
            ReleaseEngine();
            SwitchScene(0);
        }

        public void FlipCameraClick()
        {
#if UNITY_ANDROID || UNITY_IOS
            CameraID cameraID = (CameraID)(1 - currentCameraIndex);

            EngineManager.instance.SwitchCamera(cameraID);
            if (currentCameraIndex == 1)
            {
                EngineManager.instance.SetLocalVideoMirroType(MirrorType.kMirrorTypeRenderAndEncoder);
            }
            else
            {
                EngineManager.instance.SetLocalVideoMirroType(MirrorType.kMirrorTypeNone);
            }
            currentCameraIndex = 1 - currentCameraIndex;
#endif
        }

        //Login Scene index =0，View Scene index= 1
        private void SwitchScene(int index)
        {
            switch (index)
            {
                case 0:
                    if (loginPanel)        loginPanel.SetActive(true);
                    if (videoPreviewPanel) videoPreviewPanel.SetActive(false);
                    if (menuPanel)         menuPanel.SetActive(false);
                    if (flipCameraBtn)     flipCameraBtn.gameObject.SetActive(false);
                    if (roomIDTxt)         roomIDTxt.text = "";
                    if (sdkVersionTxt)     sdkVersionTxt.text = "";
                    break;
                case 1:
                    if (loginPanel)        loginPanel.SetActive(false);
                    if (videoPreviewPanel) videoPreviewPanel.SetActive(true);
                    if (menuPanel)         menuPanel.SetActive(true);
#if UNITY_ANDROID || UNITY_IOS
                    if (flipCameraBtn)     flipCameraBtn.gameObject.SetActive(true);
#endif
                    if (roomIDTxt)         roomIDTxt.text = roomID;
                    break;
                default: break;
            }
        }

        public void EnterRoomClick()
        {
            if (roomInput == null || userIdInput == null)
            {
                Debug.LogWarning("roomInput 或 userIdInput 未绑定");
                return;
            }

            if (string.IsNullOrEmpty(roomInput.text))
            {
                Debug.Log("roomID is null");
                return;
            }

            if (string.IsNullOrEmpty(userIdInput.text))
            {
                Debug.Log("userID is empty");
                return;
            }

            SwitchScene(1);
            OnEnterRoom(roomInput.text, userIdInput.text);

            // 更稳健的版本号读取
            if (sdkVersionTxt != null)
            {
                try
                {
                    sdkVersionTxt.text = "VolcEngineRTC_" + EngineManager.instance.GetSDKVersion();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("GetSDKVersion failed: " + e.Message);
                    sdkVersionTxt.text = "VolcEngineRTC_unknown";
                }
            }
        }

        private void OnEnterRoom(string room_id, string user_id)
        {
            // 先赋 roomID，再设置视频 sink，避免使用未赋值的 roomID
            roomID = room_id;
            if (roomIDTxt != null) roomIDTxt.text = roomID;

            EngineManager.instance.CreateEngine();
            EngineManager.instance.CreateRoom(room_id);
            EngineManager.instance.JoinRoom(user_id);
            SetVideoSink(true, user_id, videoLocalPreviewController);
        }

        void ReleaseEngine()
        {
            if (videoLocalPreviewController != null) videoLocalPreviewController.Reset();
            ResetRemotePreview();

            for (int i = 0; i < videoRemotePreviewControllers.Length; i++)
            {
                if (videoRemotePreviewControllers[i] != null)
                {
                    videoRemotePreviewControllers[i].Reset();
                }
            }
            if (EngineManager.instance != null)
            {
                EngineManager.instance.ReleaseEngine();
            }
        }

        void ResetRemotePreview()
        {
            for (int i = 0; i < videoRemotePreviewControllers.Length; i++)
            {
                if (videoRemotePreviewControllers[i] != null)
                    videoRemotePreviewControllers[i].SetActive(false);
            }
            remoteUsersPreviews.Clear();
        }

        private void OnDestroy()
        {
            ReleaseEngine();
        }

        private void RegisterCallback()
        {
            EngineManager.instance.RegisterLocalVideoSinkOnFrameCallback(OnLocalVideoSinkOnFrameEventHandler);
            EngineManager.instance.RegisterRemoteVideoSinkOnFrameCallback(OnRemoteVideoSinkOnFrameEventHandler);
            EngineManager.instance.RegisterUserEnterCallback(UserEnter);
            EngineManager.instance.RegisterUserLeaveCallback(UserLeave);
        }

        void OnLocalVideoSinkOnFrameEventHandler(StreamIndex index, VideoFrame videoFrame)
        {
            if (videoLocalPreviewController != null)
            {
                videoLocalPreviewController.SetYUVFrame(videoFrame);
            }
        }

        void OnRemoteVideoSinkOnFrameEventHandler(RemoteStreamKey key, VideoFrame videoFrame)
        {
            for (int i = 0; i < videoRemotePreviewControllers.Length; i++)
            {
                if (videoRemotePreviewControllers[i] != null &&
                    videoRemotePreviewControllers[i].GetActive() &&
                    videoRemotePreviewControllers[i].GetRemoteStreamKey() == key)
                {
                    videoRemotePreviewControllers[i].SetYUVFrame(videoFrame);
                    break;
                }
            }
        }

        private void UserEnter(string user_id)
        {
            if (remoteUsersPreviews.Count >= 3)
            {
                Debug.LogWarning("ignore add stream event,for users above 4");
                return;
            }
            if (remoteUsersPreviews.ContainsKey(user_id))
            {
                Debug.LogWarning("user id = " + user_id + " is exist");
                return;
            }

            for (int i = 0; i < videoRemotePreviewControllers.Length; i++)
            {
                if (videoRemotePreviewControllers[i] != null && !videoRemotePreviewControllers[i].GetActive())
                {
                    remoteUsersPreviews[user_id] = videoRemotePreviewControllers[i];
                    videoRemotePreviewControllers[i].SetActive(true);

                    SetVideoSink(false, user_id, remoteUsersPreviews[user_id]);
                    break;
                }
            }
        }

        private void UserLeave(string user_id)
        {
            if (remoteUsersPreviews.ContainsKey(user_id))
            {
                VideoPreviewController videoView = remoteUsersPreviews[user_id];
                if (videoView != null) videoView.SetActive(false);
                remoteUsersPreviews.Remove(user_id);
            }
            else
            {
                Debug.LogWarning("ignore user leave event");
            }
        }

        private void SetVideoSink(bool is_local, string user_id, VideoPreviewController videoPreview)
        {
            if (videoPreview == null) return;

            RemoteStreamKey remote_stream_key;
            remote_stream_key.RoomID = roomID;
            remote_stream_key.UserID = user_id;
            remote_stream_key.streamIndex = StreamIndex.kStreamIndexMain;
            videoPreview.SetRemoteStreamKey(remote_stream_key);

            if (is_local)
            {
                EngineManager.instance.GetVideoEngine().SetLocalVideoSink(StreamIndex.kStreamIndexMain, VideoSinkPixelFormat.kI420);
                EngineManager.instance.SetLocalVideoMirroType(MirrorType.kMirrorTypeRenderAndEncoder);
            }
            else
            {
                EngineManager.instance.GetVideoEngine().SetRemoteVideoSink(remote_stream_key, VideoSinkPixelFormat.kI420);
            }
        }

        // =========================
        // 兼容 quickstart 的辅助方法
        // =========================

        // 无参：直接走按钮点击逻辑
        public void AutoJoinRoom()
        {
            EnterRoomClick();
        }

        // 带房间号/用户ID：先填 UI，再触发按钮事件
        public void AutoJoinRoom(string roomId, string userId)
        {
            if (roomInput != null)   roomInput.text   = roomId;
            else                     Debug.LogWarning("LoginRTC.roomInput is null");

            if (userIdInput != null) userIdInput.text = userId;
            else                     Debug.LogWarning("LoginRTC.userIdInput is null");

            if (enterRoomBtn != null) enterRoomBtn.onClick.Invoke();
            else                      EnterRoomClick();
        }
    }
}
