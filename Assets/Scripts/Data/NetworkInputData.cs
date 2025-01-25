using Fusion;
using UnityEngine;


public enum InputButtons
{ 
    Jump,
    ShootAttack,
    ShootBubble
}

public struct NetworkInputData : INetworkInput
{
    public Team Team;
    public NetworkButtons Buttons;
    public Vector2 Movement;
    public Vector2 Turn;
    public float Loudness;
}
