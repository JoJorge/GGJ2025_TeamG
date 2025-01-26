using UnityEngine;

public class Scissors : FieldItem
{
    // decrease player's attack CD
    [SerializeField]
    private float decreaseAttackCD = 0.1f;
    
    protected override void CustomEffect(BasePlayer player)
    {
        var normalPlayer = player as NormalPlayer;
        if (normalPlayer == null)
        {
            return;
        }
        normalPlayer.DecreaseAttackCd(decreaseAttackCD);
        Destroy(gameObject);
    }
}
