using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("物理沙盘尺寸")]
    public int buildableWidth = 40;     
    public int buildableLength = 100;   

    // 亚网格核心：每个逻辑格子占据物理世界的 0.5 米
    public const float SUB_GRID_SIZE = 0.5f;
    
    // 数据字典容量翻倍：80 x 200
    private int[,] gridData;
    private GameObject gridPlane;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gridData = new int[Mathf.RoundToInt(buildableWidth / SUB_GRID_SIZE), Mathf.RoundToInt(buildableLength / SUB_GRID_SIZE)];
    }

    void Start()
    {
        GenerateOptimizedWorld();
    }

    void GenerateOptimizedWorld()
    {
        float centerX = (buildableWidth - 1) / 2f;
        float centerZ = (buildableLength - 1) / 2f;

        // 1. 生成外围背景
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

        // 3. 画出精美的“双层网格”物理地毯
        CreatePhysicalGridCarpet(centerX, centerZ);
    }

    void CreatePhysicalGridCarpet(float centerX, float centerZ)
    {
        gridPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Destroy(gridPlane.GetComponent<Collider>());
        gridPlane.transform.localScale = new Vector3(buildableWidth / 10f, 1f, buildableLength / 10f);
        gridPlane.transform.position = new Vector3(centerX, -0.49f, centerZ);
        gridPlane.transform.parent = this.transform;

        int pixelsPerCell = 16; // 1米 = 16像素
        int texWidth = buildableWidth * pixelsPerCell;
        int texHeight = buildableLength * pixelsPerCell;
        Texture2D gridTex = new Texture2D(texWidth, texHeight);
        gridTex.filterMode = FilterMode.Bilinear;

        Color clearColor = new Color(0, 0, 0, 0);
        Color mainLineColor = new Color(1f, 1f, 1f, 0.3f);  // 主网格：较亮的粗线 (1x1)
        Color subLineColor = new Color(1f, 1f, 1f, 0.08f);  // 亚网格：非常暗的细线 (0.5x0.5)

        Color[] pixels = new Color[texWidth * texHeight];
        for (int y = 0; y < texHeight; y++)
        {
            for (int x = 0; x < texWidth; x++)
            {
                bool isMainLine = (x % 16 == 0 || y % 16 == 0);
                bool isSubLine = (x % 8 == 0 || y % 8 == 0);

                if (isMainLine) pixels[y * texWidth + x] = mainLineColor;
                else if (isSubLine) pixels[y * texWidth + x] = subLineColor;
                else pixels[y * texWidth + x] = clearColor;
            }
        }
        gridTex.SetPixels(pixels);
        gridTex.Apply();

        Material gridMat = new Material(Shader.Find("Unlit/Transparent"));
        gridMat.mainTexture = gridTex;
        gridPlane.GetComponent<Renderer>().material = gridMat;
        gridPlane.SetActive(false);
    }

    public void ToggleGridDisplay(bool show)
    {
        if(gridPlane != null) gridPlane.SetActive(show);
    }

    // --- 基于 0.5 米亚网格的精确校验算法 ---
    public bool IsAreaAvailablePhysical(float startX, float startZ, float width, float length)
    {
        int subStartX = Mathf.RoundToInt(startX / SUB_GRID_SIZE);
        int subStartZ = Mathf.RoundToInt(startZ / SUB_GRID_SIZE);
        int subWidth = Mathf.RoundToInt(width / SUB_GRID_SIZE);
        int subLength = Mathf.RoundToInt(length / SUB_GRID_SIZE);

        for (int x = 0; x < subWidth; x++)
        {
            for (int z = 0; z < subLength; z++)
            {
                int checkX = subStartX + x;
                int checkZ = subStartZ + z;
                
                if (checkX < 0 || checkX >= gridData.GetLength(0) || checkZ < 0 || checkZ >= gridData.GetLength(1))
                    return false;
                
                if (gridData[checkX, checkZ] != 0)
                    return false;
            }
        }
        return true;
    }

    public void MarkAreaOccupiedPhysical(float startX, float startZ, float width, float length)
    {
        int subStartX = Mathf.RoundToInt(startX / SUB_GRID_SIZE);
        int subStartZ = Mathf.RoundToInt(startZ / SUB_GRID_SIZE);
        int subWidth = Mathf.RoundToInt(width / SUB_GRID_SIZE);
        int subLength = Mathf.RoundToInt(length / SUB_GRID_SIZE);

        for (int x = 0; x < subWidth; x++)
            for (int z = 0; z < subLength; z++)
                gridData[subStartX + x, subStartZ + z] = 1;
    }
}