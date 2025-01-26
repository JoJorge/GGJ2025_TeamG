using UnityEngine;

public class BubbleBehavior : MonoBehaviour
{
    public float floatHeight = 0.5f;  // �B�ʪ�����
    public float floatSpeed = 1f;     // �}�B�t��

    private Vector3 startPosition;

    void Start()
    {
        // �O���w�w����l��m
        startPosition = transform.position;
    }

    void Update()
    {
        // �ϥΥ����i�Ӳ��ͤW�U�B�ʮĪG
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        // �]�w�s����m
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
