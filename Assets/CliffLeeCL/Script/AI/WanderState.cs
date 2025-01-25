using UnityEngine;
using UnityEngine.Assertions;

namespace CliffLeeCL
{
    public class WanderState : State<Agent>
    {
        /// <summary>
        /// The direction that the agent move toward.
        /// </summary>
        Vector3 moveDirection;

        /// <summary>
        /// State behaviour when updating.
        /// </summary>
        public override void UpdateState()
        {
            RaycastHit[] hit;
            Vector3 newPosition;
            Vector3 obstacleRayOrigin1;
            Vector3 obstacleRayOrigin2;

            obstacleRayOrigin1 = stateContext.transform.position + stateContext.transform.right * stateContext.enemyStatus.obstacleRayOriginOffset.x;
            hit = Physics.RaycastAll(obstacleRayOrigin1, stateContext.transform.forward, stateContext.enemyStatus.obstacleRayLength);
            Debug.DrawRay(obstacleRayOrigin1, stateContext.transform.forward * stateContext.enemyStatus.obstacleRayLength, Color.red);
            if (hit.Length > 0)
                HandleObstacleCollision(ref hit, -stateContext.transform.right);
            else
            {
                obstacleRayOrigin2 = stateContext.transform.position - stateContext.transform.right * stateContext.enemyStatus.obstacleRayOriginOffset.x;
                hit = Physics.RaycastAll(obstacleRayOrigin2, stateContext.transform.forward, stateContext.enemyStatus.obstacleRayLength);
                Debug.DrawRay(obstacleRayOrigin2, stateContext.transform.forward * stateContext.enemyStatus.obstacleRayLength, Color.red);
                if (hit.Length > 0)
                    HandleObstacleCollision(ref hit, stateContext.transform.right);
            }

            newPosition = stateContext.rigid.position + moveDirection * stateContext.enemyStatus.wanderSpeed * Time.deltaTime;
            stateContext.rigid.MovePosition(newPosition);
        }

        void HandleObstacleCollision(ref RaycastHit[] hit, Vector3 steerDir)
        {
            for (int i = 0; i < hit.Length; i++)
                if (hit[i].transform != stateContext.transform)
                {
                    moveDirection += steerDir * stateContext.enemyStatus.angularSpeed * Time.deltaTime;
                    moveDirection.y = 0.0f;
                    moveDirection.Normalize();
                    stateContext.transform.forward = moveDirection;
                }
        }

        /// <summary>
        /// State behaviour when enter the state.
        /// </summary>
        override public void OnStateEnter()
        {
            float wanderTime = Random.Range(stateContext.enemyStatus.wanderTime.minimun, stateContext.enemyStatus.wanderTime.maximun);

            stateContext.timer.StartCountDownTimer(
                wanderTime,
                false,
                OnTimeIsUp
                );

            moveDirection = Random.onUnitSphere;
            moveDirection.y = 0.0f;
            moveDirection.Normalize();
            stateContext.transform.forward = moveDirection;

            stateContext.animator.SetTrigger("isWander");
        }

        /// <summary>
        /// State behaviour when exit the state.
        /// </summary>
        override public void OnStateExit()
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