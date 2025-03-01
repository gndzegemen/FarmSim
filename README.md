# Farm Simulation Game

A Hay Day-like farming simulation game built with Unity. This project implements core mechanics for a farming game including crop planting, building placement, resource production, and inventory management.

## Features

### Backend Systems
- **Save System**: Stores and loads player data including name, level, money, and inventory
- **Resource Production**: Automatically generates resources over time, even when the game is closed
- **Serialization**: All game data is saved using Unity's PlayerPrefs for persistence

### Gameplay Systems
- **Crop System**: Plant, grow, and harvest different types of crops on a grid-based farm
- **Building System**: Place and relocate buildings that boost resource production
- **Inventory Management**: Collect and manage various resources

### User Interface
- **Main Game UI**: Top panel showing player stats and bottom menu for game functions
- **Inventory Panel**: View and manage collected resources by category
- **Building and Crop Placement**: Intuitive UI for placing buildings and planting crops

## Setup Instructions

1. **Open the project in Unity**
   - Recommended Unity version: 2020.3 or newer
   - Make sure TextMeshPro package is installed

2. **Set up the scene**
   - Create a new scene or use the provided one
   - Add the following prefabs to your scene:
     - GameManager
     - FarmGrid
     - BuildingSystem
     - UIManager

3. **Configure the components**
   - Assign the required prefabs and sprites in the inspector:
     - For FarmGrid: Assign the tile prefab and crop sprites
     - For BuildingSystem: Assign building sprites and placement materials
     - For UIManager: Set up all UI references

4. **Run the game**
   - Press Play in the Unity Editor
   - The game should initialize all systems automatically

## How to Play

### Planting Crops
1. Click on an empty tile on the farm grid
2. Select a crop type from the planting menu
3. Wait for the crop to grow through its stages
4. Harvest when ready by clicking on the fully grown crop

### Placing Buildings
1. Click the building button in the bottom menu
2. Select a building type from the building menu
3. Position the building on a valid location (green indicator)
4. Click to place the building
5. Buildings can be relocated by dragging them to a new position

### Managing Resources
1. Resources are automatically generated over time
2. Collect resources by clicking on the resource generators
3. View your inventory by clicking the inventory button
4. Resources are used for planting crops and constructing buildings

## System Architecture

The game is built with a modular architecture using the following key components:

- **GameManager**: Coordinates initialization and communication between systems
- **SaveSystem**: Handles data persistence using PlayerPrefs
- **ResourceGenerator**: Manages automatic resource production
- **FarmGrid**: Controls the grid-based farm for planting crops
- **BuildingSystem**: Manages building placement and effects
- **UIManager**: Handles all user interface elements and interactions

Each system is designed to be independent but can communicate with other systems through well-defined interfaces.

## Customization

The game can be easily customized by:

1. **Adding new crop types**: Add new entries to the CropType enum and create corresponding CropData
2. **Adding new buildings**: Add new entries to the BuildingType enum and create corresponding BuildingData
3. **Adjusting resource production**: Modify production rates and capacities in the ResourceGenerator
4. **Changing UI elements**: Customize the UI prefabs and layouts in the UIManager

## Future Improvements

- Add more crop types and buildings
- Implement a quest/mission system
- Add a market for buying and selling resources
- Implement friend interactions and social features
- Add weather effects that impact crop growth
- Implement a day/night cycle

## Credits

- Developed as a sample project for farm simulation game mechanics
- Uses Unity's built-in systems for UI, serialization, and game logic