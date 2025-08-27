using UnityEngine;
using UnityEngine.SceneManagement; // 必须引入SceneManagement命名空间

public class SceneChanger : MonoBehaviour
{
    // 通过按钮点击调用此方法，sceneName 为目标场景名
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // 或者按Build Settings中的序号加载
    public void ChangeSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
