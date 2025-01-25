using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public abstract class BasePlayer : MonoBehaviour
{
    protected CharacterController controller;

    [SerializeField]
    private Camera camera;
    
    protected Vector3 moveSpeed = Vector3.zero;

    protected float turnSpeed;
    
    protected void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (camera == null)
        {
            camera = GetComponentInChildren<Camera>();
        }
    }

    public void SetCamera(bool isEnable, RenderTexture renderTexture = null)
    {
        camera.enabled = isEnable;
        camera.targetTexture = renderTexture;
    }

    public abstract void StartMove(Vector2 startSpeed);
    public abstract void StopMove();
    public abstract void StartTurn(Vector2 speed);
    public abstract void StopTurn();
    public abstract void Jump();
    public abstract void ShootBubble(float size);
    public abstract void ShootAttack();
}
