using UnityEngine;

namespace CliffLeeCL
{
    /// <summary>
    /// Defines the status of enemies.
    /// </summary>
    [CreateAssetMenu(fileName = "Assets/ScriptableObject/EnemyStatus", menuName = "Scriptable Objects/Enemy status", order = 1)]
    public class EnemyStatus : ScriptableObject
    {
        [Header("Status")]
        /// <summary>
        /// Define how much damage can a agent take.
        /// </summary>
        public int health = 10;
        /// <summary>
        /// The amount of damage that a agent can give.
        /// </summary>
        public int damage = 1;

        [Header("StateMachine")]
        /// <summary>
        /// Define the min and max idle time.
        /// </summary>
        public IntervalFloat idleTime = new IntervalFloat(0, 3);
        /// <summary>
        /// Define how fast the agent moves in the wander state.
        /// </summary>
        public float wanderSpeed = 20.0f;
        /// <summary>
        /// Define the min and max wander time.
        /// </summary>
        public IntervalFloat wanderTime = new IntervalFloat(1, 5);
        /// <summary>
        /// Define how fast the agent moves in the flee state.
        /// </summary>
        public float fleeSpeed = 30.0f;
        /// <summary>
        /// Define the min and max flee time.
        /// </summary>
        public IntervalFloat fleeTime = new IntervalFloat(3, 5);
        /// <summary>
        /// The remaining time after the agent is destroyed.
        /// </summary>
        public float destroyTime = 5.0f;
        /// <summary>
        /// Define how fast the agent rotates when he meets obstacles.
        /// </summary>
        public float angularSpeed = 3.0f;
        /// <summary>
        /// Define origin offset of multiple obstacle rays.
        /// </summary>
        public Vector3 obstacleRayOriginOffset = Vector3.zero;
        /// <summary>
        /// Define how far the obstacle ray is.
        /// </summary>
        public float obstacleRayLength = 5.0f;
        /// <summary>
        /// Define the distance that the agent should slow down their speed.
        /// </summary>
        public float slowingDistance = 5.0f;
        /// <summary>
        /// Define the distance behind the followTarget that the agent should follow.
        /// </summary>
        public float followingDistance = 5.0f;
    }
}
