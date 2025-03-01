using UnityEngine;
using System;
using System.Collections.Generic;

public class BuildingSystem : MonoBehaviour
{
    private static BuildingSystem _instance;
    public static BuildingSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("BuildingSystem instance not found!");
            }
            return _instance;
        }
    }

    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Transform buildingsParent;
    [SerializeField] private GameObject buildingPlacementIndicator;
    [SerializeField] private Material validPlacementMaterial;
    [SerializeField] private Material invalidPlacementMaterial;
    
    [SerializeField] private List<BuildingData> buildingDatabase = new List<BuildingData>();
    
    private BuildingType[,] grid;
    private List<PlacedBuilding> placedBuildings = new List<PlacedBuilding>();
    
    private BuildingType currentBuildingType = BuildingType.None;
    private bool isPlacingBuilding = false;
    private GameObject currentIndicator;
    
    private const string BUILDING_DATA_KEY = "BUILDING_DATA";
    
    [Serializable]
    private class SerializableBuildingData
    {
        public List<SerializablePlacedBuilding> buildings = new List<SerializablePlacedBuilding>();
    }
    
    [Serializable]
    private class SerializablePlacedBuilding
    {
        public int buildingType;
        public int x;
        public int y;
        public int width;
        public int height;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        
        // Initialize building database if empty
        if (buildingDatabase.Count == 0)
        {
            InitializeBuildingDatabase();
        }
        
        // Initialize grid
        grid = new BuildingType[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = BuildingType.None;
            }
        }
    }

    private void Start()
    {
        // Load placed buildings
        LoadBuildings();
    }

    private void Update()
    {
        if (isPlacingBuilding)
        {
            UpdateBuildingPlacement();
        }
    }

    private void InitializeBuildingDatabase()
    {
        // Add barn building data
        BuildingData barn = new BuildingData
        {
            buildingType = BuildingType.Barn,
            buildingName = "Barn",
            width = 2,
            height = 2,
            cost = 100
        };
        barn.productionBoosts[ResourceType.Wheat] = 10;
        
        // Add silo building data
        BuildingData silo = new BuildingData
        {
            buildingType = BuildingType.Silo,
            buildingName = "Silo",
            width = 1,
            height = 2,
            cost = 75
        };
        silo.productionBoosts[ResourceType.Corn] = 5;
        
        // Add mill building data
        BuildingData mill = new BuildingData
        {
            buildingType = BuildingType.Mill,
            buildingName = "Mill",
            width = 2,
            height = 1,
            cost = 150
        };
        mill.productionBoosts[ResourceType.Wheat] = 5;
        mill.productionBoosts[ResourceType.Corn] = 5;
        
        buildingDatabase.Add(barn);
        buildingDatabase.Add(silo);
        buildingDatabase.Add(mill);
    }

    public void StartPlacingBuilding(BuildingType buildingType)
    {
        if (buildingType == BuildingType.None)
        {
            return;
        }
        
        currentBuildingType = buildingType;
        isPlacingBuilding = true;
        
        // Create placement indicator
        if (currentIndicator == null)
        {
            currentIndicator = Instantiate(buildingPlacementIndicator);
        }
        
        // Set indicator size based on building dimensions
        BuildingData buildingData = GetBuildingData(buildingType);
        if (buildingData != null)
        {
            Vector3 size = new Vector3(buildingData.width * cellSize, buildingData.height * cellSize, 0.1f);
            currentIndicator.transform.localScale = size;
        }
        
        // Set initial position
        UpdateBuildingPlacement();
    }

    public void CancelPlacingBuilding()
    {
        isPlacingBuilding = false;
        currentBuildingType = BuildingType.None;
        
        if (currentIndicator != null)
        {
            Destroy(currentIndicator);
            currentIndicator = null;
        }
    }

    private void UpdateBuildingPlacement()
    {
        // Get mouse position in world space
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        
        // Convert to grid position
        int gridX = Mathf.FloorToInt(mousePosition.x / cellSize);
        int gridY = Mathf.FloorToInt(mousePosition.y / cellSize);
        
        // Clamp to grid bounds
        gridX = Mathf.Clamp(gridX, 0, gridWidth - 1);
        gridY = Mathf.Clamp(gridY, 0, gridHeight - 1);
        
        // Get building data
        BuildingData buildingData = GetBuildingData(currentBuildingType);
        if (buildingData == null)
        {
            return;
        }
        
        // Check if placement is valid
        bool isValid = IsValidPlacement(gridX, gridY, buildingData.width, buildingData.height);
        
        // Update indicator position
        Vector3 placementPosition = new Vector3(
            gridX * cellSize + (buildingData.width * cellSize / 2),
            gridY * cellSize + (buildingData.height * cellSize / 2),
            0
        );
        
        currentIndicator.transform.position = placementPosition;
        
        // Update indicator color
        Renderer renderer = currentIndicator.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = isValid ? validPlacementMaterial : invalidPlacementMaterial;
        }
        
        // Place building on mouse click
        if (Input.GetMouseButtonDown(0) && isValid)
        {
            PlaceBuilding(gridX, gridY, buildingData);
            CancelPlacingBuilding();
        }
        
        // Cancel placement on right click
        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacingBuilding();
        }
    }

    private bool IsValidPlacement(int x, int y, int width, int height)
    {
        // Check if building fits within grid bounds
        if (x < 0 || y < 0 || x + width > gridWidth || y + height > gridHeight)
        {
            return false;
        }
        
        // Check if all cells are empty
        for (int i = x; i < x + width; i++)
        {
            for (int j = y; j < y + height; j++)
            {
                if (grid[i, j] != BuildingType.None)
                {
                    return false;
                }
            }
        }
        
        return true;
    }

    private void PlaceBuilding(int x, int y, BuildingData buildingData)
    {
        // Check if player has enough resources
        if (!SaveSystem.Instance.CurrentPlayerData.UseResource(ResourceType.Coin, buildingData.cost))
        {
            Debug.LogWarning("Not enough coins to place this building");
            return;
        }
        
        // Create building object
        GameObject buildingObject = new GameObject(buildingData.buildingName);
        buildingObject.transform.SetParent(buildingsParent);
        
        // Position building
        Vector3 position = new Vector3(
            x * cellSize + (buildingData.width * cellSize / 2),
            y * cellSize + (buildingData.height * cellSize / 2),
            0
        );
        buildingObject.transform.position = position;
        
        // Add sprite renderer
        SpriteRenderer spriteRenderer = buildingObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = buildingData.buildingSprite;
        
        // Add collider for interaction
        BoxCollider2D collider = buildingObject.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(buildingData.width * cellSize, buildingData.height * cellSize);
        
        // Add building click handler
        BuildingClickHandler clickHandler = buildingObject.AddComponent<BuildingClickHandler>();
        clickHandler.Initialize(x, y, buildingData.width, buildingData.height);
        
        // Mark grid cells as occupied
        for (int i = x; i < x + buildingData.width; i++)
        {
            for (int j = y; j < y + buildingData.height; j++)
            {
                grid[i, j] = buildingData.buildingType;
            }
        }
        
        // Add to placed buildings list
        PlacedBuilding placedBuilding = new PlacedBuilding(
            buildingData.buildingType,
            x, y,
            buildingData.width,
            buildingData.height
        );
        placedBuildings.Add(placedBuilding);
        
        // Apply production boosts
        ApplyProductionBoosts(buildingData);
        
        // Save building data
        SaveBuildings();
        
        Debug.Log($"Placed {buildingData.buildingName} at ({x}, {y})");
    }

    private void ApplyProductionBoosts(BuildingData buildingData)
    {
        foreach (var boost in buildingData.productionBoosts)
        {
            ResourceType resourceType = boost.Key;
            int boostAmount = boost.Value;
            
            // Apply boost to resource production
            float currentRate = ResourceGenerator.Instance.GetProductionRate(resourceType);
            float newRate = currentRate + (boostAmount / 100f); // Convert percentage to rate
            
            ResourceGenerator.Instance.SetProductionRate(resourceType, newRate);
            
            Debug.Log($"Applied {boostAmount}% production boost to {resourceType}");
        }
    }

    public BuildingData GetBuildingData(BuildingType buildingType)
    {
        return buildingDatabase.Find(building => building.buildingType == buildingType);
    }

    public void MoveBuilding(int oldX, int oldY, int newX, int newY)
    {
        // Find the building at the old position
        PlacedBuilding building = placedBuildings.Find(b => b.x == oldX && b.y == oldY);
        if (building == null)
        {
            Debug.LogWarning($"No building found at ({oldX}, {oldY})");
            return;
        }
        
        // Get building data
        BuildingData buildingData = GetBuildingData(building.buildingType);
        if (buildingData == null)
        {
            return;
        }
        
        // Check if new position is valid
        if (!IsValidPlacement(newX, newY, building.width, building.height))
        {
            Debug.LogWarning($"Cannot move building to ({newX}, {newY})");
            return;
        }
        
        // Clear old grid cells
        for (int i = building.x; i < building.x + building.width; i++)
        {
            for (int j = building.y; j < building.y + building.height; j++)
            {
                grid[i, j] = BuildingType.None;
            }
        }
        
        // Update building position
        building.x = newX;
        building.y = newY;
        
        // Mark new grid cells as occupied
        for (int i = newX; i < newX + building.width; i++)
        {
            for (int j = newY; j < newY + building.height; j++)
            {
                grid[i, j] = building.buildingType;
            }
        }
        
        // Update visual position
        // Find the building GameObject by position
        foreach (Transform child in buildingsParent)
        {
            BuildingClickHandler clickHandler = child.GetComponent<BuildingClickHandler>();
            if (clickHandler != null && clickHandler.OriginalX == oldX && clickHandler.OriginalY == oldY)
            {
                // Update position
                Vector3 newPosition = new Vector3(
                    newX * cellSize + (building.width * cellSize / 2),
                    newY * cellSize + (building.height * cellSize / 2),
                    0
                );
                child.position = newPosition;
                
                // Update click handler
                clickHandler.Initialize(newX, newY, building.width, building.height);
                
                break;
            }
        }
        
        // Save building data
        SaveBuildings();
        
        Debug.Log($"Moved building from ({oldX}, {oldY}) to ({newX}, {newY})");
    }

    public void SaveBuildings()
    {
        SerializableBuildingData serializableData = new SerializableBuildingData();
        
        foreach (PlacedBuilding building in placedBuildings)
        {
            SerializablePlacedBuilding serializableBuilding = new SerializablePlacedBuilding
            {
                buildingType = (int)building.buildingType,
                x = building.x,
                y = building.y,
                width = building.width,
                height = building.height
            };
            
            serializableData.buildings.Add(serializableBuilding);
        }
        
        string jsonData = JsonUtility.ToJson(serializableData);
        PlayerPrefs.SetString(BUILDING_DATA_KEY, jsonData);
        PlayerPrefs.Save();
    }

    public void LoadBuildings()
    {
        if (!PlayerPrefs.HasKey(BUILDING_DATA_KEY))
        {
            return;
        }
        
        string jsonData = PlayerPrefs.GetString(BUILDING_DATA_KEY);
        SerializableBuildingData serializableData = JsonUtility.FromJson<SerializableBuildingData>(jsonData);
        
        // Clear existing buildings
        placedBuildings.Clear();
        
        // Reset grid
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = BuildingType.None;
            }
        }
        
        // Clear existing building objects
        foreach (Transform child in buildingsParent)
        {
            Destroy(child.gameObject);
        }
        
        // Place buildings
        foreach (SerializablePlacedBuilding serializableBuilding in serializableData.buildings)
        {
            BuildingType buildingType = (BuildingType)serializableBuilding.buildingType;
            BuildingData buildingData = GetBuildingData(buildingType);
            
            if (buildingData != null)
            {
                // Create building object
                GameObject buildingObject = new GameObject(buildingData.buildingName);
                buildingObject.transform.SetParent(buildingsParent);
                
                // Position building
                Vector3 position = new Vector3(
                    serializableBuilding.x * cellSize + (serializableBuilding.width * cellSize / 2),
                    serializableBuilding.y * cellSize + (serializableBuilding.height * cellSize / 2),
                    0
                );
                buildingObject.transform.position = position;
                
                // Add sprite renderer
                SpriteRenderer spriteRenderer = buildingObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = buildingData.buildingSprite;
                
                // Add collider for interaction
                BoxCollider2D collider = buildingObject.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(serializableBuilding.width * cellSize, serializableBuilding.height * cellSize);
                
                // Add building click handler
                BuildingClickHandler clickHandler = buildingObject.AddComponent<BuildingClickHandler>();
                clickHandler.Initialize(serializableBuilding.x, serializableBuilding.y, serializableBuilding.width, serializableBuilding.height);
                
                // Mark grid cells as occupied
                for (int i = serializableBuilding.x; i < serializableBuilding.x + serializableBuilding.width; i++)
                {
                    for (int j = serializableBuilding.y; j < serializableBuilding.y + serializableBuilding.height; j++)
                    {
                        grid[i, j] = buildingType;
                    }
                }
                
                // Add to placed buildings list
                PlacedBuilding placedBuilding = new PlacedBuilding(
                    buildingType,
                    serializableBuilding.x,
                    serializableBuilding.y,
                    serializableBuilding.width,
                    serializableBuilding.height
                );
                placedBuildings.Add(placedBuilding);
                
                // Apply production boosts
                ApplyProductionBoosts(buildingData);
            }
        }
        
        Debug.Log($"Loaded {placedBuildings.Count} buildings");
    }
    
    // Helper class for building click handling
    private class BuildingClickHandler : MonoBehaviour
    {
        public int OriginalX { get; private set; }
        public int OriginalY { get; private set; }
        private int width;
        private int height;
        private bool isDragging = false;
        private Vector3 dragOffset;
        
        public void Initialize(int x, int y, int w, int h)
        {
            OriginalX = x;
            OriginalY = y;
            width = w;
            height = h;
        }
        
        private void OnMouseDown()
        {
            // Start dragging
            isDragging = true;
            dragOffset = transform.position - GetMouseWorldPosition();
        }
        
        private void OnMouseDrag()
        {
            if (isDragging)
            {
                transform.position = GetMouseWorldPosition() + dragOffset;
            }
        }
        
        private void OnMouseUp()
        {
            if (isDragging)
            {
                isDragging = false;
                
                // Get new grid position
                Vector3 position = transform.position - dragOffset;
                float cellSize = BuildingSystem.Instance.cellSize;
                
                int newX = Mathf.RoundToInt(position.x / cellSize - width / 2f);
                int newY = Mathf.RoundToInt(position.y / cellSize - height / 2f);
                
                // Try to move building
                if (newX != OriginalX || newY != OriginalY)
                {
                    BuildingSystem.Instance.MoveBuilding(OriginalX, OriginalY, newX, newY);
                }
                else
                {
                    // Reset position if not moved
                    transform.position = new Vector3(
                        OriginalX * cellSize + (width * cellSize / 2),
                        OriginalY * cellSize + (height * cellSize / 2),
                        0
                    );
                }
            }
        }
        
        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = -Camera.main.transform.position.z;
            return Camera.main.ScreenToWorldPoint(mousePosition);
        }
    }
} 