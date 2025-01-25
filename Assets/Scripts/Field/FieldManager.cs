using System;
using CliffLeeCL;
using UnityEngine;

public class FieldManager : MonoBehaviour
{
    private const float SPAWN_CD = 30;
    
    public Transform spawnRoot;
    
    private ItemSpawner[] itemSpawnerArray;
    
    [SerializeField]
    private Timer spawnItemTimer;
    
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
    
    private void Start()
    {
        itemSpawnerArray = GetComponentsInChildren<ItemSpawner>();
        SpawnAllItem();
        spawnItemTimer.StartCountDownTimer(SPAWN_CD, true, SpawnAllItem);
    }
    
    private void SpawnAllItem()
    {
        foreach (var itemSpawner in itemSpawnerArray)
        {
            itemSpawner.TrySpawnItem();
        }
    }
}
