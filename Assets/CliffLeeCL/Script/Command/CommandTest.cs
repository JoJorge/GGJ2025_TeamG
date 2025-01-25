using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CliffLeeCL
{
    public class CommandTest : MonoBehaviour
    {
        void Start()
        {
            Invoker invoker = new Invoker();
            Receiver receiver = new Receiver();
            ConcreteCommand cmd1 = new ConcreteCommand(receiver);
            ConcreteCommand cmd2 = new ConcreteCommand(receiver);
            ConcreteCommand cmd3 = new ConcreteCommand(receiver);
            ConcreteCommand cmd4 = new ConcreteCommand(receiver);
            ConcreteCommand cmd5 = new ConcreteCommand(receiver);

            invoker.AddCommand(cmd1);
            invoker.AddCommand(cmd2);
            invoker.AddCommand(cmd3);

            invoker.ShowCommandList(false);
            invoker.ExecuteCommand();

            invoker.AddCommand(cmd4);
            invoker.AddCommand(cmd5);
            invoker.ExecuteCommand();
            invoker.ShowCommandList(false);
            invoker.UndoCommand();     
            invoker.ShowCommandList(true);
        }
    }
}
