using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public abstract class BasePlayer : MonoBehaviour
{
    protected CharacterController controller;

    [SerializeField]
    protected Camera camera;
    
    protected Team team = Team.None;
    
    protected void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (camera == null)
        {
            camera = GetComponentInChildren<Camera>();
        }
        controller.enabled = false;
    }

    public void SetCamera(bool isEnable, RenderTexture renderTexture = null)
    {
        camera.enabled = isEnable;
        camera.targetTexture = renderTexture;
    }
    
    public void StartController()
    {
        controller.enabled = true;
    }
    
    public void SetTeam(Team team)
    {
        this.team = team;
    }
    
    public abstract void StartMove(Vector2 startSpeed);
    public abstract void StopMove();
    public abstract void StartTurn(Vector2 speed);
    public abstract void StopTurn();
    public abstract void Jump();
    public abstract void ShootBubble(float size);
    public abstract void ShootAttack();
}
