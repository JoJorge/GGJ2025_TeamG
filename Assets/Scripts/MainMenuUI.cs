using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup mainPanel;
    [SerializeField]
    private Button startButton;
    [SerializeField]
    private Button exitButton;

    public event Action OnStart;
    public event Action OnExit;

    public bool Interactable 
    {
        get => mainPanel.interactable;
        set => mainPanel.interactable = value;
    }

    private void Awake()
    {
        startButton.onClick.AddListener(OnStartCallback);
        exitButton.onClick.AddListener(OnExitCallback);
    }

    public void OnStartCallback() 
    {
        OnStart?.Invoke();
    }
    
    public void OnExitCallback() 
    {
        OnExit?.Invoke();
    }
}
