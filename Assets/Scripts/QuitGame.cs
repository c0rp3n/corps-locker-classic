using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class QuitGame : MonoBehaviour
{
	// Use this for initialization
	void Start ()
    {
        
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(Input.GetKey("escape"))
            Quit();
    }

    public void Quit()
    {
        Application.Quit();
    }
}

