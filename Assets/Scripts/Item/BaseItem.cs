using System;
using UnityEngine;

public abstract class BaseItem : MonoBehaviour
{
    private float flySpeed = 0.0f;
    
    private bool isFlying = false;

    private void FixedUpdate()
    {
        if (isFlying)
        {
            transform.Translate(transform.forward * flySpeed * Time.fixedDeltaTime, Space.World);
        }
    }

    public virtual void Fly(Vector3 direction, Vector3 up, float speed)
    {
        transform.rotation.SetLookRotation(direction, up);
        flySpeed = speed;
        isFlying = true;
    }
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }
}
