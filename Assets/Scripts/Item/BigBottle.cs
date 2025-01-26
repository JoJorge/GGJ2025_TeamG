using UnityEngine;
using UnityEngine.Serialization;

public class BigBottle : FieldItem
{
    // increase player's max bubble ammo and recover rate
    [SerializeField]
    private int addMaxAmmo = 10;
    
    [FormerlySerializedAs("addRefillRate")] [SerializeField]
    private int addRefillPerSecond = 1;

    protected override void CustomEffect(BasePlayer player)
    {
        var normalPlayer = player as NormalPlayer;
        if (normalPlayer == null)
        {
            return;
        }
        normalPlayer.UpgradeBubble(addMaxAmmo, addRefillPerSecond);
        Destroy(gameObject);
    }
}
