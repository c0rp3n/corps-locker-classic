using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour {

    //Allows me to access and set the values of these UI components in the code.
    public PlayerController playerHandler;
    public Slider playerHealthBar;

	// Use this for initialization
	void Start ()
    {
        //Set initail values for the health bar.
        GetComponent<Text>().text = "100";
        playerHealthBar.value = 100;
        playerHealthBar.maxValue = 100;
	}
	
	// Update is called once per frame
	void Update ()
    {
        //Sets the values of the health bar to the current values in the player class in my player controller.
        GetComponent<Text>().text = playerHandler.playerAttributes.playerHealth.ToString();

        playerHealthBar.maxValue = playerHandler.playerAttributes.playerMaxHealth;
        playerHealthBar.value = playerHandler.playerAttributes.playerHealth;
    }
}