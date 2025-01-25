using System;
using UnityEngine;

public class NormalPlayer : BasePlayer
{
    private const float MAX_TURN_VERTICAL = 30;
    
    private const float MAX_FALL_SPEED = 10;

    [SerializeField]
    private float gravity = 9.8f;
    
    [SerializeField] 
    private float jumpScale = 5f;

    [SerializeField]
    private float flySpeed = 10f;
    
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
            verticalSpeed -= gravity * Time.fixedDeltaTime;
            if (verticalSpeed < -MAX_FALL_SPEED)
            {
                verticalSpeed = -MAX_FALL_SPEED;
            }
            var prvY = transform.position.y;
            controller.Move(Vector3.up * verticalSpeed);
            var nowY = transform.position.y;
            if (verticalSpeed < 0 && prvY - nowY < 1e-3f)
            {
                isGrounded = true;
                verticalSpeed = 0;
            }
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
        var bubble = Instantiate(bubblePrefab, GetSpawnPosition(), camera.transform.rotation) as Bubble;
        bubble.SetSize(size);
        bubble.Fly(flySpeed);
    }

    public override void ShootAttack()
    {
        var attackPrefab = GameConfig.Instance.itemConfig.GetItemPrefab(ItemConfig.ItemType.Attack);
        var attack = Instantiate(attackPrefab, GetSpawnPosition(), camera.transform.rotation) as Attack;
        attack.Fly(flySpeed);
    }
    
    public Vector3 GetSpawnPosition()
    {
        return transform.position + camera.transform.forward * 1.5f;
    }
}
