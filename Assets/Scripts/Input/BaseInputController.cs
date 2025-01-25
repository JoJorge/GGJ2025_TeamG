using UnityEngine;

public abstract class BaseInputController : MonoBehaviour
{
    [SerializeField]
    protected BasePlayer player;
    
    // set player
    public void SetPlayer(BasePlayer player)
    {
        this.player = player;
    }
}
