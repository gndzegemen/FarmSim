using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class ResourceProduction
{
    public ResourceType resourceType;
    public float productionRatePerMinute;
    public int maxCapacity;
    [HideInInspector]
    public float currentAmount;
}

public class ResourceGenerator : MonoBehaviour
{
    private static ResourceGenerator _instance;
    public static ResourceGenerator Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ResourceGenerator");
                _instance = go.AddComponent<ResourceGenerator>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [SerializeField]
    private List<ResourceProduction> resourceProductions = new List<ResourceProduction>();

    private const string RESOURCE_PRODUCTION_TIME_KEY = "RESOURCE_PRODUCTION_TIME";
    private float updateInterval = 10f; // Update resources every 10 seconds
    private float timer;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize with default production settings if none exist
        if (resourceProductions.Count == 0)
        {
            InitializeDefaultProductions();
        }
    }

    private void Start()
    {
        // Calculate offline production when the game starts
        CalculateOfflineProduction();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= updateInterval)
        {
            timer = 0f;
            UpdateResourceProduction(updateInterval / 60f); // Convert seconds to minutes
            SaveProductionTime();
        }
    }

    private void InitializeDefaultProductions()
    {
        // Add default resource productions
        resourceProductions.Add(new ResourceProduction
        {
            resourceType = ResourceType.Wheat,
            productionRatePerMinute = 1f,
            maxCapacity = 100,
            currentAmount = 0f
        });

        resourceProductions.Add(new ResourceProduction
        {
            resourceType = ResourceType.Corn,
            productionRatePerMinute = 0.5f,
            maxCapacity = 50,
            currentAmount = 0f
        });
    }

    private void UpdateResourceProduction(float elapsedTimeInMinutes)
    {
        foreach (var production in resourceProductions)
        {
            float newProduction = production.productionRatePerMinute * elapsedTimeInMinutes;
            production.currentAmount += newProduction;
            
            // Cap at max capacity
            if (production.currentAmount > production.maxCapacity)
            {
                production.currentAmount = production.maxCapacity;
            }
        }
        
        // Notify UI or other systems that resources have been updated
        OnResourcesUpdated();
    }

    private void CalculateOfflineProduction()
    {
        if (PlayerPrefs.HasKey(RESOURCE_PRODUCTION_TIME_KEY))
        {
            string timeString = PlayerPrefs.GetString(RESOURCE_PRODUCTION_TIME_KEY);
            long timeBinary = Convert.ToInt64(timeString);
            DateTime lastProductionTime = DateTime.FromBinary(timeBinary);
            
            // Calculate time difference in minutes
            TimeSpan timeDifference = DateTime.Now - lastProductionTime;
            float minutesElapsed = (float)timeDifference.TotalMinutes;
            
            // Update resources based on elapsed time
            UpdateResourceProduction(minutesElapsed);
            
            Debug.Log($"Calculated offline production for {minutesElapsed} minutes.");
        }
        
        // Save current time
        SaveProductionTime();
    }

    private void SaveProductionTime()
    {
        DateTime now = DateTime.Now;
        string timeString = now.ToBinary().ToString();
        PlayerPrefs.SetString(RESOURCE_PRODUCTION_TIME_KEY, timeString);
        PlayerPrefs.Save();
    }

    public void CollectResources()
    {
        if (SaveSystem.Instance.CurrentPlayerData == null)
        {
            Debug.LogError("Player data not loaded!");
            return;
        }

        foreach (var production in resourceProductions)
        {
            if (production.currentAmount > 0)
            {
                int amount = Mathf.FloorToInt(production.currentAmount);
                SaveSystem.Instance.CurrentPlayerData.AddResource(production.resourceType, amount);
                production.currentAmount -= amount;
            }
        }
        
        // Save player data after collecting resources
        SaveSystem.Instance.SavePlayerData();
    }

    // Event for UI updates
    public delegate void ResourcesUpdatedHandler();
    public event ResourcesUpdatedHandler OnResourcesUpdated = delegate { };

    // Methods to modify production rates and capacities
    public void SetProductionRate(ResourceType type, float newRate)
    {
        var production = resourceProductions.Find(p => p.resourceType == type);
        if (production != null)
        {
            production.productionRatePerMinute = newRate;
        }
        else
        {
            Debug.LogWarning($"No production found for resource type: {type}");
        }
    }

    public void SetMaxCapacity(ResourceType type, int newCapacity)
    {
        var production = resourceProductions.Find(p => p.resourceType == type);
        if (production != null)
        {
            production.maxCapacity = newCapacity;
        }
        else
        {
            Debug.LogWarning($"No production found for resource type: {type}");
        }
    }

    public float GetCurrentAmount(ResourceType type)
    {
        var production = resourceProductions.Find(p => p.resourceType == type);
        return production != null ? production.currentAmount : 0f;
    }

    public float GetProductionRate(ResourceType type)
    {
        var production = resourceProductions.Find(p => p.resourceType == type);
        return production != null ? production.productionRatePerMinute : 0f;
    }

    public int GetMaxCapacity(ResourceType type)
    {
        var production = resourceProductions.Find(p => p.resourceType == type);
        return production != null ? production.maxCapacity : 0;
    }
} 