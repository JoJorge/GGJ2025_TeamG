using UnityEngine;

namespace CliffLeeCL
{
    /// <summary>
    /// In IdleState, agent do nothing.
    /// </summary>
    public class IdleState : State<Agent>
    {
        public override void UpdateState()
        {
        }

        /// <summary>
        /// State behaviour when enter the state.
        /// </summary>
        public override void OnStateEnter()
        {
            float idleTime = Random.Range(stateContext.enemyStatus.idleTime.minimun, stateContext.enemyStatus.idleTime.maximun);

            stateContext.timer.StartCountDownTimer(
                idleTime,
                false,
                OnTimeIsUp
                );

            stateContext.animator.SetTrigger("isIdle");
        }

        /// <summary>
        /// State behaviour when exit the state.
        /// </summary>
        public override void OnStateExit()
        {
            stateContext.timer.StopCountDownTimer();
        }

        /// <summary>
        /// Is called when the timer's time is up.
        /// </summary>
        void OnTimeIsUp()
        {
            stateContext.stateMachine.TransitToState("WanderState");
        }
    }
}
