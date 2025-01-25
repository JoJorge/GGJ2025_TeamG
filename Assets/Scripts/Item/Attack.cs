using Fusion;
using UnityEngine;

public class Attack : BaseItem
{
    [Networked]
    private float flySpeed { get; set; } = 0;
    
    public override void FixedUpdateNetwork()
    {
        if (isFlying)
        {
            transform.Translate(transform.forward * flySpeed * Runner.DeltaTime, Space.World);
        }
    }

    public virtual void Fly(float speed)
    {
        flySpeed = speed;
        isFlying = true;
    }
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!Runner.IsServer)
        {
            return;
        }
        Destroy(gameObject);
    }
}
