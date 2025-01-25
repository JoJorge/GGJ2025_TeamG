using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CliffLeeCL
{
    /// <summary>
    /// The interface for context which executes the state pattern.
    /// </summary>
    public interface IContext
    {
        void SetupStateMachine();
        void UpdateStateMachine();
    }
}
