using System;
using UnityEngine;

public class FieldManager : MonoBehaviour
{
    public Transform spawnRoot;
    
    // singleton
    private static FieldManager instance = null;
    public static FieldManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        // assgin singleton
        instance = this;
    }
}
