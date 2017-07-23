using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using LibNoise;
using LibNoise.Generator;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;

public class DungoenGen : MonoBehaviour
{
    //Declare fields for perlin noise generation
    [SerializeField]
    Gradient _gradient = GradientPresets.Grayscale;

    [SerializeField]
    float _west = 6;

    [SerializeField]
    float _east = 10;

    [SerializeField]
    float _north = 1;

    [SerializeField]
    float _south = 5;

    [SerializeField]
    int _octaveCount = 6;

    [SerializeField]
    float _frecuency = 1;

    [SerializeField]
    float _persistence = 0.75f;

    //Declare world space
    public string worldName;
    public string dungeonType;
    public int mapWidth;
    public int mapHeight;
    public GameObject[,] map;
    public float tileSize = 1.28f;

    //Declare tiles
    public GameObject[] CaveRockFloorTiles;
    public GameObject[] CaveRockWallTiles;

    public GameObject[] SandstoneFloorTiles;
    public GameObject[] SandstoneWallTiles;

    public GameObject[] CatacombsFloorTiles;
    public GameObject[] CatacombsWallTiles;

    public GameObject[] DarkFloorTiles;
    public GameObject[] DarkWallTiles;

    public GameObject[] VolcanicFloorTiles;
    public GameObject[] VolcanicWallTiles;

    public GameObject[] CrystalFloorTiles;
    public GameObject[] CrystalWallTiles;

    public GameObject[] MarbleFloorTiles;
    public GameObject[] MarbleWallTiles;

    public GameObject[] EntranceTiles;

    public GameObject[] StoneRuinFloorTiles;
    public GameObject[] StoneRuinWallTiles;

    public GameObject[] EnemySpritesStore;

    public IntRange numRooms = new IntRange(15, 20);         // The range of the number of rooms there can be.
    public IntRange roomSize = new IntRange(3, 10);         // The range of widths rooms can have.
    public IntRange roomLoc = new IntRange(0, 118);         // Sets the maximum range to be 10 less than the map size stopping any out of index errors.
    
    public TileType[,] tiles;                               // A jagged array of tile types representing the board, like a grid.

    public GameObject CurrentCamera;
    public Vector3 nextCameraLoc;
    public bool ButtonLeftPressed = false;
    public bool ButtonRightPressed = false;
    public bool ButtonUpPressed = false;
    public bool ButtonDownPressed = false;
    public bool ButtonIncreasePressed = false;
    public bool ButtonDecreasedPressed = false;

    public Vector2 mainGameLoc;

    public GameObject CurrentPlayer;

    public int enemyCount;
    public Enemy[] enemyData;
    public GameObject[] enemySprites;
    public IntRange enemyCountRange;

    public bool isPlayerTurn = true;
    public bool isStartOfTurn = true;

    public enum AdjacentTile
    {
        None ,Current ,Up, Right, Down, Left,
    }

    // Use this for initialization
    void Start ()
    {
        worldName = GetLevelName();
        dungeonType = GetDungeonType();

        if (worldName == null || worldName == "")
        {
            worldName = "autosave";
        }

        if (dungeonType.Contains("cave"))
        {
            cavegen();
        }
        if (dungeonType.Contains("ruin"))
        {
            roomSystemGen();
        }
    }

    public TileType GetTileAt(int x, int y)
    {
        if (x < 0 || y < 0 || x > mapWidth || y > mapHeight)
        {
            Debug.LogWarning("X or Y coordinate is out of bounds!");
            return TileType.Empty;
        }
        return tiles[x, y];
    }

    // Update is called once per frame
    void Update ()
    {
        turnController();
        viewScale();
        interaction();
    }

    public void ChangeLevel()
    {
        setIsNewWorld(false);
        saveNewData();

        SceneManager.LoadScene("world_main");
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("world_main"));
    }

    public string GetDungeonType()
    {
        GameObject mainMenuObject = GameObject.Find("ActionsObject");

        MainMenuActions actions = mainMenuObject.GetComponent<MainMenuActions>();

        Debug.Log("World type: " + actions.DungeonType);

        return actions.DungeonType;
    }

    public void cavegen()
    {
        mapWidth = 64;
        mapHeight = 64;
        Vector2 centre = new Vector2(mapWidth / 2, mapHeight / 2);
        tiles = new TileType[mapWidth, mapHeight];

        var perlin = new Perlin();
        perlin.OctaveCount = _octaveCount;
        perlin.Frequency = _frecuency;
        perlin.Persistence = _persistence;
        perlin.Seed = UnityEngine.Random.seed;

        var heightMapBuilder = new Noise2D(mapHeight, mapWidth, perlin);
        heightMapBuilder.GeneratePlanar(_west, _east, _north, _south);

        var image = heightMapBuilder.GetTexture(_gradient);

        for (int y = 0; y < mapHeight; y++) //Starts an itteration for all y coordinates
        {
            for (int x = 0; x < mapHeight; x++) //Starts an itteration of all x coordinates for the given y coordinate
            {
                //Creates a tile at the given location
                double e = image.GetPixel(x, y).grayscale;

                tiles[x, y] = CaveTileSelector(e, x, y, mapWidth, centre);
            }
        }

        setWallTiles(64, 64);

        tiles[mapWidth / 2, mapHeight / 2] = TileType.Entrance;

        placeTiles(64, 64);

        enemyCountRange = new IntRange(0, 4);
        createEnemies();

        mainGameLoc = new Vector2(32, 32);

        CurrentCamera.transform.position = new Vector3(32 * 1.28f, 32 * 1.28f, -10);
        nextCameraLoc = CurrentCamera.transform.position;
    }

    public TileType CaveTileSelector(double elevation, int x, int y, int radius, Vector2 centre)
    {
        TileType Tile = TileType.Empty;

        float d = Vector2.Distance(new Vector2(x, y), centre) / (mapWidth / 2);
        if (new Vector2(x, y) != centre)
        {
            elevation -= Mathf.Pow(d, 2.0f);

            if (elevation > 0.25)
            {
                Tile = TileType.Floor;
            }
        }

        return Tile;
    }

    public GameObject InstantiateFromArray(GameObject[] prefabs, float xCoord, float yCoord)
    {
        int randomIndex = UnityEngine.Random.Range(0, prefabs.Length);

        Vector3 position = new Vector3(xCoord, yCoord, 0f);

        GameObject tileInstance = Instantiate(prefabs[randomIndex], position, Quaternion.identity) as GameObject;

        tileInstance.transform.parent = transform;

        return prefabs[randomIndex];
    }

    public void roomSystemGen()
    {
        mapWidth = 128;
        mapHeight = 128;

        tiles = new TileType[128, 128];

        Vector2 lastRoomLoc = roomLocGen();
        Vector2 currentRoomLoc;
        Vector2 roomSize = roomSizeGen();
        Vector2 roomEntrance;
        Vector2 roomExit;
        Vector2 dungeonEntrance;

        roomGen(lastRoomLoc, roomSize);
        dungeonEntrance = dungeonEntranceGen(lastRoomLoc, roomSize);
        roomExit = exitGen(lastRoomLoc, roomSize);

        for (int roomNumber = 1; roomNumber < numRooms.Random; roomNumber++)
        {
            currentRoomLoc = roomLocGen();
            roomSize = roomSizeGen();
            roomGen(currentRoomLoc, roomSize);
            roomEntrance = entranceGen(lastRoomLoc, currentRoomLoc, roomSize);
            corridorGen(roomEntrance, roomExit);
            roomExit = exitGen(currentRoomLoc, roomSize);
            lastRoomLoc = currentRoomLoc;
        }

        setWallTiles(128, 128);

        tiles[(int)dungeonEntrance.x, (int)dungeonEntrance.y] = TileType.Entrance;

        placeTiles(128, 128);

        enemyCountRange = new IntRange(4, 9);
        createEnemies();

        mainGameLoc = new Vector2(dungeonEntrance.x, dungeonEntrance.y);

        CurrentCamera.transform.position = new Vector3(dungeonEntrance.x * 1.28f, dungeonEntrance.y * 1.28f, -10);
        nextCameraLoc = CurrentCamera.transform.position;
    }

    public enum TileType
    {
        Empty ,Wall, Floor, Entrance,
    }

    public Vector2 roomLocGen()
    {
        return new Vector2(roomLoc.Random, roomLoc.Random);
    }

    public Vector2 roomSizeGen()
    {
        return new Vector2(roomSize.Random, roomSize.Random);
    }

    public Vector2 dungeonEntranceGen(Vector2 _roomLocation, Vector2 _roomSize)
    {
        IntRange xRange = new IntRange((int)(_roomLocation.x + 1), (int)(_roomLocation.x + _roomSize.x - 1));
        IntRange yRange = new IntRange((int)(_roomLocation.y + 1), (int)(_roomLocation.y + _roomSize.y - 1));

        return new Vector2(xRange.Random, yRange.Random);
    }
    
    public void roomGen(Vector2 _roomLocation, Vector2 _roomSize)
    {
        int _roomWidth = (int)_roomSize.x;
        int _roomHeight = (int)_roomSize.y;

        int currentX = (int)_roomLocation.x;
        int currentY = (int)_roomLocation.y;

        int endX = currentX + _roomWidth;
        int endY = currentY + _roomHeight;

        for (int x = currentX; x <= endX; x++)
        {
            for (int y = currentY; y <= endY; y++)
            {
                tiles[x, y] = TileType.Floor;
            }
        }
    }

    public Vector2 exitGen(Vector2 _roomLocation, Vector2 _roomSize)
    {
        int _roomWidth = (int)_roomSize.x;
        int _roomHeight = (int)_roomSize.y;

        IntRange dirSelect = new IntRange(0, 3);
        IntRange widthExitRange = new IntRange(1, _roomWidth - 1);
        IntRange heightExitRange = new IntRange(1, _roomHeight - 1);
        Vector2 _roomEntrance = new Vector2();

        int dirNumber = dirSelect.Random;

        switch (dirNumber)
        {
            case (0):
                {
                    _roomEntrance = new Vector2(_roomLocation.x + widthExitRange.Random, _roomLocation.y + _roomHeight);
                    break;
                }
            case (1):
                {
                    _roomEntrance = new Vector2(_roomLocation.x + _roomWidth, _roomLocation.y + heightExitRange.Random);
                    break;
                }
            case (2):
                {
                    _roomEntrance = new Vector2(_roomLocation.x + widthExitRange.Random, _roomLocation.y);
                    break;
                }
            case (3):
                {
                    _roomEntrance = new Vector2(_roomLocation.x, _roomLocation.y + heightExitRange.Random);
                    break;
                }
        }

        return _roomEntrance;
    }

    public Vector2 entranceGen(Vector2 _roomExit, Vector2 _roomLoc, Vector2 _roomSize)
    {
        int _roomWidth = (int)_roomSize.x;
        int _roomHeight = (int)_roomSize.y;
        Vector2 _roomEntrance = new Vector2();

        IntRange widthExitRange = new IntRange(1, _roomWidth - 1);
        IntRange heightExitRange = new IntRange(1, _roomHeight - 1);

        float pathDistanceLowestPoint = Vector2.Distance(_roomExit, _roomLoc);
        float pathDistanceHighestPoint = Vector2.Distance(_roomExit, new Vector2(_roomLoc.x + _roomWidth, _roomLoc.y + _roomHeight));

        Vector2 roomPosForTest1;
        Vector2 roomPosForTest2;
        float roomPos1Distance;
        float roomPos2Distance;

        if (pathDistanceLowestPoint < pathDistanceHighestPoint)
        {
            roomPosForTest1 = new Vector2(_roomLoc.x, _roomLoc.y + 1);
            roomPosForTest2 = new Vector2(_roomLoc.x + 1, _roomLoc.y);

            roomPos1Distance = Vector2.Distance(_roomExit, roomPosForTest1);
            roomPos2Distance = Vector2.Distance(_roomExit, roomPosForTest2);

            if (roomPos1Distance < roomPos2Distance)
            {
                _roomEntrance = new Vector2(_roomLoc.x, _roomLoc.y + heightExitRange.Random);
            }
            else
            {
                _roomEntrance = new Vector2(_roomLoc.x + widthExitRange.Random, _roomLoc.y);
            }
        }
        else
        {
            roomPosForTest1 = new Vector2(_roomLoc.x + _roomWidth, _roomLoc.y + _roomHeight - 1);
            roomPosForTest2 = new Vector2(_roomLoc.x + _roomWidth - 1, _roomLoc.y + _roomHeight);

            roomPos1Distance = Vector2.Distance(_roomExit, roomPosForTest1);
            roomPos2Distance = Vector2.Distance(_roomExit, roomPosForTest2);

            if (roomPos1Distance < roomPos2Distance)
            {
                _roomEntrance = new Vector2(_roomLoc.x + _roomWidth, _roomLoc.y + _roomHeight - heightExitRange.Random);
            }
            else
            {
                _roomEntrance = new Vector2(_roomLoc.x + _roomWidth - widthExitRange.Random, _roomLoc.y + _roomHeight);
            }
        }

        return _roomEntrance;
    }

    public void corridorGen(Vector2 entrance, Vector2 exit)
    {
        Vector2 Path = new Vector2((exit.x - entrance.x), (exit.y - entrance.y));
        int currentX = System.Convert.ToInt32(entrance.x);
        int currentY = System.Convert.ToInt32(entrance.y);

        if (modulus(Path.x) < modulus(Path.y))
        {
            currentX += 1;

            int midpoint = roundFloat(modulus(Path.x / 2));

            int changeX = 1;

            int changeY = 1;

            if (Path.x < 0)
            {
                changeX = -1;
            }

            if (Path.y < 0)
            {
                changeY = -1;
            }

            for (int x = 0; x < midpoint; x++)
            {
                tiles[currentX, currentY] = TileType.Floor;
                currentX += changeX;
            }

            for (int y = 0; y < modulus(Path.y); y++)
            {
                tiles[currentX, currentY] = TileType.Floor;
                currentY += changeY;
            }

            for (int x = 0; x < modulus(Path.x) - midpoint; x++)
            {
                tiles[currentX, currentY] = TileType.Floor;
                currentX += changeX;
            }
        }
        else
        {
            currentY += 1;

            int midpoint = roundFloat(modulus(Path.y / 2));

            int changeX = 1;

            int changeY = 1;

            if (Path.x < 0)
            {
                changeX = -1;
            }

            if (Path.y < 0)
            {
                changeY = -1;
            }

            for (int y = 0; y < midpoint; y++)
            {
                tiles[currentX, currentY] = TileType.Floor;
                currentY += changeY;
            }

            for (int x = 0; x < modulus(Path.x); x++)
            {
                tiles[currentX, currentY] = TileType.Floor;
                currentX += changeX;
            }

            for (int y = 0; y < modulus(Path.y) - midpoint; y++)
            {
                tiles[currentX, currentY] = TileType.Floor;
                currentY += changeY;
            }
        }
    }

    public void placeTiles(int mapWidth, int mapHeight)
    {
        GameObject[] currentWalls = null;
        GameObject[] currentFloors = null;

        switch (dungeonType)
        {
            case "rock_cave":
                currentFloors = VolcanicFloorTiles;
                currentWalls = VolcanicFloorTiles;
                break;
            case "grass_cave":
                currentFloors = CaveRockFloorTiles;
                currentWalls = CaveRockWallTiles;
                break;
            case "grass_plains_cave":
                currentFloors = MarbleFloorTiles;
                currentWalls = MarbleWallTiles;
                break;
            case "ice_cave":
                currentFloors = CrystalFloorTiles;
                currentWalls = CrystalWallTiles;
                break;
            case "sand_ruin":
                currentFloors = SandstoneFloorTiles;
                currentWalls = SandstoneWallTiles;
                break;
            case "grass_ruin":
                currentFloors = DarkFloorTiles;
                currentWalls = DarkWallTiles;
                break;
            case "grass_plains_ruin":
                currentFloors = StoneRuinFloorTiles;
                currentWalls = StoneRuinFloorTiles;
                break;
        }

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                GameObject Tile = null;

                switch (tiles[x, y])
                {
                    case (TileType.Floor):
                        {
                            Tile = InstantiateFromArray(currentFloors, x * 1.28f, y * 1.28f);
                            break;
                        }
                    case (TileType.Entrance):
                        {
                            Tile = InstantiateFromArray(EntranceTiles, x * 1.28f, y * 1.28f);
                            break;
                        }
                    case (TileType.Wall):
                        {
                            Tile = InstantiateFromArray(currentWalls, x * 1.28f, y * 1.28f);
                            break;
                        }
                }
            }
        }
    }

    public void setWallTiles(int mapWidth, int mapHeight)
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (tiles[x, y] == TileType.Floor)
                {
                    if (x - 1 > -1)
                    {
                        if (tiles[x - 1, y] != TileType.Floor)
                        {
                            tiles[x - 1, y] = TileType.Wall;
                        }
                    }

                    if (x + 1 < mapWidth)
                    {
                        if (tiles[x + 1, y] != TileType.Floor)
                        {
                            tiles[x + 1, y] = TileType.Wall;
                        }
                    }

                    if (y - 1 > -1)
                    {
                        if (tiles[x, y - 1] != TileType.Floor)
                        {
                            tiles[x, y - 1] = TileType.Wall;
                        }
                    }

                    if (y + 1 < mapHeight)
                    {
                        if (tiles[x, y + 1] != TileType.Floor)
                        {
                            tiles[x, y + 1] = TileType.Wall;
                        }
                    }

                    if (x - 1 > -1)
                    {
                        if (y - 1 > -1)
                        {
                            if (tiles[x - 1, y - 1] != TileType.Floor)
                            {
                                tiles[x - 1, y - 1] = TileType.Wall;
                            }
                        }
                    }

                    if (x + 1 < mapWidth)
                    {
                        if (y - 1 > -1)
                        {
                            if (tiles[x + 1, y - 1] != TileType.Floor)
                            {
                                tiles[x + 1, y - 1] = TileType.Wall;
                            }
                        }
                    }

                    if (x - 1 > -1)
                    {
                        if (y + 1 < mapHeight)
                        {
                            if (tiles[x - 1, y + 1] != TileType.Floor)
                            {
                                tiles[x - 1, y + 1] = TileType.Wall;
                            }
                        }
                    }

                    if (x + 1 < mapWidth)
                    {
                        if (y + 1 < mapHeight)
                        {
                            if (tiles[x + 1, y + 1] != TileType.Floor)
                            {
                                tiles[x + 1, y + 1] = TileType.Wall;
                            }
                        }
                    }
                }
            }
        }
    }

    public float modulus(float value)
    {
        if(value < 0)
        {
            value *= -1;
        }

        return value;
    }

    public int roundFloat(float value)
    {
        return (System.Convert.ToInt32(Mathf.Round(value)));
    }

    public void playerTurnController()
    {
        if (isStartOfTurn == true)
        {
            if (CurrentCamera.transform.position == nextCameraLoc)
            {
                if (Input.GetAxis("Horizontal") > 0.0f)
                {
                    if (ButtonRightPressed == false)
                    {
                        if (playerNextMovementCheck(new Vector2(1, 0)) == true)
                        {
                            int enemyCheck = enemyExistsAtNextLocation(mainGameLoc + new Vector2(1, 0));
                            if (enemyCheck >= 0)
                            {
                                enemyData[enemyCheck].enemyHeatlth -= CurrentPlayer.GetComponent<PlayerController>().playerAttributes.playerDamage;

                                if (enemyData[enemyCheck].enemyHeatlth <= 0)
                                {
                                    CurrentPlayer.GetComponent<PlayerController>().playerGainExperience(enemyData[enemyCheck].enemyExperienceGranted);
                                }
                            }
                            else
                            {
                                mainGameLoc += new Vector2(1, 0);
                                nextCameraLoc += new Vector3(1.28f, 0, 0);

                                CurrentPlayer.GetComponent<PlayerController>().healPlayer();
                            }
                        }
                        isStartOfTurn = false;
                    }
                    ButtonRightPressed = true;
                }
                else
                {
                    ButtonRightPressed = false;
                }

                if (Input.GetAxis("Horizontal") < 0.0f)
                {
                    if (ButtonLeftPressed == false)
                    {
                        if (playerNextMovementCheck(new Vector2(-1, 0)) == true)
                        {
                            int enemyCheck = enemyExistsAtNextLocation(mainGameLoc + new Vector2(-1, 0));
                            if (enemyCheck >= 0)
                            {
                                enemyData[enemyCheck].enemyHeatlth -= CurrentPlayer.GetComponent<PlayerController>().playerAttributes.playerDamage;
                            }
                            else
                            {
                                mainGameLoc += new Vector2(-1, 0);
                                nextCameraLoc += new Vector3(-1.28f, 0, 0);

                                CurrentPlayer.GetComponent<PlayerController>().healPlayer();
                            }
                        }
                        isStartOfTurn = false;
                    }
                    ButtonLeftPressed = true;
                }
                else
                {
                    ButtonLeftPressed = false;
                }

                if (Input.GetAxis("Vertical") > 0.0f)
                {
                    if (ButtonUpPressed == false)
                    {
                        if (playerNextMovementCheck(new Vector2(0, 1)) == true)
                        {
                            int enemyCheck = enemyExistsAtNextLocation(mainGameLoc + new Vector2(0, 1));
                            if (enemyCheck >= 0)
                            {
                                enemyData[enemyCheck].enemyHeatlth -= CurrentPlayer.GetComponent<PlayerController>().playerAttributes.playerDamage;
                            }
                            else
                            {
                                mainGameLoc += new Vector2(0, 1);
                                nextCameraLoc += new Vector3(0, 1.28f, 0);

                                CurrentPlayer.GetComponent<PlayerController>().healPlayer();
                            }
                        }
                        isStartOfTurn = false;
                    }
                    ButtonUpPressed = true;
                }
                else
                {
                    ButtonUpPressed = false;
                }

                if (Input.GetAxis("Vertical") < 0.0f)
                {
                    if (ButtonDownPressed == false)
                    {
                        if (playerNextMovementCheck(new Vector2(0, -1)) == true)
                        {
                            int enemyCheck = enemyExistsAtNextLocation(mainGameLoc + new Vector2(0, -1));
                            if (enemyCheck >= 0)
                            {
                                enemyData[enemyCheck].enemyHeatlth -= CurrentPlayer.GetComponent<PlayerController>().playerAttributes.playerDamage;
                            }
                            else
                            {
                                mainGameLoc += new Vector2(0, -1);
                                nextCameraLoc += new Vector3(0, -1.28f, 0);

                                CurrentPlayer.GetComponent<PlayerController>().healPlayer();
                            }
                        }
                        isStartOfTurn = false;
                    }
                    ButtonDownPressed = true;
                }
                else
                {
                    ButtonDownPressed = false;
                }
            }
        }
        else
        {
            CurrentCamera.transform.position = Vector3.MoveTowards(CurrentCamera.transform.position, nextCameraLoc, 0.16f);

            if (CurrentCamera.transform.position == nextCameraLoc)
            {
                isStartOfTurn = true;
                isPlayerTurn = false;
            }
        }
    }

    public void playerAttackEnemy(int enemyIndex)
    {
        enemyData[enemyIndex].enemyHeatlth -= CurrentPlayer.GetComponent<PlayerController>().playerAttributes.playerDamage;

        if (enemyData[enemyIndex].enemyHeatlth <= 0)
        {
            CurrentPlayer.GetComponent<PlayerController>().playerGainExperience(enemyData[enemyIndex].enemyExperienceGranted);
        }
    }

    public int enemyExistsAtNextLocation(Vector2 nextPlayerLocation)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            if (enemyData[i].enemyLocation == new Vector3(nextPlayerLocation.x, nextPlayerLocation.y) & enemyData[i].enemyHeatlth > 0)
            {
                return i;
            }
        }

        return -1;
    }

    public void viewScale()
    {
        if (Input.GetAxis("Zoom") > 0.0f)
        {
            if (ButtonIncreasePressed == false)
            {
                CurrentCamera.GetComponent<Camera>().orthographicSize += 4;
            }
            ButtonIncreasePressed = true;
        }
        else
        {
            ButtonIncreasePressed = false;
        }

        if (Input.GetAxis("Zoom") < 0.0f)
        {
            if (ButtonDecreasedPressed == false)
            {
                if (CurrentCamera.GetComponent<Camera>().orthographicSize - 4 > 0)
                {
                    CurrentCamera.GetComponent<Camera>().orthographicSize -= 4;
                }
            }
            ButtonDecreasedPressed = true;
        }
        else
        {
            ButtonDecreasedPressed = false;
        }
    }

    public void interaction()
    {
        if (Input.GetAxis("Interact") > 0.0f)
        {
            int x = (int)(mainGameLoc.x);
            int y = (int)(mainGameLoc.y);

            if (GetTileAt(x, y) == TileType.Entrance)
            {
                ChangeLevel();
            }
        }
    }

    public bool playerNextMovementCheck(Vector3 newDisplacement)
    {
        int x = (int)(mainGameLoc.x + newDisplacement.x);
        int y = (int)(mainGameLoc.y + newDisplacement.y);

        TileType newTile = GetTileAt(x, y);
        if (newTile == TileType.Floor || newTile == TileType.Entrance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void setIsNewWorld(bool newWorldYes)
    {
        GameObject mainMenuObject = GameObject.Find("ActionsObject");

        MainMenuActions actions = mainMenuObject.GetComponent<MainMenuActions>();

        actions.IsNewWorld = newWorldYes;
    }

    public void saveNewData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/saves/" + worldName + ".corSAV", FileMode.Open);
        SaveStructure SaveData = (SaveStructure)bf.Deserialize(file);
        file.Close();

        SaveData.playerData = GetCurrentPlayerData();
        SaveData.playerDungeonPositionX = CurrentCamera.transform.position.x;
        SaveData.playerDungeonPositionY = CurrentCamera.transform.position.y;

        FileStream newFile = File.Open(Application.persistentDataPath + "/saves/" + worldName + ".corSAV", FileMode.Create);
        bf.Serialize(newFile, SaveData);
        newFile.Close();
    }

    public string GetLevelName()
    {
        GameObject mainMenuObject = GameObject.Find("ActionsObject");

        MainMenuActions actions = mainMenuObject.GetComponent<MainMenuActions>();

        return actions.worldName;
    }

    public Player GetCurrentPlayerData()
    {
        PlayerController player = CurrentPlayer.GetComponent<PlayerController>();

        return player.playerAttributes;
    }

    public void createEnemies()
    {
        enemyCount = enemyCountRange.Random;

        enemyData = new Enemy[enemyCount + 1];
        enemySprites = new GameObject[enemyCount + 1];

        for (int i = 0; i < enemyCount; i++)
        {
            enemyData[i] = randomEnemyGen(); ;
            TileType currentSelectedTile = TileType.Empty;

            int x = 0;
            int y = 0;

            IntRange mapWidthRange = new IntRange(0, mapWidth);
            IntRange mapHeightRange = new IntRange(0, mapHeight);

            while (currentSelectedTile != TileType.Floor)
            {
                x = mapWidthRange.Random;
                y = mapHeightRange.Random;
                currentSelectedTile = tiles[x, y];
            }

            enemyData[i].enemyLocation = new Vector3(x, y, 0);
            enemyData[i].enemyIsAtMaxLoc = false;

            placeEnemySprite(i);

            setNewTargetEnemyLoc(i);
            setNewNextEnemyLoc(i);
        }
    }

    public void placeEnemySprite(int currentEnemyIndex)
    {
        Vector3 tempEnemyLoc = new Vector3(enemyData[currentEnemyIndex].enemyLocation.x * 1.28f, enemyData[currentEnemyIndex].enemyLocation.y * 1.28f, 0);

        switch (enemyData[currentEnemyIndex].enemyTypeVal)
        {
            case (Enemy.EnemyType.grey_rat):
                enemySprites[currentEnemyIndex] = Instantiate(EnemySpritesStore[0], tempEnemyLoc, Quaternion.identity) as GameObject;
                break;
            case (Enemy.EnemyType.warg):
                enemySprites[currentEnemyIndex] = Instantiate(EnemySpritesStore[1], tempEnemyLoc, Quaternion.identity) as GameObject;
                break;
            case (Enemy.EnemyType.wolf):
                enemySprites[currentEnemyIndex] = Instantiate(EnemySpritesStore[2], tempEnemyLoc, Quaternion.identity) as GameObject;
                break;
        }
    }

    public Enemy randomEnemyGen()
    {
        Enemy currentEnemy = new Enemy();

        IntRange enemySelector = new IntRange(0, 2);

        currentEnemy = setEnemyStats((Enemy.EnemyType)enemySelector.Random);

        return currentEnemy;
    }

    public Enemy randomEnemyGen(Enemy.EnemyType predefinedEnemy)
    {
        Enemy currentEnemy = new Enemy();

        currentEnemy = setEnemyStats(predefinedEnemy);

        return currentEnemy;
    }

    public Enemy setEnemyStats(Enemy.EnemyType enemyTypeTemp)
    {
        Enemy currentEnemy = new Enemy();

        currentEnemy.enemyTypeVal = enemyTypeTemp;

        raceBaseData baseData = new raceBaseData();

        switch (enemyTypeTemp)
        {
            case (Enemy.EnemyType.grey_rat):
                currentEnemy.enemyHeatlth = baseData.grey_rat_health.Random;
                currentEnemy.enemyDamage = baseData.grey_rat_base_damage.Random;
                currentEnemy.enemyExperienceGranted = baseData.grey_rat_experience_granted.Random;
                break;
            case (Enemy.EnemyType.warg):
                currentEnemy.enemyHeatlth = baseData.warg_health.Random;
                currentEnemy.enemyDamage = baseData.warg_base_damage.Random;
                currentEnemy.enemyExperienceGranted = baseData.warg_experience_granted.Random;
                break;
            case (Enemy.EnemyType.wolf):
                currentEnemy.enemyHeatlth = baseData.wolf_health.Random;
                currentEnemy.enemyDamage = baseData.wolf_base_damage.Random;
                currentEnemy.enemyExperienceGranted = baseData.wolf_experience_gained.Random;
                break;
        }

        return currentEnemy;
    }

    public void turnController()
    {
        if (isPlayerTurn == true)
        {
            playerTurnController();
        }
        else
        {
            enemyTurnController();
        }
    }

    public void enemyTurnController()
    {
        bool hasATurnHappened = false;

        for (int i = 0; i < enemyCount; i++)
        {
            if (enemyData[i].enemyHeatlth > 0)
            {
                if (hasATurnHappened == false)
                {
                    if (enemyData[i].enemyHasHadTurn == false)
                    {
                        if (Vector3.Distance(enemyData[i].enemyLocation, mainGameLoc) > 1)
                        {
                            if (checkHasEnemyBeenMoved(i) == false)
                            {
                                enemyPositionUpdater(i);
                            }
                            else
                            {
                                enemyMovementController(i);
                                enemyData[i].enemyHasHadTurn = true;
                            }
                        }
                        else
                        {
                            CurrentPlayer.GetComponent<PlayerController>().damagePlayer(enemyData[i].enemyDamage);
                            enemyData[i].enemyHasHadTurn = true;
                        }

                        hasATurnHappened = true;
                    }
                }
            }
            else
            {
                enemySprites[i].transform.localScale = new Vector3(0, 0, 0);
            }
        }

        if (hasATurnHappened == false)
        {
            for (int i = 0; i < enemyCount; i++)
            {
                enemyData[i].enemyHasHadTurn = false;
            }

            isPlayerTurn = true;
            isStartOfTurn = true;
        }
    }

    public void enemyPositionUpdater(int currentEnemyIndex)
    {
        if (enemyData[currentEnemyIndex].enemyLocation != enemyData[currentEnemyIndex].nextEnemyLocation)
        {
            calculateNewEnemyLoc(currentEnemyIndex);
        }
        else
        {
            enemyData[currentEnemyIndex].enemyHasHadTurn = true;
        }
    }

    public void enemyMovementController(int currentEnemyIndex)
    {
        if (enemyData[currentEnemyIndex].enemyMoves >= enemyData[currentEnemyIndex].enemyMovesMax || enemyData[currentEnemyIndex].enemyLocation == enemyData[currentEnemyIndex].targetEnemyLocation || enemyData[currentEnemyIndex].enemyIsAtMaxLoc == true)
        {
            setNewTargetEnemyLoc(currentEnemyIndex);
            setNewNextEnemyLoc(currentEnemyIndex);
        }
        else
        {
            setNewNextEnemyLoc(currentEnemyIndex);
        }
    }

    public bool checkHasEnemyBeenMoved(int currentEnemyIndex)
    {
        if (enemyData[currentEnemyIndex].enemyLocation != enemyData[currentEnemyIndex].nextEnemyLocation)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void setNewNextEnemyLoc(int currentEnemyIndex)
    {
        float distance = modulus(Vector3.Distance(enemyData[currentEnemyIndex].enemyLocation, enemyData[currentEnemyIndex].targetEnemyLocation));
        AdjacentTile nextTile = AdjacentTile.Current;
        
        if (GetTileAt((int)enemyData[currentEnemyIndex].enemyLocation.x, (int)enemyData[currentEnemyIndex].enemyLocation.y + 1) == TileType.Floor)
        {
            float newDistance = modulus(Vector3.Distance(new Vector3(enemyData[currentEnemyIndex].enemyLocation.x, enemyData[currentEnemyIndex].enemyLocation.y + 1, 0), enemyData[currentEnemyIndex].targetEnemyLocation));
            
            if (newDistance < distance)
            {
                distance = newDistance;
                nextTile = AdjacentTile.Up;
            }
        }

        if (GetTileAt((int)enemyData[currentEnemyIndex].enemyLocation.x + 1, (int)enemyData[currentEnemyIndex].enemyLocation.y) == TileType.Floor)
        {
            float newDistance = modulus(Vector3.Distance(new Vector3(enemyData[currentEnemyIndex].enemyLocation.x + 1, enemyData[currentEnemyIndex].enemyLocation.y, 0), enemyData[currentEnemyIndex].targetEnemyLocation));
            
            if (newDistance < distance)
            {
                distance = newDistance;
                nextTile = AdjacentTile.Right;
            }
        }

        if (GetTileAt((int)enemyData[currentEnemyIndex].enemyLocation.x, (int)enemyData[currentEnemyIndex].enemyLocation.y - 1) == TileType.Floor)
        {
            float newDistance = modulus(Vector3.Distance(new Vector3(enemyData[currentEnemyIndex].enemyLocation.x, enemyData[currentEnemyIndex].enemyLocation.y - 1, 0), enemyData[currentEnemyIndex].targetEnemyLocation));
            
            if (newDistance < distance)
            {
                distance = newDistance;
                nextTile = AdjacentTile.Down;
            }
        }

        if (GetTileAt((int)enemyData[currentEnemyIndex].enemyLocation.x - 1, (int)enemyData[currentEnemyIndex].enemyLocation.y) == TileType.Floor)
        {
            float newDistance = modulus(Vector3.Distance(new Vector3(enemyData[currentEnemyIndex].enemyLocation.x - 1, enemyData[currentEnemyIndex].enemyLocation.y, 0), enemyData[currentEnemyIndex].targetEnemyLocation));
            
            if (newDistance < distance)
            {
                distance = newDistance;
                nextTile = AdjacentTile.Left;
            }
        }

        switch (nextTile)
        {
            case (AdjacentTile.Current):
                enemyData[currentEnemyIndex].nextEnemyLocation =enemyData[currentEnemyIndex].enemyLocation;
                enemyData[currentEnemyIndex].enemyIsAtMaxLoc = true;
                break;

            case (AdjacentTile.Up):
                enemyData[currentEnemyIndex].nextEnemyLocation = new Vector3(enemyData[currentEnemyIndex].enemyLocation.x, enemyData[currentEnemyIndex].enemyLocation.y + 1, 0);
                enemyData[currentEnemyIndex].enemyIsAtMaxLoc = false;
                break;

            case (AdjacentTile.Right):
                enemyData[currentEnemyIndex].nextEnemyLocation = new Vector3(enemyData[currentEnemyIndex].enemyLocation.x + 1, enemyData[currentEnemyIndex].enemyLocation.y, 0);
                enemyData[currentEnemyIndex].enemyIsAtMaxLoc = false;
                break;

            case (AdjacentTile.Down):
                enemyData[currentEnemyIndex].nextEnemyLocation = new Vector3(enemyData[currentEnemyIndex].enemyLocation.x, enemyData[currentEnemyIndex].enemyLocation.y - 1, 0);
                enemyData[currentEnemyIndex].enemyIsAtMaxLoc = false;
                break;

            case (AdjacentTile.Left):
                enemyData[currentEnemyIndex].nextEnemyLocation = new Vector3(enemyData[currentEnemyIndex].enemyLocation.x - 1, enemyData[currentEnemyIndex].enemyLocation.y, 0);
                enemyData[currentEnemyIndex].enemyIsAtMaxLoc = false;
                break;
        }
    }

    

    public void setNewTargetEnemyLoc(int currentEnemyIndex)
    {
        if (modulus(Vector2.Distance(enemyData[currentEnemyIndex].enemyLocation, mainGameLoc)) <= 10)
        {
            enemyData[currentEnemyIndex].targetEnemyLocation = mainGameLoc;

            enemyData[currentEnemyIndex].enemyMovesMax = 1;
        }

        else
        {
            IntRange enemyMovementRange = new IntRange(10, 25);
            IntRange newMovementRangeX = new IntRange(0, mapWidth);
            IntRange newMovementRangeY = new IntRange(0, mapHeight);

            enemyData[currentEnemyIndex].targetEnemyLocation = new Vector3(newMovementRangeX.Random, newMovementRangeY.Random, 0);
            
            enemyData[currentEnemyIndex].enemyMovesMax = enemyMovementRange.Random;
        }

        enemyData[currentEnemyIndex].enemyMoves = 0;
    }

    public void calculateNewEnemyLoc(int currentEnemyIndex)
    {
        Vector3 tileBasedLocation = new Vector3(enemyData[currentEnemyIndex].nextEnemyLocation.x * 1.28f, enemyData[currentEnemyIndex].nextEnemyLocation.y * 1.28f, 0);

        enemySprites[currentEnemyIndex].transform.position = Vector3.MoveTowards(enemySprites[currentEnemyIndex].transform.position, (tileBasedLocation), 0.16f);

        if (enemySprites[currentEnemyIndex].transform.position == (enemyData[currentEnemyIndex].nextEnemyLocation * 1.28f))
        {
            enemyData[currentEnemyIndex].enemyLocation = enemyData[currentEnemyIndex].nextEnemyLocation;
            enemyData[currentEnemyIndex].enemyMoves += 1;
        }
    }
}