using UnityEngine;

namespace CliffLeeCL
{
    /// <summary>
    /// The proxy decide how origin method or proxy method is used based on some conditions.
    /// </summary>
    public class Proxy : Subject
    {
        public bool isLoadingDone = false;

        RealSubject realSubject;

        public Proxy(RealSubject realSubject)
        {
            this.realSubject = realSubject;
        }

        public override void Request()
        {
            if(isLoadingDone)
                realSubject.Request();
            else
                Debug.Log("Request from Proxy.");
        }
    }
}
