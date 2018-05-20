using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingScene : MonoBehaviour {

	// Use this for initialization
	void Start () {
        SceneManagement smInstance = FindObjectOfType<SceneManagement>();
        smInstance.ChangeScene(SceneManagement.Scenes.MENU);
	}
}
