using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public SceneManagement sceneManagInstance;
    private SoundManager soundManagInstance;

    public bool isGameRunning = false;
    public bool allowPlayerMovements = true;
    private bool isGameWon = false;

    private int currentLevel = -1;

    [Header("Levels")]
    [SerializeField] LevelParams[] levels;
    
    // Use this for initialization
    void Start ()
    {
        DontDestroyOnLoad(gameObject);
        sceneManagInstance = FindObjectOfType<SceneManagement>();
        //soundManagInstance = FindObjectOfType<SoundManager>();
        currentLevel = -1;
    }
	
    public void SetupScene(SceneManagement.Scenes scene)
    {
        switch (scene)
        {
            case SceneManagement.Scenes.GAME:
                SetPlayerSpawn();

                //soundManagInstance.PlayMusic(SoundManager.MusicList.GAME_MUSIC);

                allowPlayerMovements = true;
                isGameRunning = true;

                break;

            case SceneManagement.Scenes.MAP_GENERATION:
                currentLevel++;
                FindObjectOfType<MapGenerator>().SetLevel(currentLevel);
                allowPlayerMovements = true;
                break;

            case SceneManagement.Scenes.MENU:
                //soundManagInstance.PlayMusic(SoundManager.MusicList.MENU_MUSIC);
                allowPlayerMovements = false;
                isGameRunning = false;
                break;

            case SceneManagement.Scenes.END_GAME:
                //soundManagInstance.PlayMusic(SoundManager.MusicList.NONE);
                allowPlayerMovements = false;
                isGameRunning = false;                
                
                if (isGameWon)
                {
                    GameObject.Find("CanvasEndGame").transform.Find("VictoryPanel").gameObject.SetActive(true);
                    // soundManagInstance.PlaySound(SoundManager.SoundList.WIN_MUSIC);
                }
                else
                {
                    GameObject.Find("CanvasEndGame").transform.Find("DefeatPanel").gameObject.SetActive(true);
                    // soundManagInstance.PlaySound(SoundManager.SoundList.LOSE_MUSIC);
                }
                break;
            // End case
        } // End switch
    }

    public void SetPlayerSpawn()
    {

    }
}

