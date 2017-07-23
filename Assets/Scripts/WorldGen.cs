using UnityEngine;
using UnityEngine.SceneManagement;
using LibNoise;
using LibNoise.Generator;
using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;

public class WorldGen : MonoBehaviour
{
    //Perlin noise specific  variables for initialising my default settings.
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
    float _persistence = 0.5f;

    //Declares all world space types
    public GameObject[] WaterDeepArray;     //Declaring the variable as public makes it a global
    public GameObject[] WaterArray;         //allowing me too access it at any point.
    public GameObject[] WaterShallowArray;  //These store all of the tiles for each area thus so
    public GameObject[] SandArray;          //one can be selected and placed.
    public GameObject[] GrassPlainsArray;
    public GameObject[] GrassArray;
    public GameObject[] GrassWoodlandArray;
    public GameObject[] RockCliffsArray;
    public GameObject[] RockArray;
    public GameObject[] SnowArray;

    //Declare world space
    public string worldName; //Stores the current world name
    public bool IsNewWorld; //Decides how the world is going to be loaded.
    public int mapWidth = 256; //Sets the maximum x value
    public int mapHeight = 256; //Sets the maximum y value
    public float tileSize = 1.28f; //Sets the tile size
    public int radius; //store the radius of the circle for terrain fall off
    public Vector2 centre; //stores the centre of the radius of fall off
    public GameObject[,] map; //Creates and array to store all of the tiles in.
    public double elevation; //store the default elevation of

    public GameObject CurrentCamera; //gives access too the current camera
    public Vector3 nextCameraLoc; //stores where the camera needs to move too for interpolation
    public bool ButtonLeftPressed = false; //these store whether the player has pressed a button
    public bool ButtonRightPressed = false;
    public bool ButtonUpPressed = false;
    public bool ButtonDownPressed = false;
    public bool ButtonIncreasePressed = false;
    public bool ButtonDecreasedPressed = false; //emd button presses

    public Vector2 mainGameLoc; //holds the integer non scaled player location for tile references.

    // Use this for initialization
    void Start ()
    {
        
        //Initailise map vectors
        map = new GameObject[mapWidth, mapHeight];          //Creates the map array
        centre = new Vector2(mapWidth / 2, mapHeight / 2);  //Computes the centre of the map from the world size
        radius = mapWidth / 2;                              //Computes the radius of the map fall off.

        worldName = GetLevelName();         //Gets the world name
        IsNewWorld = GetLoadingAction();    //Gets the loading action such that then the world can be saved or loaded.

        //Checked if a world needs generating or loading.
        if (IsNewWorld == true)
        {
            NewWorld(); //Calls the new world function.
        }
        else
        {
            LoadWorld(); //Calls the load world function.
        }
    }

    //function the returns the tile at the given location.
    public GameObject GetTileAt(int x, int y)
    {
        if (x < 0 || y < 0 || x > mapWidth || y > mapHeight)
        {
            Debug.LogWarning("X or Y coordinate is out of bounds!");
            return null;
        }
        return map[x, y];
    }

    // Update is called once per frame
    void Update ()
    {
        //Checks for any user inputs per frame as so they are not missed.
        movement();     //Calls the movement check function
        viewScale();    //Calls the view scale check function
        interaction();  //Calls the interaction function
    }

    //function to change the level to that of a dungeon.
    public void ChangeLevel(string type)
    {
        SetDungeonType(type); //Sets the type of dungeon that is to be created.
        SaveWorld(); //Saves the current world and the associated data.
        SceneManager.LoadScene("world_dungeon"); //Loads the dungeon scene.
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("world_dungeon")); //Sets the dungeon scene as active.
    }

    //function to create a new world.
    public void NewWorld()
    {
        //Sets up the perlin instance as defined above.
        var perlin = new Perlin();          //Creates a new instance of perlin noise
        perlin.OctaveCount = _octaveCount;  //Sets the octave count
        perlin.Frequency = _frecuency;      //Sets the frquency
        perlin.Persistence = _persistence;  //Sets the persistence

        perlin.Seed = GetLevelSeed();       //Sets the seed for the world.

        //If the seed entry was left blank generate a random seed.
        if (perlin.Seed == 0)
        {
            perlin.Seed = UnityEngine.Random.seed; //Gets and sets the random seed.
        }

        var heightMapBuilder = new Noise2D(mapHeight, mapWidth, perlin); //Screates an instance of the heightmap builder from the perlin noise
        heightMapBuilder.GeneratePlanar(_west, _east, _north, _south); //Generates a planar image.

        var image = heightMapBuilder.GetTexture(_gradient); //Makes the colour range of the heighmap too be grayscale and exports it as an image

        //Will assign a sprite to each x and y value
        for (int y = 0; y < mapHeight; y++) //Starts an itteration for all y coordinates
        {
            for (int x = 0; x < mapHeight; x++) //Starts an itteration of all x coordinates for the given y coordinate
            {
                //Creates a tile at the given location
                double e = image.GetPixel(x, y).grayscale; //Gets the greyscale value of the given pixel

                map[x, y] = TileSelector(e, x, y, mapWidth, centre); //Selects a tile based upon the height value of the pixel
            }
        }

        mainGameLoc = new Vector2(128, 128); //Sets the main game location to be the centre of the map.

        CurrentCamera.transform.position = new Vector3(128 * 1.28f, 128 * 1.28f, -10); //Sets the camera lcoation to be the centre of the map.
        nextCameraLoc = CurrentCamera.transform.position; //Sets the next loaction of the camera to be the current as it is not moving.

        SaveWorld(); //Saves the newly generated world.
    }

    //function to load a previously generated and saved world.
    public void LoadWorld()
    {
        //Check too see whether the save file exists as for error correction.
        if (File.Exists(Application.persistentDataPath + "/saves/" + worldName + ".corSAV"))
        {
            BinaryFormatter bf = new BinaryFormatter(); //Creates a new instance of a binary formatter to format the data back into the Save Structure class.
            FileStream file = File.Open(Application.persistentDataPath + "/saves/" + worldName + ".corSAV", FileMode.Open); //Creates a new file stream to the given save file
            SaveStructure SaveData = (SaveStructure)bf.Deserialize(file); //converts the raw data back into the save structure class

            file.Close(); //Closes the filestream

            //Itteratuib through the 2d array
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    //Loads and places the tile at the given location.
                    map[x, y] = TileLoader(x, y, SaveData.map[x, y]);
                }
            }

            mainGameLoc = new Vector2(SaveData.mainGamePosX, SaveData.mainGamePosY); //reads the current main game location from the save file.

            CurrentCamera.transform.position = new Vector3(SaveData.playerPositionX, SaveData.playerPositionY, -10); //gets the current camera location.
            nextCameraLoc = CurrentCamera.transform.position; //sets the camera location as the current camera location.
        }

        else
        {
            NewWorld(); //Calls new world if there is no world save present.
        }
    }

    //Tile selector function to pick a random tile based upon the given height based around terrain regions.
    public GameObject TileSelector(double e, int x, int y, int mapWidth, Vector2 centre)
    {
        GameObject tile = null; //Creates an empty variable of tile to store the chosen tile.

        //Selects a tile based upon the given height.
        float d = Vector2.Distance(new Vector2(x, y), centre) / (mapWidth / 2); //calculates the disntance from the centre of the current location.
        if (new Vector2(x, y) != centre)
        {
            e += (0.05 - 1.02 * (Mathf.Pow(d, 2.0f))); //Applies the radial and exponential graduatational fall off too the current height
        }

        if (e < -0.25) //Comparing the elevation value with the value for the current region.
        {
            tile = InstantiateFromArray(WaterDeepArray, x * tileSize, y * tileSize); //Selects a random tile from the passed array.
        }

        else if (e < 0.0)
        {
            tile = InstantiateFromArray(WaterArray, x * tileSize, y * tileSize);
        }

        else if (e < 0.03)
        {
            tile = InstantiateFromArray(WaterShallowArray, x * tileSize, y * tileSize);
        }

        else if (e < 0.1)
        {
            tile = InstantiateFromArray(SandArray, x * tileSize, y * tileSize);
        }

        else if (e < 0.2)
        {
            tile = InstantiateFromArray(GrassPlainsArray, x * tileSize, y * tileSize);
        }

        else if (0.45 < e & 0.5 >= e || 0.3 < e & 0.35 >= e)
        {
            tile = InstantiateFromArray(GrassWoodlandArray, x * tileSize, y * tileSize);
        }

        else if (e >= 0.2 & e <= 0.3 || e > 0.35 & e <= 0.45 || e > 0.5 & e <= 0.525)
        {
            tile = InstantiateFromArray(GrassArray,x * tileSize, y * tileSize);
        }

        else if (0.538 >= e)
        {
            tile = InstantiateFromArray(RockCliffsArray, x * tileSize, y * tileSize);
        }

        else if (0.525 < e & 0.537 >= e || 0.538 < e & 0.55 >= e)
        {
            tile = InstantiateFromArray(RockCliffsArray, x * tileSize, y * tileSize);
        }

        else if (e < 0.7)
        {
            tile = InstantiateFromArray(RockArray, x * tileSize, y * tileSize);
        }

        else if (e >= 0.7)
        {
            tile = InstantiateFromArray(SnowArray, x * tileSize, y * tileSize);
        }

        elevation = e;

        return tile; //returns the given tile.
    }

    //Places a tile at the given location based upon the given string.
    public GameObject TileLoader(float x, float y, string tileName)
    {
        //Scales the location by the tile size.
        x *= 1.28f;
        y *= 1.28f;

        GameObject tile = null;

        //Places the tile of which the passed string corresponds.
        switch(tileName)
        {
            case "water_deep_sprite(Clone) (UnityEngine.GameObject)": //Checks whethere the string is equal too this string.
                tile = Instantiate(WaterDeepArray[0], new Vector3(x, y, 0), Quaternion.identity) as GameObject; //Places a tile at the given location
                break;
            case "water_sprite(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(WaterArray[0], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "water_shallow_sprite(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(WaterShallowArray[0], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "floor_sand_stone_sprite_0(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SandArray[0], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "floor_sand_stone_sprite_1(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SandArray[1], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "floor_sand_stone_sprite_2(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SandArray[2], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "floor_sand_stone_sprite_3(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SandArray[3], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "floor_sand_stone_sprite_4(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SandArray[4], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "floor_sand_stone_sprite_5(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SandArray[5], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "floor_sand_stone_sprite_6(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SandArray[6], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "floor_sand_stone_sprite_7(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SandArray[7], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "floor_sand_palm_sprite_0(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SandArray[8], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "floor_sand_palm_sprite_1(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SandArray[9], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "floor_sand_palm_sprite_2(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SandArray[10], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "floor_sand_palm_sprite_3(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SandArray[11], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "floor_sand_palm_sprite_4(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SandArray[12], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "floor_sand_ruin_sprite_0(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SandArray[13], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "floor_sand_ruin_sprite_1(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SandArray[14], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "floor_sand_ruin_sprite_2(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SandArray[15], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "floor_sand_ruin_sprite_3(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SandArray[16], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "grass_plains_sprite_0(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(GrassPlainsArray[0], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "grass_plains_sprite_1(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(GrassPlainsArray[1], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "grass_plains_sprite_2(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(GrassPlainsArray[2], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "grass_plains_cave_sprite_0(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(GrassPlainsArray[3], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "grass_plains_ruins_sprite_0(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(GrassPlainsArray[4], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "grass_plains_ruins_sprite_1(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(GrassPlainsArray[5], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "grass_plains_ruins_sprite_2(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(GrassPlainsArray[6], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "grass_plains_ruins_sprite_3(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(GrassPlainsArray[7], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "grass_sprite_0(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(GrassArray[0], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "grass_sprite_1(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(GrassArray[1], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "grass_sprite_2(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(GrassArray[2], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "grass_cave_sprite_0(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(GrassArray[3], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "grass_ruins_sprite_0(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(GrassArray[4], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "grass_ruins_sprite_1(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(GrassArray[5], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "grass_trees_temp_sprite(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(GrassWoodlandArray[0], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "cliffs_grass_sprite_0(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(RockCliffsArray[0], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "cliffs_grass_sprite_1(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(RockCliffsArray[1], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "cliffs_grass_sprite_2(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(RockCliffsArray[2], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "cobble_sprite_1(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(RockArray[0], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "cobble_sprite_2(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(RockArray[1], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "cobble_sprite_3(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(RockArray[2], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "cobble_sprite_4(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(RockArray[3], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "cobble_sprite_5(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(RockArray[4], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "cobble_sprite_6(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(RockArray[5], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "rock_cave_sprite(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(RockArray[6], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "ice_sprite_0(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SnowArray[0], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "ice_sprite_1(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SnowArray[1], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "ice_sprite_2(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SnowArray[2], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "ice_sprite_3(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SnowArray[3], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
            case "ice_cave_sprite_0(Clone) (UnityEngine.GameObject)":
                tile = Instantiate(SnowArray[4], new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                break;
        }

        return tile;
    }

    //Function to choose a random tile from the passed array.
    public GameObject InstantiateFromArray(GameObject[] prefabs, float xCoord, float yCoord)
    {
        int randomIndex = UnityEngine.Random.Range(0, prefabs.Length); //picks a random number between the 0 and the max index in the array.
        
        Vector3 position = new Vector3(xCoord, yCoord, 0f); //Creates a 3d vector from the passed floats.
        
        GameObject tileInstance = Instantiate(prefabs[randomIndex], position, Quaternion.identity) as GameObject; //places the sprite in the world space.
        
        tileInstance.transform.parent = transform; //sets the tiles parent

        return tileInstance; //returns the tile.
    }

    //function to save the current world state.
    public void SaveWorld()
    {
        //error correction in the case of there being no worldName.
        if (worldName == "")
        {
            worldName = "autosave";
        }

        System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/saves"); //making sure that the saves directory does exist.

        BinaryFormatter bf = new BinaryFormatter(); //Creates a new instance of the binary formatter
        FileStream file = File.Open(Application.persistentDataPath + "/saves/" + worldName + ".corSAV", FileMode.Create);

        SaveStructure SaveData = new SaveStructure();
        SaveData.worldName = worldName;
        SaveData.playerPositionX = CurrentCamera.transform.position.x;
        SaveData.playerPositionY = CurrentCamera.transform.position.y;

        SaveData.mainGamePosX = (int)mainGameLoc.x;
        SaveData.mainGamePosY = (int)mainGameLoc.y;

        SaveData.playerDungeonPositionX = 0f;
        SaveData.playerDungeonPositionY = 0f;

        SaveData.map = new string[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapWidth; y++)
            {
                SaveData.map[x, y] = map[x, y].ToString();
            }
        }

        SaveData.playerData = GetCurrentPlayerData();

        bf.Serialize(file, SaveData);
        file.Close();
    }

    public string GetLevelName()
    {
        GameObject mainMenuObject = GameObject.Find("ActionsObject");

        MainMenuActions actions = mainMenuObject.GetComponent<MainMenuActions>();

        return actions.worldName;
    }

    public int GetLevelSeed()
    {
        GameObject mainMenuObject = GameObject.Find("ActionsObject");

        MainMenuActions actions = mainMenuObject.GetComponent<MainMenuActions>();

        return actions.worldSeed;
    }

    public bool GetLoadingAction()
    {
        GameObject mainMenuObject = GameObject.Find("ActionsObject");

        MainMenuActions actions = mainMenuObject.GetComponent<MainMenuActions>();

        return actions.IsNewWorld;
    }

    public Player GetCurrentPlayerData()
    {
        GameObject mainMenuObject = GameObject.Find("PlayerHandler");

        PlayerController player = mainMenuObject.GetComponent<PlayerController>();

        return player.playerAttributes;
    }

    public void SetDungeonType(string type)
    {
        GameObject mainMenuObject = GameObject.Find("ActionsObject");

        MainMenuActions actions = mainMenuObject.GetComponent<MainMenuActions>();

        actions.DungeonType = type;
    }

    public void movement()
    {
        if (CurrentCamera.transform.position == nextCameraLoc)
        {
            if (Input.GetAxis("Horizontal") > 0.0f)
            {
                if (ButtonRightPressed == false)
                {
                    mainGameLoc += new Vector2(1, 0);
                    nextCameraLoc += new Vector3(1.28f, 0, 0);
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
                    mainGameLoc += new Vector2(-1, 0);
                    nextCameraLoc += new Vector3(-1.28f, 0, 0);
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
                    mainGameLoc += new Vector2(0, 1);
                    nextCameraLoc += new Vector3(0, 1.28f, 0);
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
                    mainGameLoc += new Vector2(0, -1);
                    nextCameraLoc += new Vector3(0, -1.28f, 0);
                }
                ButtonDownPressed = true;
            }
            else
            {
                ButtonDownPressed = false;
            }
        }

        CurrentCamera.transform.position = Vector3.MoveTowards(CurrentCamera.transform.position, nextCameraLoc, 0.16f);
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
            GameObject FetchedTile;
            int x = (int)(mainGameLoc.x);
            int y = (int)(mainGameLoc.y);
            FetchedTile = GetTileAt(x, y);

            string cmp = FetchedTile.ToString();

            if (cmp.Contains("rock_cave"))
            {
                ChangeLevel("rock_cave");
            }
            else if (FetchedTile.ToString().Contains("grass_cave"))
            {
                ChangeLevel("grass_cave");
            }
            else if (FetchedTile.ToString().Contains("grass_plains_cave"))
            {
                ChangeLevel("grass_plains_cave");
            }
            else if (FetchedTile.ToString().Contains("ice_cave"))
            {
                ChangeLevel("ice_cave");
            }
            else if (FetchedTile.ToString().Contains("sand_ruin"))
            {
                ChangeLevel("sand_ruin");
            }
            else if (FetchedTile.ToString().Contains("grass_ruin"))
            {
                ChangeLevel("grass_ruin");
            }
            else if (FetchedTile.ToString().Contains("grass_plains_ruin"))
            {
                ChangeLevel("grass_ruin");
            }
        }
    }
}

[Serializable]
class SaveStructure
{
    public string worldName { get; set; }
    public float playerPositionX { get; set; }
    public float playerPositionY { get; set; }
    public int mainGamePosX { get; set; }
    public int mainGamePosY { get; set; }
    public float playerDungeonPositionX { get; set; }
    public float playerDungeonPositionY { get; set; }
    public string[,] map { get; set; }

    public Player playerData { get; set; }
}