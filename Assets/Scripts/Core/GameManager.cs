using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int buildableWidth = 40;     // X轴：40
    public int buildableLength = 100;   // Z轴：100

    void Start()
    {
        GenerateWorld();
    }

    void GenerateWorld()
    {
        // 1. 终极障眼法：生成一块 500x500 的超大深绿色“背景布”
        GameObject bgPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        // Plane的默认大小是 10x10，缩放50倍就是 500x500，稍微下沉一点防止和地砖穿模
        bgPlane.transform.localScale = new Vector3(50f, 1f, 50f); 
        // 把它放在 40x100 网格的正中心正下方
        bgPlane.transform.position = new Vector3(buildableWidth / 2f, -0.6f, buildableLength / 2f);
        bgPlane.transform.parent = this.transform;
        
        Renderer bgRenderer = bgPlane.GetComponent<Renderer>();
        bgRenderer.material.color = new Color(0.2f, 0.35f, 0.2f); // 暗淡的城外荒地色

        // 2. 生成核心战区：只生成 40x100 的浅绿色网格地砖
        for (int x = 0; x < buildableWidth; x++)
        {
            for (int z = 0; z < buildableLength; z++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = new Vector3(x, -0.5f, z);
                cube.transform.parent = this.transform;

                Renderer r = cube.GetComponent<Renderer>();
                r.material.color = new Color(0.5f, 0.75f, 0.4f); // 明亮的内城草地色
            }
        }
    }
}