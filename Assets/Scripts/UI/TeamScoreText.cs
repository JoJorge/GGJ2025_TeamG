using CliffLeeCL;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class TeamScoreText : MonoBehaviour
{
    public ScoreManager.Team team;
    public TextMeshProUGUI teamScoreText;
    
    private void OnEnable()
    {
        EventManager.Instance.onMatchStart += OnMatchStart;
        EventManager.Instance.onTeamScored += OnTeamScored;
    }

    private void OnDisable()
    {
        EventManager.Instance.onMatchStart -= OnMatchStart;
        EventManager.Instance.onTeamScored -= OnTeamScored;
    }
    
    private void OnMatchStart()
    {
        teamScoreText.text = "0";
    }
    
    private void OnTeamScored(object sender, ScoreManager.Team scoredTeam)
    {
        if (scoredTeam == team)
        {
            teamScoreText.text = $"{ScoreManager.Instance.GetScore(scoredTeam):N0}";
        }
    }
    
    [Button]
    private void TestScore()
    {
        ScoreManager.Instance.AddScore(team, 1);
    }
}
