using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CliffLeeCL
{
    /// <summary>
    /// In hurt state, agent take damage and show its status afterwards.
    /// </summary>
    public class HurtState : State<Agent>
    {
        /// <summary>
        /// State behaviour when updating.
        /// </summary>
        public override void UpdateState()
        {
            
        }

        /// <summary>
        /// State behaviour when enter the state.
        /// </summary>
        public override void OnStateEnter()
        {
            if (stateContext.enemyStatus.health <= 0)
                OnAgentDied();
            else
                stateContext.stateMachine.TransitToState("FleeState");
        }

        /// <summary>
        /// State behaviour when exit the state.
        /// </summary>
        public override void OnStateExit()
        {
        }

        /// <summary>
        /// Actions will be done after the agent is dead.
        /// </summary>
        void OnAgentDied()
        {
            ScoreManager.Instance.AddScore(stateContext.enemyScore.score);
            ScoreManager.Instance.AddKillCount(stateContext.enemyScore.killCount);
            ScoreManager.Instance.AddCombo(stateContext.enemyScore.combo);
            ScoreManager.Instance.AddLove(stateContext.enemyScore.love);
            stateContext.rigid.constraints = RigidbodyConstraints.None;
            Object.Destroy(stateContext.gameObject, stateContext.enemyStatus.destroyTime);

            stateContext.animator.speed = 0.0f;
            AudioManager.Instance.PlaySoundRandomClipAndPitch(AudioManager.AudioName.EnemyDead1,
                AudioManager.AudioName.EnemyDead2, AudioManager.AudioName.EnemyDead3, AudioManager.AudioName.EnemySlash);
        }
    }
}
