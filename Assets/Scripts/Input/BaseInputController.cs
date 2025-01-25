using Fusion;
using UnityEngine;

public abstract class BaseInputController : SimulationBehaviour
{
    [SerializeField]
    protected BasePlayer player;
    
    // set player
    public void SetPlayer(BasePlayer player)
    {
        this.player = player;
    }
}
