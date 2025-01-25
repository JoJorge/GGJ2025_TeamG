using UnityEngine;

namespace CliffLeeCL
{
    /// <summary>
    /// The executor of the actual commands.
    /// </summary>
    public class Receiver : MonoBehaviour
    {
        public void ActionAboutConcreteCommand()
        {
            Debug.Log("Do actions aboud concrete command!");
        }

        public void ActionAboutConcreteCommandUndo()
        {
            Debug.Log("Undo actions aboud concrete command!");
        }
    }
}
