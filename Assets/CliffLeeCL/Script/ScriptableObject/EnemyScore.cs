using UnityEngine;

namespace CliffLeeCL
{
    /// <summary>
    /// Defines the score of enemies.
    /// </summary>
    [CreateAssetMenu(fileName = "Assets/ScriptableObject/EnemyScore", menuName = "Scriptable Objects/Enemy score", order = 1)]
    public class EnemyScore : ScriptableObject
    {
        [Header("Score")]
        /// <summary>
        /// Define how much score a enemy has.
        /// </summary>
        public int score = 100;
        /// <summary>
        /// The number of a killed unit.
        /// </summary>
        public int killCount = 1;
        /// <summary>
        /// The combo can a enemy gives.
        /// </summary>
        public int combo = 1;
        /// <summary>
        /// The love can a enemy gives.
        /// </summary>
        public float love = -0.05f;
    }
}
