using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class SupabaseVRUIBuilder : MonoBehaviour
{
    public Camera xrUICamera;
    public SupabaseClient supabaseClient;
    public TMP_FontAsset fallbackFont;

    private TMP_InputField emailInput;
    private TMP_InputField passwordInput;
    private TMP_Text statusText;
    private TMP_Text queryResultText;

    [ContextMenu("Create UI")]
    void CreateUI()
    {
        // Canvas
        GameObject canvasGO = new("SupabaseCanvas", typeof(Canvas), typeof(CanvasScaler));
        canvasGO.layer = LayerMask.NameToLayer("UI");
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = xrUICamera;
        canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1200, 1000);
        canvasGO.transform.position = xrUICamera.transform.position + xrUICamera.transform.forward * 2f;
        canvasGO.transform.rotation = xrUICamera.transform.rotation;
        canvasGO.transform.localScale = Vector3.one * 0.001f;
        canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1200, 1000);

        // ��� Tracked Device Graphic Raycaster
        if (canvasGO.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
        {
            canvasGO.AddComponent<TrackedDeviceGraphicRaycaster>();
        }

        // �ȴ�������
        GameObject bg = CreateUIObject("Background", canvasGO.transform);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color32(20, 20, 30, 220);
        bgImage.raycastTarget = false;

        // �ٴ���Panel
        GameObject panelLogin = CreatePanel(canvasGO.transform, "Panel_Login");
        panelLogin.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 200);
        GameObject panelQuery = CreatePanel(canvasGO.transform, "Panel_Query");
        panelQuery.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -250); ;

        // Panel_Login
        AddText(panelLogin.transform, "��¼ HIT-COZE", 36, TextAlignmentOptions.Center, FontStyles.Bold);

        AddText(panelLogin.transform, "���䣺", 24);
        emailInput = AddInputField(panelLogin.transform, "EmailInput");

        AddText(panelLogin.transform, "���룺", 24);
        passwordInput = AddInputField(panelLogin.transform, "PasswordInput", true);

        Button loginBtn = AddButton(panelLogin.transform, "��¼");
        // ΪButton���XR Poke Follow Affordance�ű���������followTransform
        var loginPokeAffordance = loginBtn.gameObject.GetComponent<XRPokeFollowAffordance>();
        if (loginPokeAffordance == null)
        {
            loginPokeAffordance = loginBtn.gameObject.AddComponent<XRPokeFollowAffordance>();
        }
        loginPokeAffordance.pokeFollowTransform = loginBtn.GetComponent<RectTransform>();
        loginPokeAffordance.clampToMaxDistance = true;
        loginPokeAffordance.maxDistance = 20f;
        // ��¼��ť�ص�
        loginBtn.onClick.AddListener(() =>
        {
            Debug.Log("��¼��ť�����");
            statusText.text = "������֤...";
            string email = emailInput.text;
            string pwd = passwordInput.text;
            statusText.text = "���ڵ�¼...";
            supabaseClient.StartCoroutine(
                supabaseClient.SignInWithPassword(email, pwd,
                    (result) => {
                        statusText.text = "��¼�ɹ���";
                        panelLogin.SetActive(false);
                        panelQuery.SetActive(true);
                    },
                    (error) => statusText.text = $"��¼ʧ��: {error.msg}")
            );
        });

        statusText = AddText(panelLogin.transform, "״̬���ȴ�����", 20, TextAlignmentOptions.Center, FontStyles.Italic);

        // Panel_Query
        Button queryBtn = AddButton(panelQuery.transform, "��ѯ�û���");
        // ΪButton���XR Poke Follow Affordance�ű�
        var queryPokeAffordance = queryBtn.gameObject.GetComponent<XRPokeFollowAffordance>();
        if (queryPokeAffordance == null)
        {
            queryPokeAffordance = queryBtn.gameObject.AddComponent<XRPokeFollowAffordance>();
        }
        queryPokeAffordance.pokeFollowTransform = queryBtn.GetComponent<RectTransform>();
        queryPokeAffordance.clampToMaxDistance = true;
        queryPokeAffordance.maxDistance = 20f;
        queryBtn.onClick.AddListener(() =>
        {
            statusText.text = "���ڲ�ѯ...";
            supabaseClient.StartCoroutine(
                supabaseClient.QueryTable("users", "?select=*",
                    (result) =>
                    {
                        statusText.text = "��ѯ�ɹ���";
                        queryResultText.text = result;
                    },
                    (error) => statusText.text = $"��ѯʧ��: {error.msg}")
            );
        });

        queryResultText = AddText(panelQuery.transform, "��ѯ�������������ʾ", 22, TextAlignmentOptions.TopLeft, FontStyles.Normal);
        queryResultText.rectTransform.sizeDelta = new Vector2(900, 200);

        // ��ʼ��ʱֻ��ʾ��¼��壬���ز�ѯ���
        panelLogin.SetActive(true);
        panelQuery.SetActive(false);
    }

    // ==== ���߷��� ====

    GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    GameObject CreatePanel(Transform parent, string name)
    {
        GameObject panel = CreateUIObject(name, parent);
        Image img = panel.AddComponent<Image>();
        img.color = new Color32(255, 255, 255, 255);
        img.raycastTarget = false;
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(1000, 600);
        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        layout.spacing = 15;
        layout.padding = new RectOffset(40, 40, 40, 40);
        panel.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return panel;
    }

    TMP_Text AddText(Transform parent, string text, int size, TextAlignmentOptions align = TextAlignmentOptions.Left, FontStyles style = FontStyles.Normal)
    {
        GameObject go = CreateUIObject("TMP_Text", parent);
        TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.alignment = align;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.color = Color.black;
        if (fallbackFont) tmp.font = fallbackFont;
        RectTransform rt = tmp.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(800, 60);
        return tmp;
    }

    TMP_InputField AddInputField(Transform parent, string name, bool isPassword = false)
    {
        GameObject inputGO = CreateUIObject(name, parent);
        Image bg = inputGO.AddComponent<Image>();
        bg.color = new Color32(240, 240, 240, 255);

        RectTransform rt = inputGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(800, 70);

        // ��� LayoutElement��ȷ������ϵͳ����ȷ����ռ�
        LayoutElement layout = inputGO.AddComponent<LayoutElement>();
        layout.preferredWidth = 800;
        layout.preferredHeight = 70;
        layout.minWidth = 200;
        layout.minHeight = 30;

        TMP_InputField input = inputGO.AddComponent<TMP_InputField>();

        // Text���
        GameObject textGO = CreateUIObject("Text", inputGO.transform);
        TMP_Text text = textGO.AddComponent<TextMeshProUGUI>();
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Left;
        text.verticalAlignment = VerticalAlignmentOptions.Middle;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.color = Color.black;
        if (fallbackFont) text.font = fallbackFont;
        RectTransform textRT = text.GetComponent<RectTransform>();
        StretchFull(textRT);

        input.textComponent = text;
        input.contentType = isPassword ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;

        // ����ռλ��
        GameObject placeholderGO = CreateUIObject("Placeholder", inputGO.transform);
        TMP_Text placeholder = placeholderGO.AddComponent<TextMeshProUGUI>();
        placeholder.text = isPassword ? "����������" : "����������";
        placeholder.fontSize = 24;
        placeholder.alignment = TextAlignmentOptions.Left;
        placeholder.verticalAlignment = VerticalAlignmentOptions.Middle;
        placeholder.color = new Color32(150, 150, 150, 255);
        if (fallbackFont) placeholder.font = fallbackFont;
        RectTransform placeholderRT = placeholder.GetComponent<RectTransform>();
        StretchFull(placeholderRT);

        input.placeholder = placeholder;

        // ���ù����ɫ
        input.caretColor = new Color32(0, 122, 255, 255);

        // ����ѡ����ɫ
        input.selectionColor = new Color32(200, 220, 255, 255);

        // ��������ַ���
        input.characterLimit = isPassword ? 32 : 64;

        // ���ø��ı�
        input.richText = false;

        return input;
    }

    Button AddButton(Transform parent, string btnText)
    {
        GameObject btnGO = CreateUIObject(btnText + "_Button", parent);
        Image img = btnGO.AddComponent<Image>();
        img.color = new Color32(0, 122, 255, 255);

        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(800, 70);

        // ��� LayoutElement��ȷ������ϵͳ����ȷ����ռ�
        LayoutElement layout = btnGO.AddComponent<LayoutElement>();
        layout.preferredWidth = 800;
        layout.preferredHeight = 70;
        layout.minWidth = 200;
        layout.minHeight = 30;

        Button btn = btnGO.AddComponent<Button>();

        GameObject txtGO = CreateUIObject("Text", btnGO.transform);
        TMP_Text txt = txtGO.AddComponent<TextMeshProUGUI>();
        txt.text = btnText;
        txt.fontSize = 26;
        txt.alignment = TextAlignmentOptions.Center;
        txt.enableWordWrapping = false;
        txt.overflowMode = TextOverflowModes.Overflow;
        txt.color = Color.white;
        if (fallbackFont) txt.font = fallbackFont;

        StretchFull(txt.GetComponent<RectTransform>());

        return btn;
    }

    void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
