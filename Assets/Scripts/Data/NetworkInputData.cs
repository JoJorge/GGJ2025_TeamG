using Fusion;
using UnityEngine;


public enum InputButtons
{ 
    Jump,
}

public struct NetworkInputData : INetworkInput
{
    public Team Team;
    public NetworkButtons Buttons;
    public Vector3 Movement;
}
