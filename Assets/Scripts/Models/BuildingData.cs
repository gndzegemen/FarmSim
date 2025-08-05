using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class BuildingData
{
    public BuildingType buildingType;
    public string buildingName;
    public int width = 1;
    public int height = 1;
    public int cost;
    public Sprite buildingSprite;
    public Dictionary<ResourceType, int> productionBoosts = new Dictionary<ResourceType, int>();
    
    // For serialization purposes
    [Serializable]
    public class ProductionBoost
    {
        public ResourceType resourceType;
        public int boostAmount;
    }
    
    public List<ProductionBoost> serializableBoosts = new List<ProductionBoost>();
    
    public void PrepareForSerialization()
    {
        serializableBoosts.Clear();
        foreach (var boost in productionBoosts)
        {
            serializableBoosts.Add(new ProductionBoost
            {
                resourceType = boost.Key,
                boostAmount = boost.Value
            });
        }
    }
    
    public void LoadFromSerialization()
    {
        productionBoosts.Clear();
        foreach (var boost in serializableBoosts)
        {
            productionBoosts[boost.resourceType] = boost.boostAmount;
        }
    }
}

[Serializable]
public class PlacedBuilding
{
    public BuildingType buildingType;
    public int x;
    public int y;
    public int width;
    public int height;
    
    public PlacedBuilding(BuildingType type, int posX, int posY, int w, int h)
    {
        buildingType = type;
        x = posX;
        y = posY;
        width = w;
        height = h;
    }
} 