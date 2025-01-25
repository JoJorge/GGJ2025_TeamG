using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemConfig", menuName = "Scriptable Objects/ItemConfig")]
public class ItemConfig : ScriptableObject
{
    public enum ItemType
    {
        Bubble,
        Attack
    }
    
    // (Item, prefab) pair
    [System.Serializable]
    public class ItemPrefabPair
    {
        public ItemType itemType;
        public BaseItem itemPrefab;
    }
    
    // pair list
    [SerializeField]
    private List<ItemPrefabPair> itemPrefabList = new System.Collections.Generic.List<ItemPrefabPair>();
    
    // pair dictionary
    private Dictionary<ItemType, BaseItem> itemPrefabDict = new Dictionary<ItemType, BaseItem>();
    
    private void OnEnable()
    {
        // init dictionary
        itemPrefabDict.Clear();
        foreach (var pair in itemPrefabList)
        {
            if (itemPrefabDict.ContainsKey(pair.itemType))
            {
                continue;
            }
            itemPrefabDict.Add(pair.itemType, pair.itemPrefab);
        }
    }
    
    public BaseItem GetItemPrefab(ItemType itemType)
    {
        if (itemPrefabDict.ContainsKey(itemType))
        {
            return itemPrefabDict[itemType];
        }
        return null;
    }
}
