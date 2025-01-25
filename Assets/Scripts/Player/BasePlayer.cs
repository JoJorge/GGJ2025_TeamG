using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public abstract class BasePlayer : MonoBehaviour
{
    private CharacterController controller;

    protected void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public abstract void Move(Vector2 moveSpeed);
    public abstract void Turn(float speed);
    public abstract void Jump();
    public abstract void BlowBubble();
    public abstract void ShootBubble();
    public abstract void ShootAttack();
}
