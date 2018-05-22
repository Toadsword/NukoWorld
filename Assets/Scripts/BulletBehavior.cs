using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehavior : MonoBehaviour {

    public int damage = 1;
    public bool isFriendly = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.tag == "Wall")
        {
            Destroy(gameObject);
        }
    }
}
