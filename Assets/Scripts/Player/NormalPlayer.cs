using System;
using CliffLeeCL;
using UnityEngine;

public class NormalPlayer : BasePlayer
{
    public enum AnimState
    {
        Idle,
        Move,
        Jump,
    }
    
    private const float MAX_TURN_VERTICAL = 60;
    
    private const float MAX_FALL_SPEED = 20;
    
    private const float MIN_ATTACK_CD = 1f;

    [SerializeField]
    private float gravity = 9.8f;
    
    [SerializeField] 
    private float jumpScale = 5f;

    [SerializeField]
    private float flySpeed = 10f;
    
    [SerializeField]
    private int maxBubbleAmmo = 100;
    
    [SerializeField]
    private int bubbleAmmoPerShoot = 10;
    
    [SerializeField]
    private int bubbleAmmoRefillPerSecond = 2;
    
    [SerializeField]
    private float attackCd = 4;
    
    [SerializeField]
    private Timer refillBubbleTimer;

    [SerializeField]
    private Timer attackCdTimer;

    [SerializeField] private Animator characterAnimator;
    
    [SerializeField]
    private Renderer characterRenderer;

    [SerializeField]
    private GameObject attackObj;
    
    private int leftBubbleAmmo = 100;
    
    private Vector3 moveSpeed = Vector3.zero;

    private Vector2 turnSpeed;
    
    private bool isGrounded = false;
    
    private float verticalSpeed = 0;
    
    private bool isMain = false;
    
    private int touchBubbleCount = 0;
    
    private bool isAttackCd = false;
    
    private float moveSpeedScale = 1;
    
    private AnimState nowAnimState = AnimState.Idle;
    
    private int touchBubbleFrame = 0;

    private bool IsTouchingBubble
    {
        get;
        set;
    } = false;

    private void Start()
    {
        leftBubbleAmmo = maxBubbleAmmo;
    }

    protected void FixedUpdate()
    {
        if (controller == null)
        {
            return;
        }
        var prvAnimState = nowAnimState;
        // horizontal move
        if (moveSpeed != Vector3.zero)
        {
            controller.Move(moveSpeed * Time.fixedDeltaTime);
            nowAnimState = AnimState.Move;
        }
        // vertical move
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

        if (verticalSpeed < 0)
        {
            isGrounded = false;
        }

        if (isGrounded)
        {
            verticalSpeed = 0;
        }
        else
        {
            nowAnimState = AnimState.Jump;
        }
        
        // turn
        if (camera == null)
        {
            return;
        }

        if (turnSpeed.x != 0)
        {
            transform.Rotate(Vector3.up, turnSpeed.x * Time.fixedDeltaTime);
            if (nowAnimState == AnimState.Idle)
            {
                nowAnimState = AnimState.Move;
            }
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
        if (nowAnimState != prvAnimState)
        {
            characterAnimator.Play(nowAnimState.ToString());
        }
    }

    private void LateUpdate()
    {
        const float TOUCH_BUBBLE_FRAME = 5;
        var nowFrame = Time.frameCount;
        if (IsTouchingBubble && nowFrame - touchBubbleFrame > TOUCH_BUBBLE_FRAME)
        {
            IsTouchingBubble = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Bubble"))
        {
            IsTouchingBubble = true;
            touchBubbleFrame = Time.frameCount;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Bubble"))
        {
            touchBubbleCount++;
        }
    }
    
    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Bubble"))
        {
            IsTouchingBubble = true;
            touchBubbleFrame = Time.frameCount;
        }
    }
    
    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Bubble"))
        {
            touchBubbleCount--;
        }
    }

    public void SetMain()
    {
        isMain = true;
        EventManager.Instance.OnCurrentAmmoChanged(this, leftBubbleAmmo);
        EventManager.Instance.OnMaxAmmoChanged(this, maxBubbleAmmo);
    }

    public override void SetTeam(Team team)
    {
        base.SetTeam(team);
        if (team == Team.Blue)
        {
            characterRenderer.material = GameConfig.Instance.playerConfig.blueMaterial;
        }
        else if (team == Team.Red)
        {
            characterRenderer.material = GameConfig.Instance.playerConfig.redMaterial;
        }
    }

    public void AddMoveSpeed(int percent)
    {
        moveSpeedScale += percent / 100f;
    }
    
    public void UpgradeBubble(int addMaxAmmo, int addAmmoRefill, bool isRefill = true)
    {
        maxBubbleAmmo += addMaxAmmo;
        bubbleAmmoRefillPerSecond += addAmmoRefill;
        if (isRefill)
        {
            leftBubbleAmmo = maxBubbleAmmo;
        }
        if (isMain)
        {
            EventManager.Instance.OnMaxAmmoChanged(this, maxBubbleAmmo);
            EventManager.Instance.OnCurrentAmmoChanged(this, leftBubbleAmmo);
        }
    }
    
    public void DecreaseAttackCd(float decreaseTime, bool resetCd = true)
    {
        attackCd = Mathf.Max(attackCd - decreaseTime, MIN_ATTACK_CD);
        if (resetCd)
        {
            attackCdTimer.StopTimer();
            isAttackCd = false;
        }
    }
    
    public override void StartMove(Vector2 startSpeed)
    {
        moveSpeed = startSpeed.y * transform.forward + startSpeed.x * transform.right;
        moveSpeed *= moveSpeedScale;
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
        if (leftBubbleAmmo < bubbleAmmoPerShoot)
        {
            return;
        }
        if (touchBubbleCount > 0 || IsTouchingBubble)
        {
            return;
        }
        if (leftBubbleAmmo == maxBubbleAmmo)
        {
            refillBubbleTimer.StartCountDownTimer(1, false, OnRefillTimerEnd);
        }
        leftBubbleAmmo -= bubbleAmmoPerShoot;
        if (isMain)
        {
            EventManager.Instance.OnCurrentAmmoChanged(this, leftBubbleAmmo);
        }
        var bubblePrefab = GameConfig.Instance.itemConfig.GetItemPrefab(ItemConfig.ItemType.Bubble);
        var bubble = Instantiate(bubblePrefab, GetSpawnPosition(), transform.rotation) as Bubble;
        bubble.SetTeam(TeamType);
        bubble.StartDelayFloat(size);
    }

    public override void ShootAttack()
    {
        if (isAttackCd)
        {
            return;
        }
        isAttackCd = true;
        attackObj.SetActive(false);
        attackCdTimer.StartCountDownTimer(attackCd, false, () => {
            isAttackCd = false;
            attackObj.SetActive(true);
        });
        EventManager.Instance.OnAttackCDStart(this, attackCd);
        var attackPrefab = GameConfig.Instance.itemConfig.GetItemPrefab(ItemConfig.ItemType.Attack);
        var attack = Instantiate(attackPrefab, GetSpawnPosition(), camera.transform.rotation) as Attack;
        attack.Fly(flySpeed);
    }
    
    private void OnRefillTimerEnd()
    {
        leftBubbleAmmo = Mathf.Min(leftBubbleAmmo + bubbleAmmoRefillPerSecond, maxBubbleAmmo);
        if (leftBubbleAmmo < maxBubbleAmmo)
        {
            refillBubbleTimer.StartCountDownTimer(1, false, OnRefillTimerEnd);
        }
        if (isMain)
        {
            EventManager.Instance.OnCurrentAmmoChanged(this, leftBubbleAmmo);
        }
    }
    
    public Vector3 GetSpawnPosition()
    {
        return transform.position + camera.transform.forward * 2f;
    }
}
