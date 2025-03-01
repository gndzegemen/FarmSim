using UnityEngine;
using System;

[Serializable]
public class CropData
{
    public CropType cropType;
    public string cropName;
    public int growthTimeMinutes;
    public int seedCost;
    public int harvestYield;
    public ResourceType resourceType;
    public Sprite[] growthStageSprites; // Array of sprites for different growth stages
}

[Serializable]
public class CropTile
{
    public CropType cropType;
    public CropState cropState;
    public float growthProgress; // 0 to 1
    public float plantTime;
    
    public CropTile()
    {
        cropType = CropType.None;
        cropState = CropState.Empty;
        growthProgress = 0f;
        plantTime = 0f;
    }
} 