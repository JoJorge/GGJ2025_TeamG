using System;
using UnityEngine;

public abstract class FieldItem : BaseItem
{
    [SerializeField]
    private float rotateSpeed = 10;
    
    protected abstract void CustomEffect(BasePlayer player);

    private void FixedUpdate()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            var player = other.GetComponent<BasePlayer>();
            CustomEffect(player);
            Destroy(gameObject);
        }
    }
}
