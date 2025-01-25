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
        AddState("PreparePlayer", context);
    }
}
