using UnityEngine;

public class Attack : BaseItem
{
    private float flySpeed = 0;
    
    protected bool isFlying = false;
    
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
        if (other.gameObject.layer == LayerMask.NameToLayer("Bubble"))
        {
            return;
        }
        Destroy(gameObject);
    }
}
