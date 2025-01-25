using UnityEngine;

namespace CliffLeeCL
{
    public class ConcreteCommand : Command
    {
        public ConcreteCommand(Receiver receiver) : base(receiver) { }

        public override void Execute()
        {
            receiver.ActionAboutConcreteCommand();
            Debug.Log("Command " + Id + " executed.");
        }

        public override void Undo()
        {
            receiver.ActionAboutConcreteCommandUndo();
            Debug.Log("Command " + Id + " undid.");
        }
    }
}
