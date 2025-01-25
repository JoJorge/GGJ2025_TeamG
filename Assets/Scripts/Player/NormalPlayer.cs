using UnityEngine;

public class NormalPlayer : BasePlayer
{
    private const float MAX_TURN_VERTICAL = 30;
    
    protected Vector3 moveSpeed = Vector3.zero;

    protected Vector2 turnSpeed;
    
    protected void FixedUpdate()
    {
        if (controller == null)
        {
            return;
        }
        if (moveSpeed != Vector3.zero)
        {
            controller.Move(moveSpeed * Time.fixedDeltaTime);
        }
        if (camera == null)
        {
            return;
        }
        if (turnSpeed.x != 0)
        {
            camera.transform.Rotate(Vector3.up, turnSpeed.x * Time.fixedDeltaTime);
        }
        if (turnSpeed.y != 0)
        {
            var nowAngle = camera.transform.localEulerAngles.x;
            if (nowAngle > 180)
            {
                nowAngle -= 360;
            }
            if (nowAngle < -180)
            {
                nowAngle += 360;
            }
            var realTurnSpeed = turnSpeed.y * Time.fixedDeltaTime;
            realTurnSpeed = Mathf.Clamp(realTurnSpeed, -MAX_TURN_VERTICAL - nowAngle, MAX_TURN_VERTICAL - nowAngle);
            if (Mathf.Abs(realTurnSpeed) > 1e-3f)
            {
                camera.transform.Rotate(Vector3.right, realTurnSpeed);
            }
        }
    }
    
    public override void StartMove(Vector2 startSpeed)
    {
        moveSpeed = new Vector3(startSpeed.x, 0, startSpeed.y);
    }
    
    public override void StopMove()
    {
        moveSpeed = Vector3.zero;
    }

    public override void StartTurn(Vector2 turnSpeed)
    {
        this.turnSpeed = turnSpeed;
    }
    
    public override void StopTurn()
    {
        turnSpeed = Vector2.zero;
    }

    public override void Jump()
    {
        
    }

    public override void ShootBubble(float size)
    {
        throw new System.NotImplementedException();
    }

    public override void ShootAttack()
    {
        throw new System.NotImplementedException();
    }
}
