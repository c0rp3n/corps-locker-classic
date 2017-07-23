using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadGameMenu : MonoBehaviour
{
    public string[] fileList;

    public string worldName; //Stores the current world name

    public Dropdown saveFileBox;

	// Use this for initialization
	void Start ()
    {
        fileList = refreshWorldSaves();
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public string[] refreshWorldSaves()
    {
        saveFileBox.ClearOptions();

        List<string> levelList = new List<string>();

        levelList.Add("None");

        string[] filesLocs = Directory.GetFiles(Application.persistentDataPath + @"\saves");

        foreach (string file in filesLocs)
        {
            if (file.EndsWith(".corSAV"))
            {
                levelList.Add(file);
            }
        }

        FileInfo[] files = new FileInfo[levelList.Count - 1];
        string[] fileNameList = new string[levelList.Count - 1];
        for (int i = 1; i < levelList.Count; i++)
        {
            files[i - 1] = new FileInfo(levelList[i]);

            fileNameList[i - 1] = files[i - 1].Name.Replace(files[i - 1].Extension, "");

            levelList[i] = fileNameList[i - 1];
        }

        saveFileBox.AddOptions(levelList);

        return fileNameList;
    }

    public void doneClicked()
    {
        GameObject mainMenuObject = GameObject.Find("ActionsObject");
        MainMenuActions values = mainMenuObject.GetComponent<MainMenuActions>();

        values.worldName = fileList[saveFileBox.value - 1];

        SceneManager.LoadScene("world_main");
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("world_main"));
    }

    public void backClicked()
    {
        Destroy(GameObject.Find("ActionsObject"));
        SceneManager.LoadScene("main_menu");
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("main_menu"));
    }
}
