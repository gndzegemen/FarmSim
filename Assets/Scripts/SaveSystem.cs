using UnityEngine;
using System;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    private static SaveSystem _instance;
    public static SaveSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("SaveSystem");
                _instance = go.AddComponent<SaveSystem>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private const string PLAYER_DATA_KEY = "PLAYER_DATA";
    private const string LAST_SAVE_TIME_KEY = "LAST_SAVE_TIME";

    public PlayerData CurrentPlayerData { get; private set; }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Load player data when the game starts
        LoadPlayerData();
    }

    public void SavePlayerData()
    {
        if (CurrentPlayerData == null)
        {
            CurrentPlayerData = new PlayerData();
        }

        // Prepare the dictionary for serialization
        CurrentPlayerData.PrepareForSerialization();

        // Convert to JSON
        string jsonData = JsonUtility.ToJson(CurrentPlayerData);
        
        // Save to PlayerPrefs
        PlayerPrefs.SetString(PLAYER_DATA_KEY, jsonData);
        
        // Save current time
        DateTime now = DateTime.Now;
        string timeString = now.ToBinary().ToString();
        PlayerPrefs.SetString(LAST_SAVE_TIME_KEY, timeString);
        
        PlayerPrefs.Save();
        
        Debug.Log("Player data saved successfully!");
    }

    public void LoadPlayerData()
    {
        if (PlayerPrefs.HasKey(PLAYER_DATA_KEY))
        {
            string jsonData = PlayerPrefs.GetString(PLAYER_DATA_KEY);
            CurrentPlayerData = JsonUtility.FromJson<PlayerData>(jsonData);
            
            // Convert serializable list back to dictionary
            CurrentPlayerData.LoadFromSerialization();
            
            Debug.Log("Player data loaded successfully!");
        }
        else
        {
            // Create new player data if none exists
            CurrentPlayerData = new PlayerData
            {
                playerName = "Player",
                level = 1,
                experience = 0,
                coins = 100
            };
            
            // Add some starting resources
            CurrentPlayerData.AddResource(ResourceType.Coin, 100);
            CurrentPlayerData.AddResource(ResourceType.Wheat, 10);
            
            Debug.Log("No saved data found. Created new player data.");
        }
    }

    public DateTime GetLastSaveTime()
    {
        if (PlayerPrefs.HasKey(LAST_SAVE_TIME_KEY))
        {
            string timeString = PlayerPrefs.GetString(LAST_SAVE_TIME_KEY);
            long timeBinary = Convert.ToInt64(timeString);
            return DateTime.FromBinary(timeBinary);
        }
        
        return DateTime.Now;
    }

    public void DeleteSaveData()
    {
        PlayerPrefs.DeleteKey(PLAYER_DATA_KEY);
        PlayerPrefs.DeleteKey(LAST_SAVE_TIME_KEY);
        PlayerPrefs.Save();
        
        Debug.Log("Save data deleted.");
    }
} 