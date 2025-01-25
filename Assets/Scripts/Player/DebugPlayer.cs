using UnityEngine;

public class DebugPlayer : BasePlayer
{
    public override void StartMove(Vector2 startSpeed)
    {
        Debug.Log("Move: " + startSpeed);
    }

    public override void StopMove()
    {
        Debug.Log("StopMove");
    }

    public override void StartTurn(Vector2 speed)
    {
        Debug.Log("Turn: " + speed);
    }

    public override void StopTurn()
    {
        Debug.Log("StopTurn");
    }
    
    public override void Jump()
    {
        Debug.Log("Jump");
    }

    public override void ShootBubble(float size)
    {
        Debug.Log("ShootBubble: " + size);
    }

    public override void ShootAttack()
    {
        Debug.Log("ShootAttack");
    }
}
