using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameManuHandler : MonoBehaviour
{
    public WorldGen worldGenHandler;
    public DungoenGen dungeonHandler;
    public Canvas inGameUI;
    public Canvas inGameMenu;

    public bool isMainWorld;

	void Start ()
    {
		if (SceneManager.GetActiveScene().ToString() == "world_main")
        {
            isMainWorld = true;
        }
        else
        {
            isMainWorld = false;
        }
	}
	
	void Update ()
    {
		
	}
}
