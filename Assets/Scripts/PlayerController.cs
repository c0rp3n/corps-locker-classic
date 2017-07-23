using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {

    //Initailising gameobjects to store the availible player sprites.
    public GameObject[] PlayerSprites;
    public GameObject[] ArmorSprites;
    public GameObject[] WeaponSprites;

    //Creating an instance of the player class to hold all relevant data.
    public Player playerAttributes;

    //creating gameobjects for the current player sprites.
    public GameObject playerShadow;
    public GameObject playerBody;

    //Store the value of the players starting loc
    public Vector3 PlayerStartingLoc;

    //Allows access to the camera object.
    public GameObject CurrentCamera;

    void Start ()
    {
        //Create an instance of the scene manager so I can check whether this is the main world or a dungeon.
        Scene ActiveScene = SceneManager.GetActiveScene();
        if (ActiveScene.name == "world_main")
        {
            //Setting the starting loc variable to the middle of the main world space
            PlayerStartingLoc = new Vector3(163.84f, 163.84f, transform.position.z);

            //Checking the loading action to fetch the right data.
            if (GetLoadingAction() == true)
            {
                playerAttributes = FetchPlayerCreation();
            }
            else
            {
                playerAttributes = GetPlayerData();
            }
        }
        else
        {
            //Setting the starting loc to the middle of a dungeon.
            PlayerStartingLoc = new Vector3(40.96f, 40.96f, transform.position.z);

            playerAttributes = GetPlayerData();
        }

        //Calls the function to place the players sprites once the data has been fetched.
        PlacePlayerObjects();
    }
	
	void Update ()
    {
        //Called once per frame sets the location of the player and its related sprites too the locaiton of the camera.
        Vector3 NewPos = new Vector3(CurrentCamera.transform.position.x, CurrentCamera.transform.position.y, 0);
        playerShadow.transform.position = NewPos;
        playerBody.transform.position = NewPos;
    }

    //Function for fetching the players details on first load from the world creation.
    public Player FetchPlayerCreation()
    {
        //Creating a temporary instance of the player class.
        Player PlayerData = new Player();

        //Fetching the data from the player creation by calling the neccesary functions.
        PlayerData.playerName = GetPlayerName();
        PlayerData.playerRace = GetPlayerRace();
        PlayerData.playerClass = GetPlayerClass();
        PlayerData.playerGender = GetPlayerGender();

        //Creating an integer array for the players current experience.
        PlayerData.playerExperience = new int[2];
        PlayerData.playerExperience[0] = 0;         //Current Experience.
        PlayerData.playerExperience[1] = 100;       //Experience needed for the next level.

        //Setting the current players level
        PlayerData.playerLevel = 1;

        //Setting the players health.
        IntRange BasePlayerHealthRange = new IntRange(50, 150);     //Generating a random integer for the players health between 50 and 100.
        PlayerData.playerHealth = BasePlayerHealthRange.Random;     //Setting the current health to be the max
        PlayerData.playerMaxHealth = PlayerData.playerHealth;       //Setting the max health to be the curent health

        //Setting the players stats by calling the neccesary function.
        PlayerData.playerStats = generatePlayerStats(PlayerData.playerClass);

        //Setting the players damage by calling the damage generator.
        PlayerData.playerDamage = damageGenerator(PlayerData.playerRace, PlayerData.playerStats[2]);

        //Set the base equipment stats by calling the function which returns an integer array.
        PlayerData.playerEquipementStats = generateBaseEquipmentStats();

        //Setting the players gold / money
        IntRange GoldRange = new IntRange(50, 150);     //Creating a random integer selector between 50 and 150
        PlayerData.playerGold = GoldRange.Random;       //Picking a random value from the random integer selector.

        //Creating an string array for the players current inventory.
        PlayerData.playerInventory = new string[16, 16];    //Creating an array of size 16 x 16.

        return PlayerData; //Returning the player class.
    }

    //procedure to place the players sprites.
    public void PlacePlayerObjects()
    {
        //Creates a new gameobject in the current worldspace referencing the shadow sprite.
        playerShadow = Instantiate(PlayerSprites[0], PlayerStartingLoc, Quaternion.identity) as GameObject;

        //Sets the base integer for the player sprite
        int index = 2;

        //A switch to set the player sprite based upon the players race the value will be set.
        switch (playerAttributes.playerRace)
        {
            case (Player.raceType.dwarf):
                index = 2;
                break;
            case (Player.raceType.elf):
                index = 4;
                break;
            case (Player.raceType.halfling):
                index = 6;
                break;
            case (Player.raceType.human):
                index = 8;
                break;
            case (Player.raceType.orc):
                index = 10;
                break;
        }

        //Checks if the player is female
        if (playerAttributes.playerGender == Player.genderType.female)
        {
            index -= 1; //taking one from the value index so that a female sprite is placed.
        }

        //Creates a new gameobject in the current worldspace referencing the chosen sprite.
        playerBody = Instantiate(PlayerSprites[index], PlayerStartingLoc, Quaternion.identity) as GameObject;
    }

    /// <summary>
    /// Theses function all operate in the same way but return a different value
    /// First they connect too the actions object which stores the player creation data
    /// Then select main menu actions scripts
    /// And finally return the appropriate value.
    /// </summary>
    /// <returns></returns>
    public string GetPlayerName()
    {
        GameObject mainMenuObject = GameObject.Find("ActionsObject");

        MainMenuActions actions = mainMenuObject.GetComponent<MainMenuActions>();

        return actions.PlayerName;
    }

    public Player.raceType GetPlayerRace()
    {
        GameObject mainMenuObject = GameObject.Find("ActionsObject");

        MainMenuActions actions = mainMenuObject.GetComponent<MainMenuActions>();

        return actions.PlayerRace;
    }

    public Player.classType GetPlayerClass()
    {
        GameObject mainMenuObject = GameObject.Find("ActionsObject");

        MainMenuActions actions = mainMenuObject.GetComponent<MainMenuActions>();

        return actions.PlayerClass;
    }

    public Player.genderType GetPlayerGender()
    {
        GameObject mainMenuObject = GameObject.Find("ActionsObject");

        MainMenuActions actions = mainMenuObject.GetComponent<MainMenuActions>();

        return actions.PlayerGender;
    }

    public Player GetPlayerData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/saves/" + GetLevelName() + ".corSAV", FileMode.Open);
        SaveStructure SaveData = (SaveStructure)bf.Deserialize(file);
        file.Close();

        return SaveData.playerData;
    }

    public string GetLevelName()
    {
        GameObject mainMenuObject = GameObject.Find("ActionsObject");

        MainMenuActions actions = mainMenuObject.GetComponent<MainMenuActions>();

        return actions.worldName;
    }

    public bool GetLoadingAction()
    {
        GameObject mainMenuObject = GameObject.Find("ActionsObject");

        MainMenuActions actions = mainMenuObject.GetComponent<MainMenuActions>();

        return actions.IsNewWorld;
    }
    //End of rep.

    //Function to set the players location.
    public void setPlayerLocation(Vector2 newLoc)
    {
        //Creates a 3d vector from the passed 2d vector.
        Vector3 playerLocNew = new Vector3(newLoc.x, newLoc.y, transform.position.z);

        //Moves the player sprites to the new location.
        playerShadow.transform.position = playerLocNew;
        playerBody.transform.position = playerLocNew;
    }

    public int[] generatePlayerStats(Player.classType playerClass)
    {
        int[] newPlayerStats = new int[8];
        //Stat Table
        // 0 - Stamina
        // 1 - Magicka
        // 2 - Strength
        // 3 - Fortitude
        // 4 - Blade
        // 5 - Blunt
        // 6 - Block
        // 7 - Archery

        //Setting the values based upon the players class.
        switch(playerClass)
        {
            case (Player.classType.archer):
                newPlayerStats[0] = 3;
                newPlayerStats[1] = 3;
                newPlayerStats[2] = 3;
                newPlayerStats[3] = 3;
                newPlayerStats[4] = 3;
                newPlayerStats[5] = 3;
                newPlayerStats[6] = 3;
                newPlayerStats[7] = 3;
                break;
        }

        //Returning the players new stats.
        return newPlayerStats;
    }

    //Creares and integer array for equipment stats.
    public int[] generateBaseEquipmentStats()
    {
        //creating the integer array of size 6.
        int[] equipmentStats = new int[6];

        //type   - name         -   place in array
        //armour - boot         -   0
        //armour - couriass     -   1
        //armour - glove        -   2
        //armour - helmet       -   3
        //armour - shield       -   4

        //weapon - damage boost -   5

        //itterates throught the array setting all values to the default 1.
        for (int i = 0; i < 6; i++)
        {
            equipmentStats[i] = 1;
        }

        return equipmentStats;
    }

    //returns the current value of the players damage based upon the given parameters.
    public int damageGenerator(Player.raceType playerRace, int playerStrength)
    {
        //Creates a random number generator for the base damage.
        IntRange baseDamage = new IntRange(1, 3);

        //Creates a defualt random gemerator for the race value generator..
        IntRange damageRaceMultiplier = new IntRange(1, 1);

        //Sets the value of the damgeRaceMul based upon the players race.
        switch (playerRace)
        {
            case (Player.raceType.dwarf):
                damageRaceMultiplier = new IntRange(3, 4);
                break;
            case (Player.raceType.elf):
                damageRaceMultiplier = new IntRange(3, 4);
                break;
            case (Player.raceType.halfling):
                damageRaceMultiplier = new IntRange(1, 3);
                break;
            case (Player.raceType.human):
                damageRaceMultiplier = new IntRange(2, 4);
                break;
            case (Player.raceType.orc):
                damageRaceMultiplier = new IntRange(4, 5);
                break;
        }

        //return a integer based upon the generated values.
        return (baseDamage.Random * damageRaceMultiplier.Random * playerStrength);
    }

    //Allows the player to gain experience
    public void playerGainExperience(int gainedExperience)
    {
        //Adds the gained exeprience to the current experience.
        playerAttributes.playerExperience[0] += gainedExperience;
        //Calls the experience check to see whether the player should level up.
        experienceCheck();
    }

    //Checks as to whether the player should level up.
    public void experienceCheck()
    {
        //If the current experience is greater than or equal too the experience needed the level up call the level up function.
        if (playerAttributes.playerExperience[0] >= playerAttributes.playerExperience[1])
        {
            levelUp();
        }
    }

    //Function to increas values upon level up.
    public void levelUp()
    {
        playerAttributes.playerLevel += 1;                                                  //Increases the players level by one.
        playerAttributes.playerExperience[1] += 100 * (playerAttributes.playerLevel - 1);   //Increases the experince needed to level.
        playerAttributes.playerMaxHealth += 10 * (playerAttributes.playerLevel - 1);        //Increases the players max health.
        playerAttributes.playerHealth = playerAttributes.playerMaxHealth;                   //Resets the players health.
        playerAttributes.playerDamage += 5 * (playerAttributes.playerLevel - 1);            //Increases the players damage.
    }

    //function that can be called to deal damage to the player
    public void damagePlayer(int enemyDamage)
    {
        // takes the inputted vlaue from the players health based upon the given algorithm.
        playerAttributes.playerHealth -= Mathf.RoundToInt(enemyDamage * ((15 - (playerAttributes.playerEquipementStats[0] 
                                                          + playerAttributes.playerEquipementStats[1] + playerAttributes.playerEquipementStats[2]
                                                          + playerAttributes.playerEquipementStats[3] + playerAttributes.playerEquipementStats[4])) / 10));
    }

    //Automatically heals the play by 5.
    public void healPlayer()
    {
        //Checks as to see that if the health would to be increased it will not overflow.
        if (playerAttributes.playerHealth + 5 <= playerAttributes.playerMaxHealth)
        {
            playerAttributes.playerHealth += 5;     //Add 5 to the health
        }
        else
        {
            playerAttributes.playerHealth = playerAttributes.playerMaxHealth;   //Sets the players health too max.
        }
    }

    //Overload of player heal instead take an inputted value.
    public void healPlayer(int healthIncrease)
    {
        if (playerAttributes.playerHealth + healthIncrease <= playerAttributes.playerMaxHealth)
        {
            playerAttributes.playerHealth += healthIncrease;
        }
        else
        {
            playerAttributes.playerHealth = playerAttributes.playerMaxHealth;
        }
    }
}
