namespace CliffLeeCL
{
    /// <summary>
    /// The base class for a command from users.
    /// </summary>
    public abstract class Command
    {
        static int maxId = 0;

        protected Receiver receiver;
        int id;

        public int Id
        {
            get { return id; }
        }

        public Command(Receiver receiver)
        {
            this.receiver = receiver;
            id = maxId++ + 1;
        }

        public abstract void Execute();
        public abstract void Undo();
    }
}
