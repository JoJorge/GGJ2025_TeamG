using System;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup mainPanel;
    [SerializeField]
    private Button redTeamButton;
    [SerializeField]
    private Button blueTeamButton;
    [SerializeField]
    private Button spectatorButton;

    public event Action<Team> OnSelectTeam;

    public bool Interactable 
    {
        get => mainPanel.interactable;
        set => mainPanel.interactable = value;
    }

    private void Awake()
    {
        redTeamButton.onClick.AddListener(() => OnSelectTeam?.Invoke(Team.Red));
        blueTeamButton.onClick.AddListener(() => OnSelectTeam?.Invoke(Team.Blue));
        spectatorButton.onClick.AddListener(() => OnSelectTeam?.Invoke(Team.Spectator));
    }

    public void ShowTeam() 
    { 
        
    }
}
