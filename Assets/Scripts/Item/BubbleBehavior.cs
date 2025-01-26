using UnityEngine;

public class BubbleBehavior : MonoBehaviour
{
    public float floatHeight = 0.5f;  // 浮動的高度
    public float floatSpeed = 1f;     // 漂浮速度

    private Vector3 startPosition;

    void Start()
    {
        // 記錄泡泡的初始位置
        startPosition = transform.position;
    }

    void Update()
    {
        // 使用正弦波來產生上下浮動效果
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        // 設定新的位置
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
