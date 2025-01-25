using UnityEngine;

namespace CliffLeeCL
{
    /// <summary>
    /// The origin subject execution method.
    /// </summary>
    public class RealSubject : Subject
    {
        public RealSubject() { }

        public override void Request()
        {
            Debug.Log("Request from RealSubject.");
        }
    }
}
