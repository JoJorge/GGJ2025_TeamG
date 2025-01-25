using CliffLeeCL;
using UnityEngine;
using Fusion;
using Timer = CliffLeeCL.Timer;

public class GameCore : SingletonMono<GameCore>, IContext
{
    public float matchStartTime = 3.0f;
    [HideInInspector]
    public Timer matchStartTimer;
    public float matchRoundTime = 60.0f;
    [HideInInspector]
    public Timer matchRoundTimer;
        
    [SerializeField]
    private NetworkRunner networkRunner;
    
    [SerializeField]
    private NetworkManager networkManager;

    [SerializeField]
    private bool isUseNetwork = false;
    
    private MainFsm mainFsm = null;

    public NetworkRunner NetworkRunner => networkRunner;
    public NetworkManager NetworkManager => networkManager;
    public bool IsUseNetwork => isUseNetwork;

    private void Awake()
    {
        matchStartTimer = gameObject.AddComponent<Timer>();
        matchRoundTimer = gameObject.AddComponent<Timer>();
        SetupStateMachine();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        networkRunner.gameObject.SetActive(isUseNetwork);
        networkManager.gameObject.SetActive(isUseNetwork);
        mainFsm.SetInitialState("Entry");
    }

    // Update is called once per frame
    void Update()
    {
        UpdateStateMachine();
    }

    public void SetupStateMachine()
    {
        mainFsm = new MainFsm(this);
    }

    public void UpdateStateMachine()
    {
        mainFsm.UpdateStateMachine();
    }
    
    public void SwitchState(string stateName)
    {
        mainFsm.TransitToState(stateName);
    }
}
