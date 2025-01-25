using System;
using CliffLeeCL;
using Fusion;
using UnityEngine;

public class Bubble : BaseItem
{

    [SerializeField]
    private float waitTime = 1.5f;

    [SerializeField]
    private CliffLeeCL.Timer waitTimer;


    [Networked]
    private float floatUpSpeedScale { get; set; } = 10;
    [Networked]
    private NetworkBool isFloating { get; set; } = false;
    [Networked]
    private Vector3 originSize { get; set; }
    [Networked]
    private Team team { get; set; } = Team.None;


    public override void Spawned()
    {
        if (!Runner.IsServer)
        {
            return;
        }
        originSize = transform.localScale;
        // add score
        if (team == Team.Blue)
        {
            ScoreManager.Instance.AddScore(ScoreManager.Team.Blue, 1);
        }
        else if (team == Team.Red)
        {
            ScoreManager.Instance.AddScore(ScoreManager.Team.Red, 1);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Runner.IsServer)
        {
            return;
        }
        if (isFloating)
        {
            transform.Translate(Vector3.up * floatUpSpeedScale * Time.fixedDeltaTime);
        }
    }

    public void SetTeam(Team team)
    {
        if (!Runner.IsServer)
        {
            return;
        }
        this.team = team;
    }

    public void StartDelayFloat(float size)
    {
        if (!Runner.IsServer)
        {
            return;
        }

        transform.localScale = originSize * size;
        floatUpSpeedScale = size * 10;
        waitTimer.StartCountDownTimer(waitTime, false, () => isFloating = true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Runner.IsServer)
        {
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Ceiling"))
        {
            // add score
            if (team == Team.Blue)
            {
                ScoreManager.Instance.AddScore(ScoreManager.Team.Blue, 2);
            }
            else if (team == Team.Red)
            {
                ScoreManager.Instance.AddScore(ScoreManager.Team.Red, 2);
            }
            Runner.Despawn(Object);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Attack"))
        {
            Debug.LogError("Shoot!");
            Runner.Despawn(Object);
        }
        else if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            isFloating = false;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (!Runner.IsServer)
        {
            return;
        }
        if (ScoreManager.Instance == null)
        {
            return;
        }
        // minus score
        if (team == Team.Blue)
        {
            ScoreManager.Instance.AddScore(ScoreManager.Team.Blue, -1);
        }
        else if (team == Team.Red)
        {
            ScoreManager.Instance.AddScore(ScoreManager.Team.Red, -1);
        }
    }
}
