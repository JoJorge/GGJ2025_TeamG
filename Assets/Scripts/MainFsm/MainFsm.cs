using UnityEngine;
using CliffLeeCL;

public class MainFsm : StateMachine<State<GameCore>, GameCore>
{
    // constructor
    public MainFsm(GameCore context) : base(context)
    {
        // add states
        AddState("Entry", context);
        AddState("WaitPlayer", context);
        AddState("MainMenu", context);
        AddState("Loading", context);
        AddState("PreparePlayer", context);
        AddState("MatchProcess", context);
        AddState("MatchOver", context);
    }
}
