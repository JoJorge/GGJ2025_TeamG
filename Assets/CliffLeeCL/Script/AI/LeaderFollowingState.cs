using UnityEngine;
using UnityEngine.Assertions;

namespace CliffLeeCL
{
    public class LeaderFollowingState : State<Agent>
    {
        /// <summary>
        /// state agent's rigidbody.
        /// </summary>
        Rigidbody rigid;

        /// <summary>
        /// State behaviour when updating.
        /// </summary>
        public override void UpdateState()
        {
           /* if (!stateContext.followTarget)
                return;

            Vector3 followPosition = stateContext.followTarget.position + (-stateContext.followTarget.forward * stateContext.followingDistance);
            Vector3 moveDirection = (followPosition - stateContext.transform.position).normalized;
            Vector3 newPosition;
            Quaternion rotationToFollowTarget = Quaternion.LookRotation((stateContext.followTarget.position - stateContext.transform.position).normalized);
            Quaternion rotationToFollowPosition = Quaternion.LookRotation(moveDirection);
            float speedFactor = Mathf.Clamp01(Vector3.Distance(followPosition, stateContext.transform.position) / stateContext.slowingDistance);

            newPosition = rigid.position + moveDirection * stateContext.followingSpeed * speedFactor * Time.deltaTime;
            
            rigid.MovePosition(newPosition);
            rigid.MoveRotation(Quaternion.Slerp(rotationToFollowPosition, rotationToFollowTarget, (1.3f - speedFactor)));*/
        }

        /// <summary>
        /// State behaviour when enter the state.
        /// </summary>
        public override void OnStateEnter()
        {
            if (!rigid)
            {
                rigid = stateContext.GetComponent<Rigidbody>();
                Assert.IsTrue(rigid, "The state agent needs \"Rigidbody\" component!");
            }
        }

        /// <summary>
        /// State behaviour when exit the state.
        /// </summary>
        public override void OnStateExit()
        {

        }
    }
}
