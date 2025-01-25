using System;
using UnityEngine;

public class NormalPlayer : BasePlayer
{
    private const float MAX_TURN_VERTICAL = 30;
    
    private const float GRAVITY = 9.8f;
    
    private const float MAX_FALL_SPEED = 10;
    
    [SerializeField]
    private float jumpScale = 10;
    
    private Vector3 moveSpeed = Vector3.zero;

    private Vector2 turnSpeed;
    
    private bool isGrounded = false;
    
    private float verticalSpeed = 0;
    
    protected void FixedUpdate()
    {
        if (controller == null)
        {
            return;
        }
        // horizontal move
        if (moveSpeed != Vector3.zero)
        {
            controller.Move(moveSpeed * Time.fixedDeltaTime);
        }
        // vertical move
        if (!isGrounded)
        {
            verticalSpeed -= GRAVITY * Time.fixedDeltaTime;
            if (verticalSpeed < -MAX_FALL_SPEED)
            {
                verticalSpeed = -MAX_FALL_SPEED;
            }
            controller.Move(Vector3.up * verticalSpeed);
        }
        else
        {
            verticalSpeed = 0;
        }
        
        // turn
        if (camera == null)
        {
            return;
        }

        if (turnSpeed.x != 0)
        {
            transform.Rotate(Vector3.up, turnSpeed.x * Time.fixedDeltaTime);
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
            var realTurnSpeed = -turnSpeed.y * Time.fixedDeltaTime;
            realTurnSpeed = Mathf.Clamp(realTurnSpeed, -MAX_TURN_VERTICAL - nowAngle, MAX_TURN_VERTICAL - nowAngle);
            if (Mathf.Abs(realTurnSpeed) > 1e-3f)
            {
                camera.transform.Rotate(Vector3.right, realTurnSpeed);
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
            // reset vertical speed and height
            verticalSpeed = 0;
        }
    }
    
    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
        }
    }

    public override void StartMove(Vector2 startSpeed)
    {
        moveSpeed = startSpeed.y * camera.transform.forward + startSpeed.x * camera.transform.right;
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
        if (isGrounded)
        {
            verticalSpeed = jumpScale;
            isGrounded = false;
        }
    }

    public override void ShootBubble(float size)
    {
        var bubblePrefab = GameConfig.Instance.itemConfig.GetItemPrefab(ItemConfig.ItemType.Bubble);
        var bubble = Instantiate(bubblePrefab, GetSpawnPosition(), transform.rotation) as Bubble;
        bubble.SetSize(size);
        bubble.Fly(transform.forward, 5);
    }

    public override void ShootAttack()
    {
        var attackPrefab = GameConfig.Instance.itemConfig.GetItemPrefab(ItemConfig.ItemType.Attack);
        var attack = Instantiate(attackPrefab, GetSpawnPosition(), transform.rotation) as Attack;
        attack.Fly(transform.forward, 10);
    }
    
    public Vector3 GetSpawnPosition()
    {
        return transform.position + camera.transform.forward * 1.5f;
    }
}
