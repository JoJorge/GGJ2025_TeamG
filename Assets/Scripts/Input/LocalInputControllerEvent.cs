using UnityEngine;
using UnityEngine.InputSystem;

public class LocalInputControllerEvent : BaseInputController
{
    public float moveScalar = 1;
    public float turnScalar = 1;
    
    public void OnMove(InputValue value)
    {
        player.StartMove(value.Get<Vector2>() * moveScalar);
    }
    
    public void OnLook(InputValue value)
    {
        player.StartTurn(value.Get<Vector2>() * turnScalar);
    }
    
    public void OnJump()
    {
        player.Jump();
    }
    
    public void OnAttack()
    {
        player.ShootAttack();
    }
    
    public void OnSecondaryAttack()
    {
        player.ShootBubble(1);
    }
}
