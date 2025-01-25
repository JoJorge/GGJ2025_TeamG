using UnityEngine;

public class OpenOnlyOnMobile : MonoBehaviour
{
    public GameObject[] openObj;

#if UNITY_ANDROID || UNITY_IOS
    void Start()
    {
        foreach (var obj in openObj)
            obj.SetActive(true);
    }
#endif
}
