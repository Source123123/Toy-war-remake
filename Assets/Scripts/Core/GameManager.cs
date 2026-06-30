using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        Debug.Log("<color=green>Toy War Remastered: 引擎与 Agent 连通成功，沙盘初始化...</color>");
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        for (int x = 0; x < 20; x++)
        {
            for (int z = 0; z < 20; z++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetParent(transform);
                cube.transform.position = new Vector3(x, 0f, z);
            }
        }
    }
}
