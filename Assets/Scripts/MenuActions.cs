using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActions : MonoBehaviour {

    SceneManagement sceneManagInstance;
    SoundManager soundManagInstance;
    GameObject creditsPanel;
    bool isCreditPanelActive = false;

    // Use this for initialization
    void Start() {
        sceneManagInstance = FindObjectOfType<SceneManagement>();
        soundManagInstance = FindObjectOfType<SoundManager>();
        creditsPanel = GameObject.Find("Canvas").transform.Find("CreditPanel").gameObject;
    }

    // Update is called once per frame
    void Update() {

    }

    public void StartBtn()
    {
        sceneManagInstance.ChangeScene(SceneManagement.Scenes.MAP_GENERATION);
        soundManagInstance.PlaySound(SoundManager.SoundList.NEKO_NOISE);
    }

    public void MenuBtn()
    {
        sceneManagInstance.ChangeScene(SceneManagement.Scenes.MENU);
    }

    public void CreditBtn()
    {
        isCreditPanelActive = !isCreditPanelActive;
        creditsPanel.SetActive(isCreditPanelActive);
    }

    public void QuitBtn()
    {
        Application.Quit();
    }
}
