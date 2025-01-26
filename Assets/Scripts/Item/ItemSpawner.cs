using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField]
    private List<ItemConfig.ItemType> itemTypeList = new List<ItemConfig.ItemType>();
    
    [SerializeField]
    private Transform spawnRoot;
    
    private BaseItem item = null;
    
    public void TrySpawnItem()
    {
        if (item != null || GameConfig.Instance?.itemConfig == null)
        {
            return;
        }
        if (itemTypeList.Count == 0)
        {
            return;
        }
        
        var itemType = itemTypeList[Random.Range(0, itemTypeList.Count)];
        var itemPrefab = GameConfig.Instance.itemConfig.GetItemPrefab(itemType);
        item = GameObject.Instantiate<BaseItem>(itemPrefab);
        item.transform.position = spawnRoot.position;
    }
}
