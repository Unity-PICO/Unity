using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VRBookReader : MonoBehaviour
{
    public TextMeshProUGUI pageText;
    public Button nextPageBtn, prevPageBtn;
    public Button closeBtn; // ��Inspector������رհ�ť
    public GameObject uiRoot; // ��Ҫ���ص�UI��������Inspector������

    public GameObject uiShow1; // �������ر�ʱ��ʾ��UI1
    public GameObject uiShow2; // �������ر�ʱ��ʾ��UI2
    public GameObject uiShow3; // �������ر�ʱ��ʾ��UI3
    public GameObject uiShow4;

    public TextAsset bookTextFile; // �������txt�ļ�

    public int charsPerPage = 1000;
    private int currentPage = 0;
    private int totalPages;
    private string fullText;

    private void Start()
    {
        if (bookTextFile != null)
        {
            fullText = bookTextFile.text;
        }
        else
        {
            Debug.LogError("����Inspector��ΪbookTextFile��ֵ��");
            fullText = "";
        }

        totalPages = Mathf.CeilToInt((float)fullText.Length / charsPerPage);
        ShowPage(0);

        nextPageBtn.onClick.AddListener(() => ShowPage(currentPage + 1));
        prevPageBtn.onClick.AddListener(() => ShowPage(currentPage - 1));

        if (closeBtn != null && uiRoot != null)
        {
            closeBtn.onClick.AddListener(OnCloseBtnClick);
        }
    }

    private void OnCloseBtnClick()
    {
        uiRoot.SetActive(false);
        if (uiShow1 != null) uiShow1.SetActive(true);
        if (uiShow2 != null) uiShow2.SetActive(true);
        if (uiShow3 != null) uiShow3.SetActive(true);
        if (uiShow4 != null) uiShow4.SetActive(true);

    }

    void ShowPage(int page)
    {
        page = Mathf.Clamp(page, 0, totalPages - 1);
        currentPage = page;

        int start = page * charsPerPage;
        int end = Mathf.Min(fullText.Length, start + charsPerPage);
        pageText.text = fullText.Substring(start, end - start);
    }
}
