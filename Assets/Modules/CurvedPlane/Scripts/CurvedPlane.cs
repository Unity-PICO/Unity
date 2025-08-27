using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[AddComponentMenu("Mesh/Curved Plane")]
public class CurvedPlane : MonoBehaviour
{
    [Header("Plane Generating Settings")]
    [Tooltip("Recreate the plane each FixedUpdate (for dynamic shape).")]
    public bool UseFixedUpdate = true;

    [Header("Plane Mesh Settings")]
    [Tooltip("Enable curve deformation.")]
    public bool useCurving = true;

    [Tooltip("Mesh resolution (triangles count).")]
    [Range(2, 100)]
    public int quality = 40;

    [Tooltip("Use the GameObject's position as center.")]
    public bool defaultCenter = true;
    [Tooltip("Custom center point when DefaultCenter is off.")]
    public Vector3 m_CustomCenter;

    [Tooltip("Horizontal curve strength (normalized).")]
    [Range(0f, 2f)]
    public float m_CurveCoeficientX = 1f;

    [Tooltip("Vertical curve strength (normalized).")]
    [Range(0f, 2f)]
    public float m_CurveCoeficientY = 1f;

    [Header("Plane Render Settings")]
    [Tooltip("Material to apply to the plane.")]
    public Material CustomMaterial;
    [Tooltip("Optional target for reference (unused).")]
    public GameObject m_Target;

    [Header("Plane Size Settings")]
    [Tooltip("Size by texture's aspect ratio.")]
    public bool TexturesSize = false;
    [Tooltip("Scale factor when using texture size.")]
    public float Scale = 1f;
    [Range(1f, 10f)]
    [Tooltip("Plane width if not using texture size.")]
    public float width = 6f;
    [Range(1f, 10f)]
    [Tooltip("Plane height if not using texture size.")]
    public float height = 4f;

    // 私有缓存
    Vector3[] vertices;
    Vector3[] normals;
    Vector2[] uvs;
    int[] triangles;
    Material _material;
    bool _initialized = false;

    private Vector3 Center => defaultCenter ? transform.localPosition : m_CustomCenter;

    public Material material
    {
        get
        {
            if (_material == null)
            {
                _material = CustomMaterial != null
                    ? CustomMaterial
                    : new Material(Shader.Find("Unlit/Texture"));
            }
            return _material;
        }
    }

    void OnEnable()
    {
        CreatePlane();
    }

    void FixedUpdate()
    {
        if (UseFixedUpdate)
            CreatePlane();
    }

    public void UpdatePlane()
    {
        CreatePlane();
    }

    private void CreatePlane()
    {
        // ------- 确保组件存在（RequireComponent 已经自动添加，但再保险） -------
        var renderer = GetComponent<MeshRenderer>();
        if (renderer == null) renderer = gameObject.AddComponent<MeshRenderer>();

        var filter = GetComponent<MeshFilter>();
        if (filter == null) filter = gameObject.AddComponent<MeshFilter>();

        // ------- 只在第一次赋材质，之后保留 VLC 输出来的贴图 -------
        if (!_initialized)
        {
            renderer.material = material;
            _initialized = true;
        }

        // ------- 准备网格数据 -------
        int resX = quality, resY = quality;
        vertices  = new Vector3[resX * resY];
        normals   = new Vector3[resX * resY];
        uvs       = new Vector2[resX * resY];
        triangles = new int[(resX - 1) * (resY - 1) * 6];

        // 根据贴图尺寸或手动设置计算宽高
        float w = width, h = height;
        if (TexturesSize && material.mainTexture != null)
        {
            w = material.mainTexture.width  / 100f * Scale;
            h = material.mainTexture.height / 100f * Scale;
        }

        // 顶点、UV、法线
        for (int y = 0; y < resY; y++)
        {
            float yPos = ((float)y / (resY - 1) - 0.5f) * h;
            for (int x = 0; x < resX; x++)
            {
                float xPos = ((float)x / (resX - 1) - 0.5f) * w;
                Vector3 p = new Vector3(xPos, yPos, 0);
                vertices[x + y * resX] = useCurving
                    ? CalculateParabolicZ(p, w * 0.5f, h * 0.5f)
                    : p;
                normals[x + y * resX] = Vector3.back;
                uvs[x + y * resX] = new Vector2((float)x / (resX - 1), (float)y / (resY - 1));
            }
        }

        // 三角形索引
        int ti = 0;
        for (int y = 0; y < resY - 1; y++)
        {
            for (int x = 0; x < resX - 1; x++)
            {
                int i = x + y * resX;
                triangles[ti++] = i;
                triangles[ti++] = i + resX;
                triangles[ti++] = i + resX + 1;
                triangles[ti++] = i;
                triangles[ti++] = i + resX + 1;
                triangles[ti++] = i + 1;
            }
        }

        // 应用到 Mesh
        var mesh = filter.mesh;
        mesh.Clear();
        mesh.vertices  = vertices;
        mesh.normals   = normals;
        mesh.uv        = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }

    /// <summary>
    /// 抛物线曲面：z = -(coefX * (x/halfW)^2 + coefY * (y/halfH)^2)
    /// </summary>
    private Vector3 CalculateParabolicZ(Vector3 p, float halfW, float halfH)
    {
        float nx = p.x / halfW; // -1..1
        float ny = p.y / halfH; // -1..1
        float z  = -(m_CurveCoeficientX * nx * nx
                   + m_CurveCoeficientY * ny * ny);
        return new Vector3(p.x, p.y, z);
    }
}
