using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActions : MonoBehaviour {

    SceneManagement smInstance;
    GameObject creditsPanel;
    bool isCreditPanelActive = false;

    // Use this for initialization
    void Start() {
        smInstance = FindObjectOfType<SceneManagement>();
        creditsPanel = GameObject.Find("Canvas").transform.Find("CreditPanel").gameObject;
    }

    // Update is called once per frame
    void Update() {

    }

    public void StartBtn()
    {
        smInstance.ChangeScene(SceneManagement.Scenes.MAP_GENERATION);
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
