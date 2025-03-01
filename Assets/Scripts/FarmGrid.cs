using UnityEngine;
using System;
using System.Collections.Generic;

public class FarmGrid : MonoBehaviour
{
    private static FarmGrid _instance;
    public static FarmGrid Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("FarmGrid instance not found!");
            }
            return _instance;
        }
    }

    [SerializeField] private int gridWidth = 5;
    [SerializeField] private int gridHeight = 5;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Transform gridParent;
    
    [Header("Farm Field Settings")]
    [SerializeField] private Transform farmFieldParent; // Parent object for the brown farm field
    [SerializeField] private string farmFieldTilePrefix = "3D_Tile_Farm_Field_01"; // Prefix for farm field tiles
    [SerializeField] private Color plantableTileColor = new Color(0.5f, 0.3f, 0.1f); // Dark brown color for plantable tiles
    
    [SerializeField] private List<CropData> cropDatabase = new List<CropData>();
    
    private CropTile[,] cropGrid;
    private GameObject[,] tileObjects;
    private SpriteRenderer[,] tileRenderers;
    private Dictionary<CropType, Transform> cropTypeParents = new Dictionary<CropType, Transform>();
    
    private const string FARM_GRID_DATA_KEY = "FARM_GRID_DATA";
    
    [Serializable]
    private class SerializableCropGrid
    {
        public List<SerializableCropTile> tiles = new List<SerializableCropTile>();
        public int width;
        public int height;
    }
    
    [Serializable]
    private class SerializableCropTile
    {
        public int x;
        public int y;
        public int cropType;
        public int cropState;
        public float growthProgress;
        public float plantTime;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        
        // Initialize crop database if empty
        if (cropDatabase.Count == 0)
        {
            InitializeCropDatabase();
        }
    }

    private void Start()
    {
        // Create parent transforms for each crop type
        CreateCropTypeParents();
        
        // Initialize or load the grid
        if (LoadGrid())
        {
            Debug.Log("Farm grid loaded from save data.");
        }
        else
        {
            InitializeGrid();
            Debug.Log("New farm grid initialized.");
        }
        
        // Start updating crops
        InvokeRepeating("UpdateCrops", 1f, 1f);
    }

    private void CreateCropTypeParents()
    {
        // Create a parent transform for each crop type
        foreach (CropType cropType in Enum.GetValues(typeof(CropType)))
        {
            if (cropType == CropType.None) continue;
            
            GameObject parentObj = new GameObject(cropType.ToString() + "Crops");
            parentObj.transform.SetParent(transform);
            cropTypeParents[cropType] = parentObj.transform;
        }
    }

    private void InitializeCropDatabase()
    {
        // Add wheat crop data
        CropData wheat = new CropData
        {
            cropType = CropType.Wheat,
            cropName = "Wheat",
            growthTimeMinutes = 2, // Short time for testing
            seedCost = 5,
            harvestYield = 15,
            resourceType = ResourceType.Wheat,
            growthStageSprites = new Sprite[4] // Should be assigned in the inspector
        };
        
        // Add corn crop data
        CropData corn = new CropData
        {
            cropType = CropType.Corn,
            cropName = "Corn",
            growthTimeMinutes = 5, // Longer time for testing
            seedCost = 10,
            harvestYield = 25,
            resourceType = ResourceType.Corn,
            growthStageSprites = new Sprite[4] // Should be assigned in the inspector
        };
        
        cropDatabase.Add(wheat);
        cropDatabase.Add(corn);
    }

    private void InitializeGrid()
    {
        cropGrid = new CropTile[gridWidth, gridHeight];
        tileObjects = new GameObject[gridWidth, gridHeight];
        tileRenderers = new SpriteRenderer[gridWidth, gridHeight];
        
        // Find farm field tiles if farmFieldParent is assigned
        if (farmFieldParent != null)
        {
            // Find all farm field tiles under the parent
            for (int i = 0; i < farmFieldParent.childCount; i++)
            {
                Transform tileParent = farmFieldParent.GetChild(i);
                
                // Extract coordinates from the tile name (assuming format like "tile01", "tile02", etc.)
                string tileName = tileParent.name.ToLower();
                int tileIndex = int.Parse(tileName.Replace("tile", ""));
                
                // Calculate grid coordinates (assuming tiles are numbered from left to right, bottom to top)
                int x = (tileIndex - 1) % gridWidth;
                int y = (tileIndex - 1) / gridWidth;
                
                if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
                {
                    // Find the farm field tile object
                    Transform farmFieldTile = null;
                    for (int j = 0; j < tileParent.childCount; j++)
                    {
                        if (tileParent.GetChild(j).name.Contains(farmFieldTilePrefix))
                        {
                            farmFieldTile = tileParent.GetChild(j);
                            break;
                        }
                    }
                    
                    if (farmFieldTile != null)
                    {
                        // Set the farm field tile color to dark brown to indicate it's plantable
                        Renderer tileRenderer = farmFieldTile.GetComponent<Renderer>();
                        if (tileRenderer != null)
                        {
                            // Create a new material instance to avoid affecting other tiles
                            Material tileMaterial = new Material(tileRenderer.material);
                            tileMaterial.color = plantableTileColor;
                            tileRenderer.material = tileMaterial;
                        }
                        
                        // Create crop tile data
                        cropGrid[x, y] = new CropTile();
                        tileObjects[x, y] = farmFieldTile.gameObject;
                        
                        // Add sprite renderer for crop visuals if needed
                        GameObject cropVisual = new GameObject("CropVisual_" + x + "_" + y);
                        cropVisual.transform.position = farmFieldTile.position + new Vector3(0, 0.01f, 0); // Slightly above the tile
                        cropVisual.transform.SetParent(farmFieldTile);
                        
                        SpriteRenderer spriteRenderer = cropVisual.AddComponent<SpriteRenderer>();
                        spriteRenderer.sortingOrder = 1; // Ensure it renders above the tile
                        tileRenderers[x, y] = spriteRenderer;
                        
                        // Add click handler component
                        TileClickHandler clickHandler = farmFieldTile.gameObject.AddComponent<TileClickHandler>();
                        clickHandler.Initialize(x, y);
                    }
                }
            }
        }
        else
        {
            // Fallback to the original grid creation if farmFieldParent is not assigned
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    cropGrid[x, y] = new CropTile();
                    
                    // Create visual representation
                    Vector3 position = new Vector3(x * tileSize, 0, y * tileSize);
                    GameObject tileObj = Instantiate(tilePrefab, position, Quaternion.identity, gridParent);
                    tileObj.name = $"Tile_{x}_{y}";
                    
                    tileObjects[x, y] = tileObj;
                    tileRenderers[x, y] = tileObj.GetComponent<SpriteRenderer>();
                    
                    // Add click handler component
                    TileClickHandler clickHandler = tileObj.AddComponent<TileClickHandler>();
                    clickHandler.Initialize(x, y);
                }
            }
        }
    }

    private void UpdateCrops()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (cropGrid[x, y] == null) continue;
                
                CropTile tile = cropGrid[x, y];
                
                if (tile.cropState == CropState.Seeded || tile.cropState == CropState.Growing)
                {
                    CropData cropData = GetCropData(tile.cropType);
                    if (cropData != null)
                    {
                        // Calculate growth based on real time
                        float currentTime = Time.time;
                        float elapsedTime = currentTime - tile.plantTime;
                        float totalGrowthTime = cropData.growthTimeMinutes * 60f; // Convert minutes to seconds
                        
                        tile.growthProgress = Mathf.Clamp01(elapsedTime / totalGrowthTime);
                        
                        // Update crop state based on growth progress
                        if (tile.growthProgress < 0.33f)
                        {
                            tile.cropState = CropState.Seeded;
                        }
                        else if (tile.growthProgress < 1f)
                        {
                            tile.cropState = CropState.Growing;
                        }
                        else
                        {
                            tile.cropState = CropState.Ready;
                        }
                        
                        // Update visual representation
                        UpdateTileVisual(x, y);
                    }
                }
            }
        }
    }

    private void UpdateTileVisual(int x, int y)
    {
        if (cropGrid[x, y] == null || tileRenderers[x, y] == null) return;
        
        CropTile tile = cropGrid[x, y];
        SpriteRenderer renderer = tileRenderers[x, y];
        
        if (tile.cropType == CropType.None || tile.cropState == CropState.Empty)
        {
            // Empty tile
            renderer.sprite = null; // Or use a default empty tile sprite
            return;
        }
        
        CropData cropData = GetCropData(tile.cropType);
        if (cropData != null && cropData.growthStageSprites.Length > 0)
        {
            int spriteIndex = 0;
            
            // Select sprite based on growth stage
            if (tile.cropState == CropState.Seeded)
            {
                spriteIndex = 0;
            }
            else if (tile.cropState == CropState.Growing)
            {
                // Map growth progress to middle sprites
                float normalizedGrowth = (tile.growthProgress - 0.33f) / 0.67f;
                spriteIndex = 1 + Mathf.FloorToInt(normalizedGrowth * (cropData.growthStageSprites.Length - 2));
                spriteIndex = Mathf.Clamp(spriteIndex, 1, cropData.growthStageSprites.Length - 2);
            }
            else if (tile.cropState == CropState.Ready)
            {
                spriteIndex = cropData.growthStageSprites.Length - 1;
            }
            
            // Apply sprite
            if (spriteIndex >= 0 && spriteIndex < cropData.growthStageSprites.Length)
            {
                renderer.sprite = cropData.growthStageSprites[spriteIndex];
                
                // Make sure the crop visual is under the correct crop type parent
                if (cropTypeParents.ContainsKey(tile.cropType))
                {
                    renderer.transform.SetParent(cropTypeParents[tile.cropType]);
                }
            }
        }
    }

    public bool PlantCrop(int x, int y, CropType cropType)
    {
        // Check if coordinates are valid
        if (!IsValidCoordinate(x, y))
        {
            Debug.LogWarning($"Invalid coordinates: {x}, {y}");
            return false;
        }
        
        // Check if tile is empty
        if (cropGrid[x, y].cropState != CropState.Empty)
        {
            Debug.LogWarning($"Tile at {x}, {y} is not empty");
            return false;
        }
        
        // Get crop data
        CropData cropData = GetCropData(cropType);
        if (cropData == null)
        {
            Debug.LogWarning($"No data found for crop type: {cropType}");
            return false;
        }
        
        // Check if player has enough resources to plant
        if (!SaveSystem.Instance.CurrentPlayerData.UseResource(ResourceType.Coin, cropData.seedCost))
        {
            Debug.LogWarning("Not enough coins to plant this crop");
            return false;
        }
        
        // Plant the crop
        cropGrid[x, y].cropType = cropType;
        cropGrid[x, y].cropState = CropState.Seeded;
        cropGrid[x, y].growthProgress = 0f;
        cropGrid[x, y].plantTime = Time.time;
        
        // Update visual
        UpdateTileVisual(x, y);
        
        // Save grid state
        SaveGrid();
        
        return true;
    }

    public bool HarvestCrop(int x, int y)
    {
        // Check if coordinates are valid
        if (!IsValidCoordinate(x, y))
        {
            return false;
        }
        
        // Check if crop is ready for harvest
        CropTile tile = cropGrid[x, y];
        if (tile.cropState != CropState.Ready)
        {
            return false;
        }
        
        // Get crop data
        CropData cropData = GetCropData(tile.cropType);
        if (cropData == null)
        {
            return false;
        }
        
        // Add harvested resources to player inventory
        SaveSystem.Instance.CurrentPlayerData.AddResource(cropData.resourceType, cropData.harvestYield);
        
        // Reset tile
        tile.cropType = CropType.None;
        tile.cropState = CropState.Empty;
        tile.growthProgress = 0f;
        tile.plantTime = 0f;
        
        // Update visual
        UpdateTileVisual(x, y);
        
        // Save changes
        SaveSystem.Instance.SavePlayerData();
        SaveGrid();
        
        return true;
    }

    public CropTile GetCropTile(int x, int y)
    {
        if (IsValidCoordinate(x, y))
        {
            return cropGrid[x, y];
        }
        return null;
    }

    public CropData GetCropData(CropType cropType)
    {
        return cropDatabase.Find(crop => crop.cropType == cropType);
    }

    private bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight && cropGrid[x, y] != null;
    }

    public void SaveGrid()
    {
        SerializableCropGrid serializableGrid = new SerializableCropGrid
        {
            width = gridWidth,
            height = gridHeight
        };
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (cropGrid[x, y] == null) continue;
                
                CropTile tile = cropGrid[x, y];
                
                SerializableCropTile serializableTile = new SerializableCropTile
                {
                    x = x,
                    y = y,
                    cropType = (int)tile.cropType,
                    cropState = (int)tile.cropState,
                    growthProgress = tile.growthProgress,
                    plantTime = tile.plantTime
                };
                
                serializableGrid.tiles.Add(serializableTile);
            }
        }
        
        string jsonData = JsonUtility.ToJson(serializableGrid);
        PlayerPrefs.SetString(FARM_GRID_DATA_KEY, jsonData);
        PlayerPrefs.Save();
    }

    public bool LoadGrid()
    {
        if (!PlayerPrefs.HasKey(FARM_GRID_DATA_KEY))
        {
            return false;
        }
        
        string jsonData = PlayerPrefs.GetString(FARM_GRID_DATA_KEY);
        SerializableCropGrid serializableGrid = JsonUtility.FromJson<SerializableCropGrid>(jsonData);
        
        // Check if grid dimensions match
        if (serializableGrid.width != gridWidth || serializableGrid.height != gridHeight)
        {
            Debug.LogWarning("Saved grid dimensions don't match current grid dimensions. Creating new grid.");
            return false;
        }
        
        // Initialize grid first
        InitializeGrid();
        
        // Load tile data
        foreach (SerializableCropTile serializableTile in serializableGrid.tiles)
        {
            int x = serializableTile.x;
            int y = serializableTile.y;
            
            if (IsValidCoordinate(x, y))
            {
                cropGrid[x, y].cropType = (CropType)serializableTile.cropType;
                cropGrid[x, y].cropState = (CropState)serializableTile.cropState;
                cropGrid[x, y].growthProgress = serializableTile.growthProgress;
                cropGrid[x, y].plantTime = serializableTile.plantTime;
                
                // Adjust plant time for time passed while game was closed
                if (cropGrid[x, y].cropState == CropState.Seeded || cropGrid[x, y].cropState == CropState.Growing)
                {
                    float timeDifference = Time.time - cropGrid[x, y].plantTime;
                    cropGrid[x, y].plantTime = Time.time - timeDifference;
                }
                
                // Update visual
                UpdateTileVisual(x, y);
            }
        }
        
        return true;
    }
    
    // Helper class for tile click handling
    private class TileClickHandler : MonoBehaviour
    {
        private int x;
        private int y;
        
        public void Initialize(int xCoord, int yCoord)
        {
            x = xCoord;
            y = yCoord;
        }
        
        private void OnMouseDown()
        {
            // Handle tile click - can be expanded based on game state
            CropTile tile = FarmGrid.Instance.GetCropTile(x, y);
            
            if (tile == null) return;
            
            if (tile.cropState == CropState.Empty)
            {
                // Show planting UI or plant default crop
                UIManager.Instance.ShowPlantingUI(x, y);
            }
            else if (tile.cropState == CropState.Ready)
            {
                // Harvest crop
                FarmGrid.Instance.HarvestCrop(x, y);
            }
            else
            {
                // Show crop info
                UIManager.Instance.ShowCropInfo(x, y);
            }
        }
    }
} 