using System;
using CliffLeeCL;
using UnityEngine;
using UnityEngine.Serialization;

public class FieldManager : MonoBehaviour
{
    private const float SPAWN_CD = 30;
    
    public Transform blueSpawnRoot;

    public Transform redSpawnRoot;
    
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
    }
    
    public void StartSpawnItem()
    {
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
