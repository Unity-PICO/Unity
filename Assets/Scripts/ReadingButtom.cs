using UnityEngine;
using UnityEngine.UI;

public class ToggleObjectsOnClick : MonoBehaviour
{
    public GameObject objectA;
    public GameObject objectB;
    public GameObject objectC;
    public GameObject objectD;
    public GameObject objectE;

    public Button targetButton;

    void Start()
    {
        if (targetButton != null)
        {
            targetButton.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogWarning("Button not assigned to ToggleObjectsOnClick script.");
        }
    }

    void OnButtonClicked()
    {
        if (objectA != null) objectA.SetActive(false);
        if (objectB != null) objectB.SetActive(false);
        if (objectC != null) objectC.SetActive(false);
        if (objectD != null) objectC.SetActive(true);
        if (objectE != null) objectE.SetActive(false);
    }
}
