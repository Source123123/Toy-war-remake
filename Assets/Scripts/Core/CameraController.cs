using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("缩放设置 (Zoom)")]
    public float zoomSensitivity = 15f;  // 再次大幅降低敏感度，极其细腻
    public float minZoom = 5f;           // 最近拉到 5，看清建筑细节
    public float maxZoom = 20f;          // 最远拉到 20，纵览全局，但绝不漏出边界
    public float zoomSmoothTime = 0.1f;

    [Header("绝对空气墙限制 (Clamp)")]
    // 制作人，如果你觉得镜头边缘还是能看到太多外面，或者顶部的建筑被切了，
    // 你可以在 Unity 运行的时候，直接在右侧面板修改这四个数字来微调手感！
    public float minX = -15f;  // 镜头最左能跑到哪
    public float maxX = 25f;   // 镜头最右能跑到哪
    public float minZ = -20f;  // 镜头最下能跑到哪
    public float maxZ = 75f;   // 镜头最上能跑到哪 (特意留长，为了给高层主基地预留不被切头的空间)

    private float targetZoom;
    private float zoomVelocity = 0f;

    private Vector3 dragStartPosition;
    private Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        targetZoom = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
    }

    void LateUpdate()
    {
        // 1. 经典右键拖拽平移
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (groundPlane.Raycast(ray, out float entry))
            {
                dragStartPosition = ray.GetPoint(entry);
            }
        }

        if (Input.GetMouseButton(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (groundPlane.Raycast(ray, out float entry))
            {
                Vector3 currentDragPosition = ray.GetPoint(entry);
                Vector3 offset = dragStartPosition - currentDragPosition;
                offset.y = 0; 
                transform.position += offset;
            }
        }

        // 2. 细腻滚轮缩放
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetZoom -= scroll * zoomSensitivity;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetZoom, ref zoomVelocity, zoomSmoothTime);

        // 3. 绝对空气墙锁死
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        transform.position = pos;
    }
}