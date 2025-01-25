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

    public virtual void Fly(float speed)
    {
        flySpeed = speed;
        isFlying = true;
    }
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }
}
