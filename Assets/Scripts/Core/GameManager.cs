using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("沙盘数据配置")]
    public int buildableWidth = 40;     
    public int buildableLength = 100;   

    private int[,] gridData;
    private GameObject gridPlane; // 真正的物理网格地毯

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gridData = new int[buildableWidth, buildableLength];
    }

    void Start()
    {
        GenerateOptimizedWorld();
    }

    void GenerateOptimizedWorld()
    {
        float centerX = (buildableWidth - 1) / 2f;
        float centerZ = (buildableLength - 1) / 2f;

        // 1. 生成大外围背景
        GameObject bgPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        bgPlane.transform.localScale = new Vector3(50f, 1f, 50f); 
        bgPlane.transform.position = new Vector3(centerX, -0.6f, centerZ);
        bgPlane.transform.parent = this.transform;
        bgPlane.GetComponent<Renderer>().material.color = new Color(0.2f, 0.35f, 0.2f); 

        // 2. 生成战区草地
        GameObject tacticalPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        tacticalPlane.transform.localScale = new Vector3(buildableWidth / 10f, 1f, buildableLength / 10f);
        tacticalPlane.transform.position = new Vector3(centerX, -0.5f, centerZ);
        tacticalPlane.transform.parent = this.transform;
        tacticalPlane.GetComponent<Renderer>().material.color = new Color(0.5f, 0.75f, 0.4f); 

        // 3. 【架构师黑科技】用代码自动画一张透明的网格地毯！
        CreatePhysicalGridCarpet(centerX, centerZ);
    }

    void CreatePhysicalGridCarpet(float centerX, float centerZ)
    {
        gridPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Destroy(gridPlane.GetComponent<Collider>()); // 去掉碰撞体
        gridPlane.transform.localScale = new Vector3(buildableWidth / 10f, 1f, buildableLength / 10f);
        // 关键：贴在草地上面一点点 (-0.49f)，大楼下面，实现完美的遮挡！
        gridPlane.transform.position = new Vector3(centerX, -0.49f, centerZ);
        gridPlane.transform.parent = this.transform;

        // 在内存里画网格图片
        int pixelsPerCell = 16;
        int texWidth = buildableWidth * pixelsPerCell;
        int texHeight = buildableLength * pixelsPerCell;
        Texture2D gridTex = new Texture2D(texWidth, texHeight);
        gridTex.filterMode = FilterMode.Bilinear;

        Color clearColor = new Color(0, 0, 0, 0);       // 完全透明
        Color lineColor = new Color(1f, 1f, 1f, 0.25f); // 淡淡的白线

        Color[] pixels = new Color[texWidth * texHeight];
        for (int y = 0; y < texHeight; y++)
        {
            for (int x = 0; x < texWidth; x++)
            {
                // 每隔 16 像素画一条线
                bool isLine = (x % pixelsPerCell == 0 || y % pixelsPerCell == 0);
                pixels[y * texWidth + x] = isLine ? lineColor : clearColor;
            }
        }
        gridTex.SetPixels(pixels);
        gridTex.Apply();

        // 贴到地毯上
        Material gridMat = new Material(Shader.Find("Unlit/Transparent"));
        gridMat.mainTexture = gridTex;
        gridPlane.GetComponent<Renderer>().material = gridMat;

        // 游戏一开始，默认关闭网格！
        gridPlane.SetActive(false);
    }

    // --- 对外提供的建造模式开关接口 ---
    public void ToggleGridDisplay(bool show)
    {
        if(gridPlane != null) gridPlane.SetActive(show);
    }

    // --- 数据校验接口保持不变 ---
    public bool IsAreaAvailable(int startX, int startZ, int width, int length)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                int checkX = startX + x;
                int checkZ = startZ + z;
                
                if (checkX < 0 || checkX >= buildableWidth || checkZ < 0 || checkZ >= buildableLength)
                    return false;
                
                if (gridData[checkX, checkZ] != 0)
                    return false;
            }
        }
        return true;
    }

    public void MarkAreaOccupied(int startX, int startZ, int width, int length)
    {
        for (int x = 0; x < width; x++)
            for (int z = 0; z < length; z++)
                gridData[startX + x, startZ + z] = 1;
    }
}