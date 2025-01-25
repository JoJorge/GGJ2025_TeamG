using System;
using CliffLeeCL;
using UnityEngine;

public class Bubble : BaseItem
{
    private Vector3 originSize;
    
    private float floatUpSpeedScale = 10;
    
    [SerializeField]
    private float waitTime = 1.5f;
    
    [SerializeField]
    private Timer waitTimer;

    private bool isFloating = false;
    
    private Team team = Team.None;
    
    private void Awake()
    {
        originSize = transform.localScale;
    }

    private void Start()
    {
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

    private void FixedUpdate()
    {
        if (isFloating)
        {
            transform.Translate(Vector3.up * floatUpSpeedScale * Time.fixedDeltaTime);
        }
    }

    public void SetTeam(Team team)
    {
        this.team = team;
    }
    
    public void StartDelayFloat(float size)
    {
        transform.localScale = originSize * size;
        floatUpSpeedScale = size * 10;
        waitTimer.StartCountDownTimer(waitTime, false, () => isFloating = true);
    }

    private void OnCollisionEnter(Collision other)
    {
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
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Attack"))
        {
            Destroy(gameObject);
        }
        else
        {
            isFloating = false;
        }
    }

    private void OnDestroy()
    {
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
