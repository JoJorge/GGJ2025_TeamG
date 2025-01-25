using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Scriptable Objects/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    public enum PlayerType
    {
        Normal,
        Debug,
        Net,
    }
    
    // (PlayerType, prefab) pair
    [Serializable]
    public class PlayerTypePrefabPair
    {
        public PlayerType playerType;
        public BasePlayer player;
    }
    
    // pair list
    [SerializeField]
    private List<PlayerTypePrefabPair> playerTypePrefabList = new List<PlayerTypePrefabPair>();

    // pair dictionary
    private Dictionary<PlayerType, BasePlayer> playerTypePrefabDict = new Dictionary<PlayerType, BasePlayer>();
    
    private void OnEnable()
    {
        // init dictionary
        playerTypePrefabDict.Clear();
        foreach (var pair in playerTypePrefabList)
        {
            if (playerTypePrefabDict.ContainsKey(pair.playerType))
            {
                continue;
            }
            playerTypePrefabDict.Add(pair.playerType, pair.player);
        }
    }
    
    public BasePlayer GetPlayerPrefab(PlayerType playerType)
    {
        if (playerTypePrefabDict.ContainsKey(playerType))
        {
            return playerTypePrefabDict[playerType];
        }
        return null;
    }
}
