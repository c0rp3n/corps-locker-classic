using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldCreationController : MonoBehaviour
{
    public string WorldName;
    public int WorldSeed;
    public string PlayerName;
    public Player.classType PlayerClass;

    public float WorldUIScale;
    public float PlayerUIScale;
    public float PlayerIamgeScale;

    public Button WorldNameID;
    public InputField WorldNameInput;
    public Button WorldSeedID;
    public InputField WorldSeedInput;
    public Image PlayerBase;
    public Image PlayerShadow;
    public Button PlayerNameID;
    public InputField PlayerNameInput;
    public Button PlayerRaceID;
    public Button PlayerRaceDwarf;
    public Button PlayerRaceElf;
    public Button PlayerRaceHobbit;
    public Button PlayerRaceHuman;
    public Button PlayerRaceOrc;
    public Button PlayerGenderID;
    public Button PlayerGenderMale;
    public Button PlayerGenderFemale;
    public Button PlayerClassID;
    public Button PlayerClassArcher;
    public Button PlayerClassAssassin;
    public Button PlayerClassBarbarian;
    public Button PlayerClassCrusader;
    public Button PlayerClassHealer;
    public Button PlayerClassHunter;
    public Button PlayerClassMage;
    public Button PlayerClassPaladin;
    public Button PlayerClassRanger;
    public Button PlayerClassThief;
    public Button PlayerClassWarrior;
    public Button PlayerClassWizard;

    public Sprite[] PlayerSprites;

    public Player.raceType PlayerRace;

    public Player.genderType PlayerGender;

    void Start()
    {
        WorldNameID = GameObject.Find("World_Name_Identifier").GetComponent<Button>();
        WorldNameInput = GameObject.Find("World_Name_Input_Box").GetComponent<InputField>();
        WorldSeedID = GameObject.Find("World_Seed_Identifier").GetComponent<Button>();
        WorldSeedInput = GameObject.Find("World_Seed_Input_Box").GetComponent<InputField>();
        PlayerBase = GameObject.Find("Player_Base_Image").GetComponent<Image>();
        PlayerShadow = GameObject.Find("Player_Shadow_Image").GetComponent<Image>();
        PlayerNameID = GameObject.Find("Player_Name_Identifier").GetComponent<Button>();
        PlayerNameInput = GameObject.Find("Player_Name_Input_Box").GetComponent<InputField>();
        PlayerRaceID = GameObject.Find("Race_Identifier").GetComponent<Button>();
        PlayerRaceDwarf = GameObject.Find("Race_Dwarf_Button").GetComponent<Button>();
        PlayerRaceElf = GameObject.Find("Race_Elf_Button").GetComponent<Button>();
        PlayerRaceHobbit = GameObject.Find("Race_Hobbit_Button").GetComponent<Button>();
        PlayerRaceHuman = GameObject.Find("Race_Human_Button").GetComponent<Button>();
        PlayerRaceOrc = GameObject.Find("Race_Orc_Button").GetComponent<Button>();
        PlayerGenderID = GameObject.Find("Gender_Identifier").GetComponent<Button>();
        PlayerGenderMale = GameObject.Find("Gender_Male_Button").GetComponent<Button>();
        PlayerGenderFemale = GameObject.Find("Gender_Female_Button").GetComponent<Button>();
        PlayerClassID = GameObject.Find("Class_Identifier").GetComponent<Button>();
        PlayerClassArcher = GameObject.Find("Class_Archer_Button").GetComponent<Button>();
        PlayerClassAssassin = GameObject.Find("Class_Assassin_Button").GetComponent<Button>();
        PlayerClassBarbarian = GameObject.Find("Class_Barbarian_Button").GetComponent<Button>();
        PlayerClassCrusader = GameObject.Find("Class_Crusader_Button").GetComponent<Button>();
        PlayerClassHealer = GameObject.Find("Class_Healer_Button").GetComponent<Button>();
        PlayerClassHunter = GameObject.Find("Class_Hunter_Button").GetComponent<Button>();
        PlayerClassMage = GameObject.Find("Class_Mage_Button").GetComponent<Button>();
        PlayerClassPaladin = GameObject.Find("Class_Paladin_Button").GetComponent<Button>();
        PlayerClassRanger = GameObject.Find("Class_Ranger_Button").GetComponent<Button>();
        PlayerClassThief = GameObject.Find("Class_Thief_Button").GetComponent<Button>();
        PlayerClassWarrior = GameObject.Find("Class_Warrior_Button").GetComponent<Button>();
        PlayerClassWizard = GameObject.Find("Class_Wizard_Button").GetComponent<Button>();

        WorldTabClicked();
    }

    public void WorldTabClicked()
    {
        ChangeWorldUIVisible(true);
        ChangePlayerUIVisible(false);
    }

    public void PlayerTabClicked()
    {
        ChangeWorldUIVisible(false);
        ChangePlayerUIVisible(true);
    }

    public void BackClicked()
    {
        Destroy(GameObject.Find("ActionsObject"));
        SceneManager.LoadScene("main_menu");
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("main_menu"));
    }

    public void RaceDwarfClicked()
    {
        PlayerRace = Player.raceType.dwarf;
        ChangeBaseSprite();
    }

    public void RaceElfClicked()
    {
        PlayerRace = Player.raceType.elf;
        ChangeBaseSprite();
    }

    public void RaceHobbitClicked()
    {
        PlayerRace = Player.raceType.halfling;
        ChangeBaseSprite();
    }

    public void RaceHumanClicked()
    {
        PlayerRace = Player.raceType.human;
        ChangeBaseSprite();
    }

    public void RaceOrcClicked()
    {
        PlayerRace = Player.raceType.orc;
        ChangeBaseSprite();
    }

    public void GanderMaleClicked()
    {
        PlayerGender = Player.genderType.male;
        ChangeBaseSprite();
    }

    public void GanderFemaleClicked()
    {
        PlayerGender = Player.genderType.female;
        ChangeBaseSprite();
    }

    public void WorldNameChanged()
    {
        WorldName = WorldNameInput.text;
    }

    public void PlayerNameChanged()
    {
        PlayerName = PlayerNameInput.text;
    }

    public void ClassArcherClicked()
    {
        PlayerClass = Player.classType.archer;
    }

    public void ClassAssassinClicked()
    {
        PlayerClass = Player.classType.assassin;
    }

    public void ClassBarbarianClicked()
    {
        PlayerClass = Player.classType.barbarian;
    }

    public void ClassCrusaderClicked()
    {
        PlayerClass = Player.classType.crusader;
    }

    public void ClassHealerClicked()
    {
        PlayerClass = Player.classType.healer;
    }

    public void ClassHunterClicked()
    {
        PlayerClass = Player.classType.hunter;
    }

    public void ClassMageClicked()
    {
        PlayerClass = Player.classType.mage;
    }

    public void ClassPaladinClicked()
    {
        PlayerClass = Player.classType.paladin;
    }

    public void ClassRangerClicked()
    {
        PlayerClass = Player.classType.ranger;
    }

    public void ClassThiefClicked()
    {
        PlayerClass = Player.classType.thief;
    }

    public void ClassWarriorClicked()
    {
        PlayerClass = Player.classType.warrior;
    }

    public void ClassWizardClicked()
    {
        PlayerClass = Player.classType.wizard;
    }

    public void SeedChanged()
    {
        WorldSeed = int.Parse(WorldSeedInput.text);
    }

    public void DoneClicked()
    {
        if (PlayerNameInput.text != (""))
        {
            GameObject mainMenuObject = GameObject.Find("ActionsObject");

            MainMenuActions values = mainMenuObject.GetComponent<MainMenuActions>();

            values.worldName = WorldName;
            values.worldSeed = WorldSeed;
            values.PlayerName = PlayerName;
            values.PlayerRace = PlayerRace;
            values.PlayerGender = PlayerGender;
            values.PlayerClass = PlayerClass;

            SceneManager.LoadScene("world_main");
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("world_main"));
        }
    }

    public void ChangeBaseSprite()
    {
        if (PlayerGender == Player.genderType.male)
        {
            switch (PlayerRace)
            {
                case (Player.raceType.dwarf):
                    PlayerBase.sprite = PlayerSprites[1];
                    break;
                case (Player.raceType.elf):
                    PlayerBase.sprite = PlayerSprites[3];
                    break;
                case (Player.raceType.halfling):
                    PlayerBase.sprite = PlayerSprites[5];
                    break;
                case (Player.raceType.human):
                    PlayerBase.sprite = PlayerSprites[7];
                    break;
                case (Player.raceType.orc):
                    PlayerBase.sprite = PlayerSprites[9];
                    break;
            }
        }
        else
        {
            switch (PlayerRace)
            {
                case (Player.raceType.dwarf):
                    PlayerBase.sprite = PlayerSprites[0];
                    break;
                case (Player.raceType.elf):
                    PlayerBase.sprite = PlayerSprites[2];
                    break;
                case (Player.raceType.halfling):
                    PlayerBase.sprite = PlayerSprites[4];
                    break;
                case (Player.raceType.human):
                    PlayerBase.sprite = PlayerSprites[6];
                    break;
                case (Player.raceType.orc):
                    PlayerBase.sprite = PlayerSprites[8];
                    break;
            }
        }
    }

    public void ChangeWorldUIVisible(bool setting)
    {
        if (setting == false)
        {
            WorldUIScale = 0;
        }
        else
        {
            WorldUIScale = 0.1f;
        }
        
        WorldNameID.enabled = setting;
        WorldNameID.transform.localScale = new Vector3(WorldUIScale, WorldUIScale, WorldUIScale);

        WorldNameInput.enabled = setting;
        WorldNameInput.transform.localScale = new Vector3(WorldUIScale, WorldUIScale, WorldUIScale);

        WorldSeedID.enabled = setting;
        WorldSeedID.transform.localScale = new Vector3(WorldUIScale, WorldUIScale, WorldUIScale);

        WorldSeedInput.enabled = setting;
        WorldSeedInput.transform.localScale = new Vector3(WorldUIScale, WorldUIScale, WorldUIScale);
    }

    public void ChangePlayerUIVisible(bool setting)
    {
        if (setting == false)
        {
            PlayerUIScale = 0;
            PlayerIamgeScale = 0;
        }
        else
        {
            PlayerUIScale = 0.1f;
            PlayerIamgeScale = 3;
        }
        
        PlayerBase.enabled = setting;
        PlayerBase.transform.localScale = new Vector3(PlayerIamgeScale, PlayerIamgeScale, PlayerIamgeScale);
        
        PlayerShadow.enabled = setting;
        PlayerShadow.transform.localScale = new Vector3(PlayerIamgeScale, PlayerIamgeScale, PlayerIamgeScale);

        PlayerNameID.enabled = setting;
        PlayerNameID.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);

        PlayerNameInput.enabled = setting;
        PlayerNameInput.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);

        PlayerRaceID.enabled = setting;
        PlayerRaceID.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);

        PlayerRaceDwarf.enabled = setting;
        PlayerRaceDwarf.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);

        PlayerRaceElf.enabled = setting;
        PlayerRaceElf.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);

        PlayerRaceHobbit.enabled = setting;
        PlayerRaceHobbit.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);

        PlayerRaceHuman.enabled = setting;
        PlayerRaceHuman.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);

        PlayerRaceOrc.enabled = setting;
        PlayerRaceOrc.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);

        PlayerGenderID.enabled = setting;
        PlayerGenderID.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);

        PlayerGenderMale.enabled = setting;
        PlayerGenderMale.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);

        PlayerGenderFemale.enabled = setting;
        PlayerGenderFemale.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);

        PlayerClassID.enabled = setting;
        PlayerClassID.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);

        PlayerClassArcher.enabled = setting;
        PlayerClassArcher.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);
        
        PlayerClassAssassin.enabled = setting;
        PlayerClassAssassin.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);
        
        PlayerClassBarbarian.enabled = setting;
        PlayerClassBarbarian.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);
        
        PlayerClassCrusader.enabled = setting;
        PlayerClassCrusader.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);


        PlayerClassHealer.enabled = setting;
        PlayerClassHealer.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);

        PlayerClassHunter.enabled = setting;
        PlayerClassHunter.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);

        PlayerClassMage.enabled = setting;
        PlayerClassMage.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);

        PlayerClassPaladin.enabled = setting;
        PlayerClassPaladin.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);

        PlayerClassRanger.enabled = setting;
        PlayerClassRanger.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);

        PlayerClassThief.enabled = setting;
        PlayerClassThief.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);

        PlayerClassWarrior.enabled = setting;
        PlayerClassWarrior.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);

        PlayerClassWizard.enabled = setting;
        PlayerClassWizard.transform.localScale = new Vector3(PlayerUIScale, PlayerUIScale, PlayerUIScale);
    }
}
