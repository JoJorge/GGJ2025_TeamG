
using UnityEngine;
using Fusion;

public class PlayerMetadata : NetworkBehaviour
{
    [Networked]
    public Team Team { get; set; }

    public override void FixedUpdateNetwork()
    {
        if(Team == Team.None && GetInput(out NetworkInputData data) && data.Team != Team.None)
        {
            Team = data.Team;
            Debug.LogError($"Set team to {Team}");
        }
    }

}