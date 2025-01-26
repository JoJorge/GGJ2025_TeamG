using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup mainPanel;
    [SerializeField]
    private GameObject winRoot;
    [SerializeField]
    private GameObject drawRoot;
    [SerializeField]
    private TextMeshProUGUI teamText;
    [SerializeField]
    private Button confirmButton;

    [SerializeField]
    private Color blueTeamColor;
    
    [SerializeField]
    private Color redTeamColor;
    [SerializeField]
    private Color drawColor;

    public event Action OnConfirm;

    public bool Interactable 
    {
        get => mainPanel.interactable;
        set => mainPanel.interactable = value;
    }

    private void Awake()
    {
        confirmButton.onClick.AddListener(OnConfirmCallback);
    }

    public void SetWinner(Team team)
    {
        var isDraw = IsDraw(team);
        drawRoot.SetActive(isDraw);
        winRoot.SetActive(!isDraw);

        teamText.color = GetTeamColor(team);
        teamText.text = GetTeamText(team);
    }

    bool IsDraw(Team team)
    {
        return team != Team.Red && team != Team.Blue;
    }

    private Color GetTeamColor(Team team)
        => team switch
        {
            Team.Blue => blueTeamColor,
            Team.Red => redTeamColor,
            _ => drawColor
        };

    private string GetTeamText(Team team)
        => team switch
        {
            Team.Blue => "Blue Team",
            Team.Red => "Red Team",
            _ => string.Empty
        };

    public void OnConfirmCallback() 
    {
        OnConfirm?.Invoke();
    }
}
