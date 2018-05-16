using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonController : MonoBehaviour {
    
    string demonName = "Demon";
    float moveSpeed = 5.0f;
    float bulletSpeed = 9.0f;
    float bulletScale = 1.0f;
    float fireRate = 0.4f;
    int healthPoint = 4;

    bool isSet = false;

    // Use this for initialization
    void Start () {
        isSet = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(isSet)
        {

        }
	}

    public void SetDemonStats(DemonStats demonStats)
    {
        demonName = demonStats.demonName;
        moveSpeed = demonStats.moveSpeed;
        bulletSpeed = demonStats.bulletSpeed;
        bulletScale = demonStats.bulletScale;
        fireRate = demonStats.fireRate;
        healthPoint = demonStats.healthPoint;

        isSet = true;
    }

    public void GetHit(int damage)
    {
        healthPoint -= damage;
        // TODO : Add animation of being hit
        // TODO : Add sound of being hit
        if(healthPoint <= 0.0f)
        {
            Destroy(gameObject);
        }
    }
}
