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

    [SerializeField]
    private Vector2 sizeRange = new Vector2(0.5f, 1.5f);
    
    [SerializeField]
    private Vector2 floatUpSpeedRange = new Vector2(5, 15);
    
    [SerializeField]
    private float flySpeed = 5;
    
    [SerializeField]
    private float flyTime = 0.5f;
    
    private bool isWaiting = false;
    
    private bool isFloating = false;
    
    private Team team = Team.None;
    
    protected bool isFlying = false;
    
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
        if (isFlying)
        {
            transform.Translate(transform.forward * flySpeed * Time.fixedDeltaTime, Space.World);
        }
        else if (isFloating)
        {
            transform.Translate(Vector3.up * floatUpSpeedScale * Time.fixedDeltaTime);
        }
    }

    public void SetTeam(Team team)
    {
        this.team = team;
        if (team == Team.Blue)
        {
            GetComponent<Renderer>().material = GameConfig.Instance.itemConfig.blueBubbleMaterial;
        }
        else if (team == Team.Red)
        {
            GetComponent<Renderer>().material = GameConfig.Instance.itemConfig.redBubbleMaterial;
        }
    }
    
    // size is a float between 0 and 1
    public void StartDelayFloat(float size)
    {
        size = Mathf.Clamp01(size);
        transform.localScale = Vector3.one * (sizeRange.x + (sizeRange.y - sizeRange.x) * size);
        floatUpSpeedScale = floatUpSpeedRange.x + (floatUpSpeedRange.y - floatUpSpeedRange.x) * size;
        isWaiting = true;
        isFlying = true;
        waitTimer.StartCountDownTimer(flyTime, false, () => {
            isFlying = false;
            isWaiting = true;
            waitTimer.StartCountDownTimer(waitTime, false, () => { 
                isFloating = true; 
                isWaiting = false;
            });
        });
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
        else if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            isFloating = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Attack"))
        {
            Destroy(gameObject);
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
