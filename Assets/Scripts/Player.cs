using System;
using UnityEngine;

[Serializable]
public class Player
{
    public enum raceType
    {
        dwarf, elf, halfling, human, orc,
    }

    public enum classType
    {
        archer, assassin, barbarian, crusader, healer, hunter, mage, paladin, ranger, thief, warrior, wizard,
    }

    public enum genderType
    {
        male, female,
    }

    public string playerName { get; set; }

    public raceType playerRace { get; set; }
    
    public classType playerClass { get; set; }

    public genderType playerGender { get; set; }

    public int playerHealth { get; set; }

    public int playerMaxHealth { get; set; }

    public int playerDamage { get; set; }

    public int playerGold { get; set; }

    public int playerLevel { get; set; }

    public int[] playerExperience { get; set; }

    public int[] playerStats { get; set; }

    public int[] playerEquipementStats { get; set; }

    public string[] playerArmorWeapon { get; set; }

    public string[,] playerInventory { get; set; }
}
