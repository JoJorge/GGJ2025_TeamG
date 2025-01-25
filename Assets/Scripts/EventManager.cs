using System;
using UnityEngine;

namespace CliffLeeCL
{
    /// <summary>
    /// This singleton class manage all events in the game.
    /// </summary>
    /// <code>
    /// // Usage in other class example:\n
    /// void Start(){\n
    ///     EventManager.instance.onGameOver += LocalFunction;\n
    /// }\n
    /// \n
    /// // If OnEnable function will cause error, try listen to events in Start function.\n
    /// void OnEnable(){\n
    ///     EventManager.instance.onGameOver += LocalFunction;\n
    /// }\n
    /// \n
    /// void OnDisable(){\n
    ///     EventManager.instance.onGameOver -= LocalFunction;\n
    /// }\n
    /// \n
    /// void LocalFunction(){\n
    ///     //Do something here\n
    /// }
    /// </code>
    public class EventManager : Singleton<EventManager>
    {
        /// <summary>
        /// Define default event's function signature.
        /// </summary>
        public delegate void DefaultEventHandler();
        /// <summary>
        /// The event is called when match start.
        /// </summary>
        public event DefaultEventHandler onMatchStart;
        /// <summary>
        /// The event is called when match over.
        /// </summary>
        public event DefaultEventHandler onMatchOver;
        /// <summary>
        /// The event is called when a team scored.
        /// </summary>
        public event EventHandler<ScoreManager.Team> onTeamScored;
        
        /// <summary>
        /// The function is called when a match start.
        /// </summary>
        public void OnMatchStart()
        {
            onMatchStart?.Invoke();
            Debug.Log("OnMatchStart event is invoked!");
        }

        /// <summary>
        /// The function is called when a player scored.
        /// </summary>
        public void OnMatchOver()
        {
            onMatchOver?.Invoke();
            Debug.Log("OnMatchOver event is invoked!");
        }

        /// <summary>
        /// The function is called when a team scored.
        /// </summary>
        public void OnTeamScored(object sender, ScoreManager.Team team)
        {
            onTeamScored?.Invoke(sender, team);
        }
    }
}

