using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("UIManager instance not found!");
            }
            return _instance;
        }
    }

    [Header("Top Panel")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private Slider experienceBar;
    
    [Header("Bottom Menu")]
    [SerializeField] private Button cropButton;
    [SerializeField] private Button buildingButton;
    [SerializeField] private Button inventoryButton;
    [SerializeField] private Button shopButton;
    
    [Header("Crop Planting UI")]
    [SerializeField] private GameObject cropPlantingPanel;
    [SerializeField] private Transform cropButtonsContainer;
    [SerializeField] private Button cropButtonPrefab;
    
    [Header("Building Placement UI")]
    [SerializeField] private GameObject buildingPlacementPanel;
    [SerializeField] private Transform buildingButtonsContainer;
    [SerializeField] private Button buildingButtonPrefab;
    
    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform inventoryItemsContainer;
    [SerializeField] private GameObject inventoryItemPrefab;
    [SerializeField] private Button inventoryCloseButton;
    [SerializeField] private TMP_Dropdown inventoryCategoryDropdown;
    
    [Header("Crop Info UI")]
    [SerializeField] private GameObject cropInfoPanel;
    [SerializeField] private TextMeshProUGUI cropNameText;
    [SerializeField] private TextMeshProUGUI cropGrowthText;
    [SerializeField] private Slider cropGrowthBar;
    [SerializeField] private Button cropInfoCloseButton;
    
    // Current selected tile for planting
    private int selectedTileX;
    private int selectedTileY;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    private void Start()
    {
        // Initialize UI
        InitializeUI();
        
        // Update player info
        UpdatePlayerInfo();
        
        // Subscribe to events
        ResourceGenerator.Instance.OnResourcesUpdated += UpdateResourceDisplay;
    }

    private void InitializeUI()
    {
        // Set up button listeners
        cropButton.onClick.AddListener(() => ShowCropMenu());
        buildingButton.onClick.AddListener(() => ShowBuildingMenu());
        inventoryButton.onClick.AddListener(() => ShowInventoryPanel());
        shopButton.onClick.AddListener(() => ShowShopPanel());
        
        // Set up crop planting panel
        if (cropButtonsContainer != null && cropButtonPrefab != null)
        {
            SetupCropButtons();
        }
        
        // Set up building placement panel
        if (buildingButtonsContainer != null && buildingButtonPrefab != null)
        {
            SetupBuildingButtons();
        }
        
        // Set up inventory panel
        if (inventoryCloseButton != null)
        {
            inventoryCloseButton.onClick.AddListener(() => inventoryPanel.SetActive(false));
        }
        
        if (inventoryCategoryDropdown != null)
        {
            inventoryCategoryDropdown.onValueChanged.AddListener(FilterInventoryByCategory);
        }
        
        // Set up crop info panel
        if (cropInfoCloseButton != null)
        {
            cropInfoCloseButton.onClick.AddListener(() => cropInfoPanel.SetActive(false));
        }
        
        // Hide all panels initially
        if (cropPlantingPanel != null) cropPlantingPanel.SetActive(false);
        if (buildingPlacementPanel != null) buildingPlacementPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (cropInfoPanel != null) cropInfoPanel.SetActive(false);
    }

    private void SetupCropButtons()
    {
        // Clear existing buttons
        foreach (Transform child in cropButtonsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Get crop types from FarmGrid
        foreach (CropType cropType in System.Enum.GetValues(typeof(CropType)))
        {
            if (cropType == CropType.None) continue;
            
            // Create button
            Button cropButton = Instantiate(cropButtonPrefab, cropButtonsContainer);
            
            // Set button text
            TextMeshProUGUI buttonText = cropButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = cropType.ToString();
            }
            
            // Set button image if available
            Image buttonImage = cropButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                // Try to get crop sprite from FarmGrid
                CropData cropData = FarmGrid.Instance.GetComponent<FarmGrid>().GetCropData(cropType);
                if (cropData != null && cropData.growthStageSprites.Length > 0)
                {
                    buttonImage.sprite = cropData.growthStageSprites[0];
                }
            }
            
            // Set button click handler
            CropType capturedType = cropType; // Capture for lambda
            cropButton.onClick.AddListener(() => PlantSelectedCrop(capturedType));
        }
    }

    private void SetupBuildingButtons()
    {
        // Clear existing buttons
        foreach (Transform child in buildingButtonsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Get building types from BuildingSystem
        foreach (BuildingType buildingType in System.Enum.GetValues(typeof(BuildingType)))
        {
            if (buildingType == BuildingType.None) continue;
            
            // Create button
            Button buildingButton = Instantiate(buildingButtonPrefab, buildingButtonsContainer);
            
            // Set button text
            TextMeshProUGUI buttonText = buildingButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = buildingType.ToString();
            }
            
            // Set button image if available
            Image buttonImage = buildingButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                // Try to get building sprite from BuildingSystem
                BuildingData buildingData = BuildingSystem.Instance.GetComponent<BuildingSystem>().GetBuildingData(buildingType);
                if (buildingData != null && buildingData.buildingSprite != null)
                {
                    buttonImage.sprite = buildingData.buildingSprite;
                }
            }
            
            // Set button click handler
            BuildingType capturedType = buildingType; // Capture for lambda
            buildingButton.onClick.AddListener(() => StartPlacingBuilding(capturedType));
        }
    }

    public void UpdatePlayerInfo()
    {
        PlayerData playerData = SaveSystem.Instance.CurrentPlayerData;
        if (playerData == null) return;
        
        // Update player name
        if (playerNameText != null)
        {
            playerNameText.text = playerData.playerName;
        }
        
        // Update level
        if (levelText != null)
        {
            levelText.text = $"Level: {playerData.level}";
        }
        
        // Update coins
        if (coinsText != null)
        {
            coinsText.text = $"Coins: {playerData.coins}";
        }
        
        // Update experience bar
        if (experienceBar != null)
        {
            // Calculate experience progress (simplified)
            float experienceNeeded = playerData.level * 100f;
            experienceBar.value = playerData.experience / experienceNeeded;
        }
    }

    private void UpdateResourceDisplay()
    {
        // Update coins display
        if (coinsText != null)
        {
            coinsText.text = $"Coins: {SaveSystem.Instance.CurrentPlayerData.GetResourceAmount(ResourceType.Coin)}";
        }
    }

    public void ShowPlantingUI(int tileX, int tileY)
    {
        selectedTileX = tileX;
        selectedTileY = tileY;
        
        // Show crop planting panel
        if (cropPlantingPanel != null)
        {
            cropPlantingPanel.SetActive(true);
        }
    }

    private void PlantSelectedCrop(CropType cropType)
    {
        // Try to plant the selected crop
        bool success = FarmGrid.Instance.PlantCrop(selectedTileX, selectedTileY, cropType);
        
        if (success)
        {
            // Hide planting panel
            if (cropPlantingPanel != null)
            {
                cropPlantingPanel.SetActive(false);
            }
            
            // Update player info
            UpdatePlayerInfo();
        }
    }

    private void StartPlacingBuilding(BuildingType buildingType)
    {
        // Start building placement
        BuildingSystem.Instance.StartPlacingBuilding(buildingType);
        
        // Hide building panel
        if (buildingPlacementPanel != null)
        {
            buildingPlacementPanel.SetActive(false);
        }
    }

    public void ShowCropInfo(int tileX, int tileY)
    {
        // Get crop data
        CropTile tile = FarmGrid.Instance.GetCropTile(tileX, tileY);
        if (tile == null || tile.cropType == CropType.None)
        {
            return;
        }
        
        CropData cropData = FarmGrid.Instance.GetCropData(tile.cropType);
        if (cropData == null)
        {
            return;
        }
        
        // Update crop info panel
        if (cropNameText != null)
        {
            cropNameText.text = cropData.cropName;
        }
        
        if (cropGrowthText != null)
        {
            string stateText = tile.cropState.ToString();
            int growthPercent = Mathf.RoundToInt(tile.growthProgress * 100);
            cropGrowthText.text = $"State: {stateText} ({growthPercent}%)";
        }
        
        if (cropGrowthBar != null)
        {
            cropGrowthBar.value = tile.growthProgress;
        }
        
        // Show crop info panel
        if (cropInfoPanel != null)
        {
            cropInfoPanel.SetActive(true);
        }
    }

    private void ShowCropMenu()
    {
        // Show crop planting panel
        if (cropPlantingPanel != null)
        {
            cropPlantingPanel.SetActive(true);
        }
        
        // Hide other panels
        if (buildingPlacementPanel != null) buildingPlacementPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    private void ShowBuildingMenu()
    {
        // Show building placement panel
        if (buildingPlacementPanel != null)
        {
            buildingPlacementPanel.SetActive(true);
        }
        
        // Hide other panels
        if (cropPlantingPanel != null) cropPlantingPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    private void ShowInventoryPanel()
    {
        // Update inventory items
        UpdateInventoryItems();
        
        // Show inventory panel
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }
        
        // Hide other panels
        if (cropPlantingPanel != null) cropPlantingPanel.SetActive(false);
        if (buildingPlacementPanel != null) buildingPlacementPanel.SetActive(false);
    }

    private void UpdateInventoryItems()
    {
        if (inventoryItemsContainer == null || inventoryItemPrefab == null)
        {
            return;
        }
        
        // Clear existing items
        foreach (Transform child in inventoryItemsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Get player inventory
        PlayerData playerData = SaveSystem.Instance.CurrentPlayerData;
        if (playerData == null || playerData.inventory == null)
        {
            return;
        }
        
        // Get selected category
        string selectedCategory = "All";
        if (inventoryCategoryDropdown != null && inventoryCategoryDropdown.options.Count > 0)
        {
            selectedCategory = inventoryCategoryDropdown.options[inventoryCategoryDropdown.value].text;
        }
        
        // Add items to inventory panel
        foreach (var item in playerData.inventory)
        {
            ResourceType resourceType = item.Key;
            int amount = item.Value;
            
            // Skip if amount is 0
            if (amount <= 0) continue;
            
            // Filter by category if needed
            if (selectedCategory != "All")
            {
                // Simple category check - can be expanded
                if (resourceType.ToString() != selectedCategory && 
                    !resourceType.ToString().StartsWith(selectedCategory))
                {
                    continue;
                }
            }
            
            // Create inventory item
            GameObject itemObj = Instantiate(inventoryItemPrefab, inventoryItemsContainer);
            
            // Set item name
            TextMeshProUGUI itemNameText = itemObj.transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
            if (itemNameText != null)
            {
                itemNameText.text = resourceType.ToString();
            }
            
            // Set item amount
            TextMeshProUGUI itemAmountText = itemObj.transform.Find("ItemAmount")?.GetComponent<TextMeshProUGUI>();
            if (itemAmountText != null)
            {
                itemAmountText.text = amount.ToString();
            }
            
            // Set item icon if available
            Image itemIcon = itemObj.transform.Find("ItemIcon")?.GetComponent<Image>();
            if (itemIcon != null)
            {
                // Try to get resource sprite - would need a resource sprite database
                // For now, just use a placeholder or leave as is
            }
            
            // Add drag functionality if needed
            DraggableItem draggable = itemObj.AddComponent<DraggableItem>();
            draggable.Initialize(resourceType, amount);
        }
    }

    private void FilterInventoryByCategory(int categoryIndex)
    {
        // Update inventory with selected filter
        UpdateInventoryItems();
    }

    private void ShowShopPanel()
    {
        // Shop functionality would be implemented here
        Debug.Log("Shop panel not implemented yet");
    }
    
    // Helper class for draggable inventory items
    private class DraggableItem : MonoBehaviour
    {
        private ResourceType resourceType;
        private int amount;
        private Vector3 originalPosition;
        private Transform originalParent;
        private bool isDragging = false;
        
        public void Initialize(ResourceType type, int qty)
        {
            resourceType = type;
            amount = qty;
        }
        
        private void Start()
        {
            originalPosition = transform.position;
            originalParent = transform.parent;
        }
        
        public void OnBeginDrag()
        {
            isDragging = true;
            originalPosition = transform.position;
            
            // Create a visual copy that follows the mouse
            transform.SetParent(UIManager.Instance.transform);
        }
        
        public void OnDrag()
        {
            if (isDragging)
            {
                transform.position = Input.mousePosition;
            }
        }
        
        public void OnEndDrag()
        {
            isDragging = false;
            
            // Check if dropped on a valid target
            // This would be implemented based on game mechanics
            
            // Reset position if not dropped on valid target
            transform.SetParent(originalParent);
            transform.position = originalPosition;
        }
    }
} 