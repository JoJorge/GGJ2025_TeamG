namespace CliffLeeCL
{
    /// <summary>
    /// The base class defines the state behaviour.
    /// </summary>
    public abstract class State<C>
        where C: IContext
    {
        /// <summary>
        /// Context that execute the state machine.
        /// </summary>
        public C stateContext;

        /// <summary>
        /// State behaviour when updating.
        /// </summary>
        public abstract void UpdateState();

        /// <summary>
        /// State behaviour when enter the state.
        /// </summary>
        public abstract void OnStateEnter();

        /// <summary>
        /// State behaviour when exit the state.
        /// </summary>
        public abstract void OnStateExit();
    }
}
