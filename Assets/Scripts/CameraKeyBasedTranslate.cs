using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;

public class CameraKeyBasedTranslate : MonoBehaviour {
    
    private bool ButtonLeftPressed = false;
    
    private bool ButtonRightPressed = false;
    
    private bool ButtonUpPressed = false;
    
    private bool ButtonDownPressed = false;
    
    private bool ButtonIncreasePressed = false;
    
    private bool ButtonDecreasedPressed = false;

    // Use this for initialization
    void Start ()
    {

	}

    // Update is called once per frame
    void Update ()
    {
        movement();

        viewScale();
    }

    public void movement()
    {
        if (Input.GetAxis("Horizontal") > 0.0f)
        {
            if (ButtonRightPressed == false)
            {
                transform.position += new Vector3(1.28f, 0, 0);
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
                transform.position += new Vector3(-1.28f, 0, 0);
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
                transform.position += new Vector3(0, 1.28f, 0);
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
                transform.position += new Vector3(0, -1.28f, 0);
            }
            ButtonDownPressed = true;
        }
        else
        {
            ButtonDownPressed = false;
        }
    }

    public void viewScale()
    {
        if (Input.GetAxis("Zoom") > 0.0f)
        {
            if (ButtonIncreasePressed == false)
            {
                Camera.current.orthographicSize += 4;
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
                if (Camera.current.orthographicSize - 4 > 0)
                {
                    Camera.current.orthographicSize -= 4;
                }
            }
            ButtonDecreasedPressed = true;
        }
        else
        {
            ButtonDecreasedPressed = false;
        }
    }

    public void setCameraPosition(Vector2 newLoc)
    {
        transform.position = new Vector3(newLoc.x, newLoc.y, transform.position.z);
    }
}
