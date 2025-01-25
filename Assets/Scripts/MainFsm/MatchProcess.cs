using UnityEngine;

namespace CliffLeeCL
{
    public class MatchProcess : State<GameCore>
    {

        public override void UpdateState()
        {
        }

        public override void OnStateEnter()
        {
            stateContext.matchStartTimer.StartCountDownTimer(stateContext.matchStartTime, false, OnMatchStart);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public override void OnStateExit()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        void OnMatchStart()
        {
            EventManager.Instance.OnMatchStart();
            stateContext.matchRoundTimer.StartCountDownTimer(stateContext.matchRoundTime, false, () =>
            {
                EventManager.Instance.OnMatchOver();
                stateContext.SwitchState("MatchOver");
            });
        }
    }
}
