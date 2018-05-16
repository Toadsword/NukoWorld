using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehavior : MonoBehaviour {

    public int damage = 1;
    public bool isFriendly = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Demon" && isFriendly)
        {
            //Deal damage to enemy
            collision.gameObject.GetComponent<DemonController>().GetHit(damage);
            Destroy(gameObject);
        }
        else if (collision.transform.tag == "player" && !isFriendly)
        {
            collision.gameObject.GetComponent<PlayerController>().GetHit(damage);
            Destroy(gameObject);
        }

    }
}
