using UnityEngine;
using UnityEngine.Assertions;

namespace CliffLeeCL
{
    public class Wander2DState : State<Agent>
    {
        /// <summary>
        /// state agent's rigidbody.
        /// </summary>
        Rigidbody2D rigid;
        /// <summary>
        /// The direction that the agent move toward.
        /// </summary>
        Vector2 moveDirection;

        /// <summary>
        /// State behaviour when updating.
        /// </summary>
        public override void UpdateState()
        {
            Vector3 newPosition = rigid.position + moveDirection * stateContext.enemyStatus.wanderSpeed * Time.deltaTime;

            rigid.MovePosition(newPosition);
        }

        /// <summary>
        /// State behaviour when enter the state.
        /// </summary>
        public override void OnStateEnter()
        {
            float wanderTime = Random.Range(stateContext.enemyStatus.wanderTime.minimun, stateContext.enemyStatus.wanderTime.maximun);

            stateContext.timer.StartCountDownTimer(
                wanderTime,
                false,
                OnTimeIsUp
                );

            moveDirection = Random.onUnitSphere;
            moveDirection.Normalize();
            stateContext.transform.right = moveDirection;

            if (!rigid)
            {
                rigid = stateContext.GetComponent<Rigidbody2D>();
                Assert.IsTrue(rigid, "The state agent needs \"Rigidbody\" component!");
            }
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
            stateContext.stateMachine.TransitToState("IdleState");
        }
    }
}