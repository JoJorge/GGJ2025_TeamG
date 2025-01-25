using UnityEngine;

public class DebugPlayer : BasePlayer
{
    public override void Move(Vector2 moveSpeed)
    {
        Debug.Log("Move: " + moveSpeed);
    }

    public override void Turn(float speed)
    {
        Debug.Log("Turn: " + speed);
    }

    public override void Jump()
    {
        Debug.Log("Jump");
    }

    public override void BlowBubble()
    {
        Debug.Log("BlowBubble");
    }

    public override void ShootBubble()
    {
        Debug.Log("ShootBubble");
    }

    public override void ShootAttack()
    {
        Debug.Log("ShootAttack");
    }
}
