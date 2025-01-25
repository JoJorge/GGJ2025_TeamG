using System;
using UnityEngine;
using Fusion;

public abstract class BaseItem : NetworkBehaviour
{
    [Networked]
    protected NetworkBool isFlying { get; set; } = false;
}
