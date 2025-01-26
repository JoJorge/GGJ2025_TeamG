using UnityEngine;

public class BrownianMotion : MonoBehaviour
{
    public float speed = 0.1f; // 控制移動的速度
    public float changeInterval = 1f; // 控制方向改變的時間間隔

    private Vector3 randomDirection; // 隨機方向
    private float timer; // 用於計時

    void Start()
    {
        // 初始化隨機方向
        GenerateRandomDirection();
    }

    void Update()
    {
        // 更新計時器
        timer += Time.deltaTime;

        // 每隔指定時間生成新的隨機方向
        if (timer >= changeInterval)
        {
            GenerateRandomDirection();
            timer = 0f; // 重置計時器
        }

        // 根據隨機方向移動物件
        transform.Translate(randomDirection * speed * Time.deltaTime, Space.World);
    }

    // 生成隨機方向，僅在 X-Z 平面上
    private void GenerateRandomDirection()
    {
        float x = Random.Range(-1f, 1f);
        float z = Random.Range(-1f, 1f);
        randomDirection = new Vector3(x, 0, z).normalized; // 確保方向是單位向量
    }
}
