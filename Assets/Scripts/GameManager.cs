using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("GameManager instance not found!");
            }
            return _instance;
        }
    }

    [SerializeField] private bool resetSaveData = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Reset save data if needed (for testing)
        if (resetSaveData)
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("All save data has been reset.");
        }
    }

    private void Start()
    {
        // Initialize systems in the correct order
        StartCoroutine(InitializeSystems());
    }

    private IEnumerator InitializeSystems()
    {
        // Wait for a frame to ensure all components are initialized
        yield return null;
        
        // Initialize SaveSystem first
        if (SaveSystem.Instance == null)
        {
            GameObject saveSystemGO = new GameObject("SaveSystem");
            saveSystemGO.AddComponent<SaveSystem>();
        }
        
        yield return null;
        
        // Initialize ResourceGenerator
        if (ResourceGenerator.Instance == null)
        {
            GameObject resourceGeneratorGO = new GameObject("ResourceGenerator");
            resourceGeneratorGO.AddComponent<ResourceGenerator>();
        }
        
        yield return null;
        
        // Make sure FarmGrid and BuildingSystem are initialized
        // These should already exist in the scene
        if (FarmGrid.Instance == null)
        {
            Debug.LogError("FarmGrid not found in the scene. Please add it to the scene.");
        }
        
        if (BuildingSystem.Instance == null)
        {
            Debug.LogError("BuildingSystem not found in the scene. Please add it to the scene.");
        }
        
        yield return null;
        
        // Initialize UI last
        if (UIManager.Instance == null)
        {
            Debug.LogError("UIManager not found in the scene. Please add it to the scene.");
        }
        else
        {
            // Update UI with player data
            UIManager.Instance.UpdatePlayerInfo();
        }
        
        Debug.Log("All systems initialized successfully.");
    }

    private void OnApplicationQuit()
    {
        // Save all data when the application quits
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SavePlayerData();
        }
        
        if (FarmGrid.Instance != null)
        {
            FarmGrid.Instance.SaveGrid();
        }
        
        if (BuildingSystem.Instance != null)
        {
            BuildingSystem.Instance.SaveBuildings();
        }
        
        Debug.Log("All data saved on application quit.");
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        // Save data when the application is paused (e.g., when the app goes to background on mobile)
        if (pauseStatus)
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SavePlayerData();
            }
            
            if (FarmGrid.Instance != null)
            {
                FarmGrid.Instance.SaveGrid();
            }
            
            if (BuildingSystem.Instance != null)
            {
                BuildingSystem.Instance.SaveBuildings();
            }
            
            Debug.Log("All data saved on application pause.");
        }
    }
} 