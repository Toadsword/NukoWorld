using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehavior : MonoBehaviour {

    public float damage = 2.0f;
    public bool fromPlayer = true;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.transform.tag == "enemy" && fromPlayer)
        {
            //Deal damage to enemy
        }
        else if(collision.transform.tag == "player")
        {
            //Deal damage to player, usually 0.5 but we never know :3
        }
        Destroy(gameObject);
    }
}
