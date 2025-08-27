using UnityEngine;
using UnityEngine.SceneManagement; // ��������SceneManagement�����ռ�

public class SceneChanger : MonoBehaviour
{
    // ͨ����ť������ô˷�����sceneName ΪĿ�곡����
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // ���߰�Build Settings�е���ż���
    public void ChangeSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
