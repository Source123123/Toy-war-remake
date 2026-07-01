using UnityEngine;

public class BuildingPlacement : MonoBehaviour
{
    [Header("建筑参数面板")]
    public Vector2 physicalSize = new Vector2(3, 3);
    
    [Tooltip("勾选代表城墙/地雷，开启 0.5 格半步移动模式！")]
    public bool isHalfGridBuilding = false; 
    
    private GameObject previewObject;
    
    // 【架构师核心修复 1】：把数学射线的接收平面也下沉到 -0.5，与真实草地完美对齐！
    private Plane groundPlane = new Plane(Vector3.up, new Vector3(0, -0.5f, 0)); 
    
    private bool isValid = false;
    private float currentMinX = 0f;
    private float currentMinZ = 0f;
    private bool isBuildMode = false;

    void Start()
    {
        previewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(previewObject.GetComponent<Collider>());
        previewObject.name = "Preview_Ghost";
        previewObject.SetActive(false);
    }

    void Update()
    {
        previewObject.transform.localScale = new Vector3(physicalSize.x, 3f, physicalSize.y);

        if (Input.GetKeyDown(KeyCode.B))
        {
            isBuildMode = !isBuildMode;
            previewObject.SetActive(isBuildMode);
            GameManager.Instance.ToggleGridDisplay(isBuildMode);
        }

        if (!isBuildMode) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            float step = isHalfGridBuilding ? GameManager.SUB_GRID_SIZE : 1.0f;

            bool isWidthOddInSteps = Mathf.RoundToInt(physicalSize.x / step) % 2 != 0;
            bool isLengthOddInSteps = Mathf.RoundToInt(physicalSize.y / step) % 2 != 0;

            float offsetX = isWidthOddInSteps ? (step / 2f) : 0f;
            float offsetZ = isLengthOddInSteps ? (step / 2f) : 0f;

            float snapX = Mathf.Round((hitPoint.x - offsetX) / step) * step + offsetX;
            float snapZ = Mathf.Round((hitPoint.z - offsetZ) / step) * step + offsetZ;

            // 【架构师核心修复 2】：建筑 Y 轴高度从 -0.5 米（地基表面）起算，绝对不再浮空！
            float groundLevel = -0.5f; 
            float centerY = groundLevel + (previewObject.transform.localScale.y / 2f);
            
            previewObject.transform.position = new Vector3(snapX, centerY, snapZ);
            
            currentMinX = snapX - physicalSize.x / 2f;
            currentMinZ = snapZ - physicalSize.y / 2f;

            isValid = GameManager.Instance.IsAreaAvailablePhysical(currentMinX, currentMinZ, physicalSize.x, physicalSize.y);

            Renderer r = previewObject.GetComponent<Renderer>();
            r.material.color = isValid ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
        }

        if (Input.GetMouseButtonDown(0) && isValid)
        {
            PlaceBuilding();
        }
    }

    void PlaceBuilding()
    {
        GameObject newBuilding = GameObject.CreatePrimitive(PrimitiveType.Cube);
        newBuilding.transform.position = previewObject.transform.position;
        newBuilding.transform.localScale = previewObject.transform.localScale;
        newBuilding.GetComponent<Renderer>().material.color = new Color(0.7f, 0.7f, 0.7f); 
        newBuilding.transform.parent = null; 

        GameManager.Instance.MarkAreaOccupiedPhysical(currentMinX, currentMinZ, physicalSize.x, physicalSize.y);
    }
}