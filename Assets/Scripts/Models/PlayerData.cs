using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public string playerName;
    public int level;
    public int experience;
    public int coins;
    
    public Dictionary<ResourceType, int> inventory = new Dictionary<ResourceType, int>();
    
    // For serialization purposes since Dictionary is not serializable by default
    [Serializable]
    public class SerializableInventoryItem
    {
        public ResourceType resourceType;
        public int amount;
    }
    
    public List<SerializableInventoryItem> serializableInventory = new List<SerializableInventoryItem>();
    
    // Convert dictionary to serializable list
    public void PrepareForSerialization()
    {
        serializableInventory.Clear();
        foreach (var item in inventory)
        {
            serializableInventory.Add(new SerializableInventoryItem
            {
                resourceType = item.Key,
                amount = item.Value
            });
        }
    }
    
    // Convert serializable list back to dictionary
    public void LoadFromSerialization()
    {
        inventory.Clear();
        foreach (var item in serializableInventory)
        {
            inventory[item.resourceType] = item.amount;
        }
    }
    
    // Helper methods for inventory management
    public void AddResource(ResourceType type, int amount)
    {
        if (!inventory.ContainsKey(type))
        {
            inventory[type] = 0;
        }
        
        inventory[type] += amount;
    }
    
    public bool HasResource(ResourceType type, int amount)
    {
        return inventory.ContainsKey(type) && inventory[type] >= amount;
    }
    
    public bool UseResource(ResourceType type, int amount)
    {
        if (!HasResource(type, amount))
        {
            return false;
        }
        
        inventory[type] -= amount;
        return true;
    }
    
    public int GetResourceAmount(ResourceType type)
    {
        if (!inventory.ContainsKey(type))
        {
            return 0;
        }
        
        return inventory[type];
    }
}
