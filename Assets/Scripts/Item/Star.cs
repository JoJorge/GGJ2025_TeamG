using System;
using CliffLeeCL;
using UnityEngine;

public class Star : BaseItem
{
    [SerializeField]
    private int addScore = 3;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            var player = other.GetComponent<BasePlayer>();
            if (player.TeamType == Team.Blue)
            {
                ScoreManager.Instance.AddScore(ScoreManager.Team.Blue, addScore);
            }
            else if (player.TeamType == Team.Red)
            {
                ScoreManager.Instance.AddScore(ScoreManager.Team.Red, addScore);
            }
            Destroy(gameObject);
        }
    }
}
