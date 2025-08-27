using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VRBookReader : MonoBehaviour
{
    public TextMeshProUGUI pageText;
    public Button nextPageBtn, prevPageBtn;
    public Button closeBtn; // 在Inspector中拖入关闭按钮
    public GameObject uiRoot; // 需要隐藏的UI根对象，在Inspector中拖入

    public GameObject uiShow1; // 新增：关闭时显示的UI1
    public GameObject uiShow2; // 新增：关闭时显示的UI2
    public GameObject uiShow3; // 新增：关闭时显示的UI3
    public GameObject uiShow4;

    public TextAsset bookTextFile; // 拖入你的txt文件

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
            Debug.LogError("请在Inspector中为bookTextFile赋值！");
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
