using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Enemy
{
    //This class is a general class for enemies it contains all relevant data for a single enemy this can be instanced at any time.

    //An enum to declare the type of enemy.
    public enum EnemyType
    {
        grey_rat, warg, wolf,
    }

    //Variable using the before created enum so that a type may be sey.
    public EnemyType enemyTypeVal { get; set; }

    //Stores and enemy name for unique enemies.
    public string enemyName { get; set; }

    //Stores the health of the enemy as a integer.
    public int enemyHeatlth { get; set; }
    
    //The current counter of enemy movements.
    public int enemyMoves { get; set; }

    //Stores the max amount of movements on the current vector.
    public int enemyMovesMax { get; set; }

    //The current location of the enemy.
    public Vector3 enemyLocation { get; set; }

    //The location the enemy is moving too currently.
    public Vector3 nextEnemyLocation { get; set; }

    //The location an enemies pathfinding it trying to get too.
    public Vector3 targetEnemyLocation { get; set; }

    //States whether an enemy can get any closer to their target loc.
    public bool enemyIsAtMaxLoc { get; set; }

    //Boolean to store whether an enemy has moved or attacked in the current movement cycle
    public bool enemyHasHadTurn { get; set; }

    //Stores the damage of an enemy
    public int enemyDamage { get; set; }

    //Stores the experience the player will get once they kill this enemy
    public int enemyExperienceGranted { get; set; }
}

public class raceBaseData
{
    //This class contains the data for all enemy types, this data can be accesessed at any time
    //For each enemy there is a specific entry for each stat:
    //                                                      health,
    //                                                      damage,
    //                                                      experience granted
    //They are all interger ranges so the values cna be randomsised.

    //Grey Rat
    public IntRange grey_rat_health = new IntRange(5, 15);
    public IntRange grey_rat_base_damage = new IntRange(2, 7);
    public IntRange grey_rat_experience_granted = new IntRange(22, 36);

    //Warg
    public IntRange warg_health = new IntRange(40, 60);
    public IntRange warg_base_damage = new IntRange(12, 24);
    public IntRange warg_experience_granted = new IntRange(55, 75);

    //Wolf
    public IntRange wolf_health = new IntRange(30, 50);
    public IntRange wolf_base_damage = new IntRange(10, 18);
    public IntRange wolf_experience_gained = new IntRange(45, 65);
} 
