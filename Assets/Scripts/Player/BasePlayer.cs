using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
public abstract class BasePlayer : MonoBehaviour
{
    protected CharacterController controller;

    [SerializeField]
    protected Camera camera;
    
    public Team TeamType
    {
        get;
        protected set;
    }= Team.None;
    
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
        this.TeamType = team;
    }
    
    public abstract void StartMove(Vector2 startSpeed);
    public abstract void StopMove();
    public abstract void StartTurn(Vector2 speed);
    public abstract void StopTurn();
    public abstract void Jump();
    public abstract void ShootBubble(float size);
    public abstract void ShootAttack();
}
