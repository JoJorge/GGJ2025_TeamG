using System;
using Sirenix.OdinInspector;
using UnityEngine;

// debug input controller, use button to send input to player
public class DebugInputController : BaseInputController
{
    public Vector2 moveSpeed = Vector2.zero;
    
    public Vector2 turnSpeed = Vector2.zero;
    
    [Range(0, 1)]
    public float bubbleSize = 0.1f;

    [Button, EnableIf("@(player != null)"), DisableInEditorMode]
    private void StartMove()
    {
        player.StartMove(moveSpeed);
    }
    
    [Button, EnableIf("@(player != null)"), DisableInEditorMode] 
    private void StopMove()
    {
        player.StopMove();
    }
    
    [Button, EnableIf("@(player != null)"), DisableInEditorMode]
    private void StartTurn()
    {
        player.StartTurn(turnSpeed);
    }
    
    [Button, EnableIf("@(player != null)"), DisableInEditorMode]
    private void StopTurn()
    {
        player.StopTurn();
    }
    
    [Button, EnableIf("@(player != null)"), DisableInEditorMode]
    private void Jump()
    {
        player.Jump();
    }
    
    [Button, EnableIf("@(player != null)"), DisableInEditorMode]
    private void BlowBubble()
    {
    }
    
    [Button, EnableIf("@(player != null)"), DisableInEditorMode]
    private void ShootBubble()
    {
        player.ShootBubble(bubbleSize);
    }
    
    [Button, EnableIf("@(player != null)"), DisableInEditorMode]
    private void ShootAttack()
    {
        player.ShootAttack();
    }
}
