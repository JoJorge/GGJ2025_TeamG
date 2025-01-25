using UnityEngine;

namespace CliffLeeCL
{
    public class ProxyTest : MonoBehaviour
    {
        void Start()
        {
            RealSubject realSubject = new RealSubject();
            Proxy proxy = new Proxy(realSubject);

            proxy.Request();
            proxy.isLoadingDone = true;
            proxy.Request();
        }
    }
}
