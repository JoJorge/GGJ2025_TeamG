using System;
using CliffLeeCL;
using UnityEngine;
using Fusion;

public class GameCore : MonoBehaviour, IContext
{
    [SerializeField]
    private NetworkRunner networkRunner;
    
    [SerializeField]
    private NetworkManager networkManager;


    private MainFsm mainFsm = null;
    
    // singleton
    private static GameCore instance = null;
    public static GameCore Instance
    {
        get
        {
            return instance;
        }
    }

    public NetworkRunner NetworkRunner => networkRunner;
    public NetworkManager NetworkManager => networkManager;

    private void Awake()
    {
        // assgin singleton
        instance = this;
        
        SetupStateMachine();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
