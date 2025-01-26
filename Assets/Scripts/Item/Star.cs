using System;
using CliffLeeCL;
using UnityEngine;

public class Star : FieldItem
{
    [SerializeField]
    private int addScore = 3;

    protected override void CustomEffect(BasePlayer player)
    {
        if (player.TeamType == Team.Blue)
        {
            ScoreManager.Instance.AddScore(ScoreManager.Team.Blue, addScore);
        }
        else if (player.TeamType == Team.Red)
        {
            ScoreManager.Instance.AddScore(ScoreManager.Team.Red, addScore);
        }
    }
}
