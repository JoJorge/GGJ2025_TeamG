using UnityEngine;

public class Boots : FieldItem
{
    [SerializeField] 
    [Range(0, 100)]
    private int addSpeedPercent = 5;
    
    protected override void CustomEffect(BasePlayer player)
    {
        var normalPlayer = player as NormalPlayer;
        if (normalPlayer != null)
        {
            normalPlayer.AddMoveSpeed(addSpeedPercent);
        }
    }
}
