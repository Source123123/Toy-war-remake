using UnityEngine;

public class BuildingPlacement : MonoBehaviour
{
    [Header("建筑参数面板")]
    public Vector2Int buildingSize = new Vector2Int(3, 3);
    
    private GameObject previewObject;
    private Plane groundPlane = new Plane(Vector3.up, Vector3.zero); 
    
    private bool isValid = false;
    private int currentStartX = 0;
    private int currentStartZ = 0;

    // 核心状态机：是否处于建造模式
    private bool isBuildMode = false;

    void Start()
    {
        previewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(previewObject.GetComponent<Collider>());
        previewObject.name = "Preview_Ghost";
        previewObject.transform.localScale = new Vector3(buildingSize.x, 3f, buildingSize.y);
        
        // 游戏开始时，默认隐藏手里的建筑
        previewObject.SetActive(false);
    }

    void Update()
    {
        // 1. 监听切换按键 (模拟玩家点击 UI 按钮)
        if (Input.GetKeyDown(KeyCode.B))
        {
            isBuildMode = !isBuildMode;
            
            // 同步开关手里的建筑 和 地上的网格
            previewObject.SetActive(isBuildMode);
            GameManager.Instance.ToggleGridDisplay(isBuildMode);
        }

        // 如果不在建造模式，后续逻辑直接掐断，玩家可以安心看风景
        if (!isBuildMode) return;

        // 2. 建造逻辑（保持原有的完美手感）
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            float snapX = (buildingSize.x % 2 == 0) ? Mathf.Round(hitPoint.x - 0.5f) + 0.5f : Mathf.Round(hitPoint.x);
            float snapZ = (buildingSize.y % 2 == 0) ? Mathf.Round(hitPoint.z - 0.5f) + 0.5f : Mathf.Round(hitPoint.z);

            currentStartX = Mathf.RoundToInt(snapX - (buildingSize.x - 1) / 2f);
            currentStartZ = Mathf.RoundToInt(snapZ - (buildingSize.y - 1) / 2f);

            previewObject.transform.position = new Vector3(snapX, previewObject.transform.localScale.y / 2f, snapZ);
            
            isValid = GameManager.Instance.IsAreaAvailable(currentStartX, currentStartZ, buildingSize.x, buildingSize.y);

            Renderer r = previewObject.GetComponent<Renderer>();
            r.material.color = isValid ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
        }

        // 3. 左键确认建造
        if (Input.GetMouseButtonDown(0) && isValid)
        {
            PlaceBuilding();
            
            // 可选体验：造完一栋房子后，自动退出建造模式 (取消注释即可体验)
            // isBuildMode = false;
            // previewObject.SetActive(false);
            // GameManager.Instance.ToggleGridDisplay(false);
        }
    }

    void PlaceBuilding()
    {
        GameObject newBuilding = GameObject.CreatePrimitive(PrimitiveType.Cube);
        newBuilding.transform.position = previewObject.transform.position;
        newBuilding.transform.localScale = previewObject.transform.localScale;
        newBuilding.GetComponent<Renderer>().material.color = new Color(0.7f, 0.7f, 0.7f); 
        newBuilding.transform.parent = null; 

        GameManager.Instance.MarkAreaOccupied(currentStartX, currentStartZ, buildingSize.x, buildingSize.y);
    }
}