using UnityEngine;
using System;
using System.Collections.Generic;

namespace CliffLeeCL
{
    /// <summary>
    /// The class handles behaviour and trasition between states.
    /// </summary>
    public class StateMachine<S, C> 
        where S: State<C>
        where C: IContext
    {
        /// <summary>
        /// This dictionary stores all states the stateMachine uses by state's name.
        /// </summary>
        protected Dictionary<string, S> stateNameToStateMap = new Dictionary<string, S>();
        /// <summary>
        /// Stores the current state;
        /// </summary>
        protected S currentState = null;
        /// <summary>
        /// Context that execute the state machine.
        /// </summary>
        protected C stateContext;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public StateMachine()
        {

        }

        /// <summary>
        /// Assign agent for the state machine when constructing.
        /// </summary>
        /// <param name="agent">Agent that execute the state machine.</param>
        public StateMachine(C context)
        {
            SetStateContext(context);
        }

        /// <summary>
        /// Clear all reference.
        /// </summary>
        ~StateMachine()
        {
            if (stateNameToStateMap != null)
            {
                stateNameToStateMap.Clear();
                stateNameToStateMap = null;
            }

            currentState = null;
        }

        /// <summary>
        /// Assign agent for the state machine.
        /// </summary>
        /// <param name="context">Context that execute the state machine.param>
        public void SetStateContext(C context) {
            stateContext = context;
            foreach(KeyValuePair<string, S> pair in stateNameToStateMap)
                pair.Value.stateContext = context;
        }

        /// <summary>
        /// Set initial state.
        /// </summary>
        /// <param name="stateName"></param>
        public void SetInitialState(string stateName)
        {
            if (!stateNameToStateMap.ContainsKey(stateName))
            {
                Debug.LogError("There is no state (" + stateName + ")");
                return;
            }
            TransitToState(stateName);
        }

        /// <summary>
        /// Add the state to state machine by state's name.
        /// </summary>
        /// <param name="stateName">The name of the state which is about to add.</param>
        public void AddState(string stateName, C context)
        {
            Type type = Type.GetType("CliffLeeCL." + stateName);
                
            if(type == null)
            {
                Debug.LogError("There is no state (" + stateName + ")");
                return;
            }

            if (stateNameToStateMap.ContainsKey(stateName))
            {
                Debug.LogWarning("Already has the state (" + stateName + ")");
                return;
            }

            S state = (S)Activator.CreateInstance(type);
            if(state == null)
            {
                Debug.LogError("Create instance error! (" + stateName + ")");
                return;
            }
            state.stateContext = context;
            stateNameToStateMap.Add(stateName, state);
        }

        /// <summary>
        /// Remove the state from state machine by state's name.
        /// </summary>
        /// <param name="stateName">The name of the state which is about to remove.</param>
        public void RemoveState(string stateName)
        {
            if (stateNameToStateMap.ContainsKey(stateName))
                stateNameToStateMap.Remove(stateName);
        }

        /// <summary>
        /// Transit to specific state.
        /// </summary>
        /// /// <param name="stateName">The name of the state which is about to transit to.</param>
        public void TransitToState(string stateName)
        {
            if (stateContext == null)
                return;
            if (!stateNameToStateMap.ContainsKey(stateName))
            {
                Debug.LogError("There is no state (" + stateName + ")");
                return;
            }
            if (currentState == stateNameToStateMap[stateName]) // Transit to the same state repeatly.
                return;
            if (currentState == null) // First time initialization.
                currentState = stateNameToStateMap[stateName];

            currentState.OnStateExit();
            currentState = stateNameToStateMap[stateName];
            currentState.OnStateEnter();
        }

        /// <summary>
        /// Check whether the current state matches the stateName.
        /// </summary>
        /// <param name="stateName">The name of the state which is about to compare.</param>
        /// <returns>Is true when the current state matches the stateName.</returns>
        public bool IsCurrentState(string stateName)
        {
            if (stateContext == null)
                return false;
            if (!stateNameToStateMap.ContainsKey(stateName))
            {
                Debug.LogError("There is no state (" + stateName + ")");
                return false;
            }
            if (currentState == stateNameToStateMap[stateName])
                return true;
            return false;
        }

        /// <summary>
        /// Update state machine.
        /// </summary>
        public void UpdateStateMachine()
        {
            if (currentState == null || stateContext == null)
                return;
            currentState.UpdateState();
        }
    }
}
