using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    [SerializeField] float mouseToCameraPositionRatio = 0.2f;

    GameObject player;

    private void Start()
    {
        if (player == null)
            player = FindObjectOfType<PlayerController>().gameObject;
    }

    // Update is called once per frame
    void Update () {
        Vector3 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.transform.position;
        Vector3 newCameraPos = player.transform.position + cursorPos * mouseToCameraPositionRatio;
        this.transform.position = new Vector3(newCameraPos.x, newCameraPos.y, -10.0f);
    }
}
