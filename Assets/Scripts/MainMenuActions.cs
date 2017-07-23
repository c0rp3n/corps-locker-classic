using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuActions : MonoBehaviour
{
    public string worldName { get; set; }

    public int worldSeed { get; set; }

    public bool IsNewWorld { get; set; }

    public string PlayerName { get; set; }

    public Player.raceType PlayerRace { get; set; }

    public Player.genderType PlayerGender { get; set; }

    public Player.classType PlayerClass { get; set; }

    public Vector3 PlayerWorldLoc { get; set; }

    public string DungeonType { get; set; }

    public Vector3 PlayerDungeonLoc { get; set; }

    public void NewGame()
    {
        DontDestroyOnLoad(transform.gameObject);
        IsNewWorld = true;
        SceneManager.LoadScene("new_world_creator");
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("new_world_creator"));
    }

    public void LoadGame()
    {
        DontDestroyOnLoad(transform.gameObject);
        IsNewWorld = false;
        SceneManager.LoadScene("load_world");
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("load_world"));
    }
}
