using UnityEngine;
using System.Collections.Generic;

namespace CliffLeeCL
{
    /// <summary>
    /// The manager of commands. Just like the waiter in the restaurant.
    /// </summary>
    public class Invoker
    {
        List<Command> command = new List<Command>();
        List<Command> commandHistory = new List<Command>();

        public void AddCommand(Command newCommand)
        {
            command.Add(newCommand);
        }

        public void DeleteCommand(Command commandToDelete)
        {
            for(int i = 0; i < command.Count; i++)
            {
                if(command[i].Id == commandToDelete.Id)
                    command.RemoveAt(i);
            }
        }

        public void DeleteAllCommand()
        {
            command.Clear();
        }

        public void ExecuteCommand()
        {
            for(int i = 0; i < command.Count; i++)
            {
                command[i].Execute();
                commandHistory.Add(command[i]);
            }
            command.Clear();
        }

        public void UndoCommand()
        {
            commandHistory[commandHistory.Count - 1].Undo();
            commandHistory.RemoveAt(commandHistory.Count - 1);
        }

        public void ShowCommandList(bool isHistory)
        {
            List<Command> commandToShow = isHistory ? commandHistory : command;
            string commandStr = "Cmd: ";

            for (int i = 0; i < commandToShow.Count; i++)
            {
                commandStr += command[i].Id + " -> ";
                if (i == commandToShow.Count - 1)
                    commandStr += "end";
            }
            Debug.Log(commandStr);
        }
    }
}
