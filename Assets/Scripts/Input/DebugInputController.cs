using Sirenix.OdinInspector;
using UnityEngine;

// debug input controller, use button to send input to player
public class DebugInputController : BaseInputController
{
    public Vector2 moveSpeed = Vector2.zero;
    
    [Range(-1, 1)]
    public float turnSpeed = 0;
    
    [Button, EnableIf("@(player != null)"), DisableInEditorMode]
    private void StartMove()
    {
        player.Move(moveSpeed);
    }
    
    [Button, EnableIf("@(player != null)"), DisableInEditorMode] 
    private void StopMove()
    {
        player.Move(Vector2.zero);
    }
    
    [Button, EnableIf("@(player != null)"), DisableInEditorMode]
    private void StartTurn()
    {
        player.Turn(turnSpeed);
    }
    
    [Button, EnableIf("@(player != null)"), DisableInEditorMode]
    private void StopTurn()
    {
        player.Turn(0);
    }
    
    [Button, EnableIf("@(player != null)"), DisableInEditorMode]
    private void Jump()
    {
        player.Jump();
    }
    
    [Button, EnableIf("@(player != null)"), DisableInEditorMode]
    private void BlowBubble()
    {
        player.BlowBubble();
    }
    
    [Button, EnableIf("@(player != null)"), DisableInEditorMode]
    private void ShootBubble()
    {
        player.ShootBubble();
    }
    
    [Button, EnableIf("@(player != null)"), DisableInEditorMode]
    private void ShootAttack()
    {
        player.ShootAttack();
    }
}
