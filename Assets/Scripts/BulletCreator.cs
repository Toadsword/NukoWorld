using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCreator : MonoBehaviour {

    public struct BulletData
    {
        public Vector3 position;
        public Vector2 direction;
        public float speed;
        public float scale;
        public int damage;
        public bool isFriendly;
    }

    public static void CreateBullet(GameObject bulletPrefab, BulletData bulletData)
    {
        Debug.Log("coucou");
        GameObject bulletObject = Instantiate(bulletPrefab, bulletData.position, bulletPrefab.transform.rotation);
        bulletObject.GetComponent<Rigidbody2D>().velocity = bulletData.direction * bulletData.speed;
        bulletObject.transform.localScale = new Vector3(bulletData.scale, bulletData.scale, 1.0f);
        bulletObject.GetComponent<BulletBehavior>().damage = bulletData.damage;
        bulletObject.GetComponent<BulletBehavior>().isFriendly = bulletData.isFriendly;
        Debug.Log(bulletObject.ToString());
    }
}
