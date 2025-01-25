using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

namespace CliffLeeCL
{
    public class Agent : MonoBehaviour, IContext
    {
        /// <summary>
        /// Define the status of a enemy.
        /// </summary>
        public EnemyStatus enemyStatus;
        /// <summary>
        /// Define the score of a enemy.
        /// </summary>
        public EnemyScore enemyScore;
 
        [HideInInspector]
        /// <summary>
        /// state agent's rigidbody.
        /// </summary>
        public Rigidbody rigid;
        [HideInInspector]
        /// <summary>
        /// state agent's animator.
        /// </summary>
        public Animator animator;
        [HideInInspector]
        /// <summary>
        /// The transform of target that the agnet should be following.
        /// </summary>
        public Transform followTarget;
        [HideInInspector]
        /// <summary>
        /// Use in the state machine.
        /// </summary>
        public Timer timer;

        /// <summary>
        /// The state machine controls the agnet's behaviour.
        /// </summary>
        public StateMachine<State<Agent>, Agent> stateMachine;

        Renderer render;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            SetupStateMachine();
            timer = gameObject.AddComponent<Timer>();
        }

        public void SetupStateMachine()
        {
            stateMachine = new StateMachine<State<Agent>, Agent>(this);
            stateMachine.AddState("IdleState", this);
            stateMachine.AddState("WanderState", this);
            stateMachine.AddState("FleeState", this);
            stateMachine.AddState("HurtState", this);
        }

        public void UpdateStateMachine()
        {
            stateMachine.UpdateStateMachine();
        }

        /// <summary>
        /// Start is called once on the frame when a script is enabled.
        /// </summary>
        void Start()
        {
            animator = gameObject.GetComponent<Animator>();
            Assert.IsTrue(animator, "The state agent needs \"Animator\" component!");
            rigid = gameObject.GetComponent<Rigidbody>();
            Assert.IsTrue(rigid, "The state agent needs \"Rigidbody\" component!");
            render = gameObject.GetComponent<Renderer>();
            stateMachine.SetInitialState("IdleState");
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            UpdateStateMachine();
        }

        /// <summary>
        /// The function will be called by attacker to damage the owner of this function.
        /// </summary>
        /// <param name="damage">The amount of damage</param>
        public void TakeDamage(int damage)
        {
            stateMachine.TransitToState("HurtState");
            enemyStatus.health -= damage;
        }

    }
}
