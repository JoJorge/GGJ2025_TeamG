using UnityEngine;

public class BrownianMotion : MonoBehaviour
{
    public float speed = 0.1f; // ����ʪ��t��
    public float changeInterval = 1f; // �����V���ܪ��ɶ����j

    private Vector3 randomDirection; // �H����V
    private float timer; // �Ω�p��

    void Start()
    {
        // ��l���H����V
        GenerateRandomDirection();
    }

    void Update()
    {
        // ��s�p�ɾ�
        timer += Time.deltaTime;

        // �C�j���w�ɶ��ͦ��s���H����V
        if (timer >= changeInterval)
        {
            GenerateRandomDirection();
            timer = 0f; // ���m�p�ɾ�
        }

        // �ھ��H����V���ʪ���
        transform.Translate(randomDirection * speed * Time.deltaTime, Space.World);
    }

    // �ͦ��H����V�A�Ȧb X-Z �����W
    private void GenerateRandomDirection()
    {
        float x = Random.Range(-1f, 1f);
        float z = Random.Range(-1f, 1f);
        randomDirection = new Vector3(x, 0, z).normalized; // �T�O��V�O���V�q
    }
}
