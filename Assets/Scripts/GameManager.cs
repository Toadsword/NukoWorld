using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    const int NUM_LEVELS = 3;

    [Header("Prefabs")]
    [SerializeField] GameObject playerPrefab;

    [Header("Public Vars")]
    public SceneManagement sceneManagInstance;
    public SoundManager soundManagInstance;

    public bool isGameRunning = false;
    public bool isGameWon = false;

    public int currentLevel = -1;

    public MapGenerator levelObj;

    public GameObject playerObj;
    
    // Use this for initialization
    void Start ()
    {
        DontDestroyOnLoad(gameObject);
        sceneManagInstance = FindObjectOfType<SceneManagement>();
        soundManagInstance = FindObjectOfType<SoundManager>();
        currentLevel = -1;
    }
	
    public void SetupScene(SceneManagement.Scenes scene)
    {
        switch (scene)
        {
            case SceneManagement.Scenes.GAME:
                if(currentLevel != 2)
                    soundManagInstance.PlayMusic(SoundManager.MusicList.GAME_MUSIC_1);
                else
                    soundManagInstance.PlayMusic(SoundManager.MusicList.GAME_MUSIC_2);

                MapGenerator mpGener = FindObjectOfType<MapGenerator>();
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null && mpGener != null)
                {
                    player.transform.position = (Vector2)mpGener.entrancePlace;
                }
                isGameRunning = true;

                break;

            case SceneManagement.Scenes.MAP_GENERATION:
                if (playerObj == null)
                {
                    playerObj = Instantiate(playerPrefab, playerPrefab.transform.position, playerPrefab.transform.rotation);
                    DontDestroyOnLoad(playerObj);
                }

                isGameRunning = false;

                if (levelObj != null)
                    Destroy(levelObj);

                currentLevel++;
                Debug.Log("currentLevel : " + currentLevel);
                MapGenerator temp = levelObj;
                foreach (MapGenerator mapGener in FindObjectsOfType<MapGenerator>())
                {
                    if (levelObj == mapGener)
                        Destroy(levelObj.gameObject);
                    else
                        temp = mapGener;
                }
                levelObj = temp;
                levelObj.SetLevel(currentLevel);
                break;

            case SceneManagement.Scenes.MENU:
                if (playerObj != null)
                {
                    Destroy(playerObj);
                    playerObj = null;
                }

                isGameRunning = false;
                isGameWon = false;
                currentLevel = -1;
                soundManagInstance.PlayMusic(SoundManager.MusicList.MENU_MUSIC);
                break;

            case SceneManagement.Scenes.END_GAME:
                if (playerObj != null)
                {
                    Destroy(playerObj);
                    playerObj = null;
                }

                isGameRunning = false;
                currentLevel = -1;
                if(!isGameWon)
                    soundManagInstance.PlayMusic(SoundManager.MusicList.NONE);

                if (isGameWon)
                {
                    GameObject.Find("CanvasEndGame").transform.Find("VictoryPanel").gameObject.SetActive(true);
                    GameObject.Find("CanvasEndGame").transform.Find("DefeatPanel").gameObject.SetActive(false);
                    // soundManagInstance.PlaySound(SoundManager.SoundList.WIN_MUSIC);
                }
                else
                {
                    GameObject.Find("CanvasEndGame").transform.Find("VictoryPanel").gameObject.SetActive(false);
                    GameObject.Find("CanvasEndGame").transform.Find("DefeatPanel").gameObject.SetActive(true);
                    // soundManagInstance.PlaySound(SoundManager.SoundList.LOSE_MUSIC);
                }
                isGameWon = false;
                break;
            // End case
        } // End switch
    }

    public void EndLevel()
    {
        if(currentLevel != NUM_LEVELS - 1)
        {
            sceneManagInstance.ChangeScene(SceneManagement.Scenes.MAP_GENERATION);
        }
        else
        {
            EndGame(true);
        }
    }

    public void EndGame(bool hasWon)
    {
        isGameRunning = false;
        isGameWon = hasWon;
        sceneManagInstance.ChangeScene(SceneManagement.Scenes.END_GAME);
    }
}

