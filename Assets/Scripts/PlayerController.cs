﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    public const float MIN_CAP_FIRE_RATE = 0.1f;

    [Header("Overall stats")]
    [SerializeField] public float moveSpeed = 5.0f;
    [SerializeField] public float bulletSpeed = 9.0f;
    [SerializeField] public int bulletDamage = 3;
    [SerializeField] public float bulletScale = 0.3f;
    [SerializeField] public float fireRate = 0.4f;
    [SerializeField] public float spamClickReductionFireRate = 0.08f;
    [SerializeField] public int healthPoint = 3;
    public int currentHealth;
    [SerializeField] public float slowTime = 3.0f;
    [SerializeField] public float slowTimeRecoveryScale = 0.3f;
    [SerializeField] public float moveSpeedBonucWhileSlown = 3.0f;

    [Header("Prefabs")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] GameObject particlePrefab;
    [SerializeField] GameObject cursor;

    [Header("Sounds")]
    [SerializeField] float timeBetweenFootsteps = 0.3f;
    float footStepsTimer;

    public List<ObjBehavior.EffectName> holdingItems;

    // Cd means Cooldown
    float fireRateCd;

    float slowTimeTimer;
    bool blockedSlowTimeTimer;

    bool isTimeSlown;

    bool onStairs = false;
    GameManager gmInstance;
    SoundManager smInstance;

    Vector2 lastAngle;
    Transform gun;
    Transform gunCannon;
    Image clock;
    Image clockBackground;
    BulletCreator.BulletData bulletData;
    Rigidbody2D rigid;

    AudioSource clockSound;

	// Use this for initialization
	void Start ()
    {
        gmInstance = FindObjectOfType<GameManager>();
        smInstance = FindObjectOfType<SoundManager>();

        rigid = GetComponent<Rigidbody2D>();
        gun = transform.Find("GunObject");
        gunCannon = gun.Find("Gun").Find("CannonOutputPoint");
        clock = transform.Find("PlayerUI").Find("Clock").GetComponent<Image>();
        clockBackground = transform.Find("PlayerUI").Find("ClockBackground").GetComponent<Image>();
        lastAngle = new Vector2(0.0f, 0.0f);

        bulletData.damage = bulletDamage;
        bulletData.scale = bulletScale;
        bulletData.speed = bulletSpeed;
        bulletData.isFriendly = true;

        currentHealth = healthPoint;
        holdingItems = new List<ObjBehavior.EffectName>();

        fireRateCd = 0.0f;
        footStepsTimer = 0.0f;

        slowTimeTimer = slowTime;
        blockedSlowTimeTimer = false;
        onStairs = false;
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        if(gmInstance.isGameRunning)
        {
            UpdateGunPosition();
            ManageInputs();
        
            if(Utility.IsOver(footStepsTimer) && (rigid.velocity.x != 0 || rigid.velocity.y != 0))
            {
                footStepsTimer = Utility.StartTimer(timeBetweenFootsteps);
                smInstance.PlaySound(SoundManager.SoundList.WALK);
            }
        }
    }

    private void UpdateGunPosition()
    {
        Vector3 v3 = Input.mousePosition;
        v3.z = 10.0f;
        v3 = Camera.main.ScreenToWorldPoint(v3);

        cursor.transform.position = v3;
        lastAngle = new Vector2(transform.position.x - v3.x, transform.position.y - v3.y);
        float angle = Mathf.Atan2(lastAngle.y, lastAngle.x) * Mathf.Rad2Deg + 90.0f;
        gun.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void ManageInputs()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            fireRateCd -= spamClickReductionFireRate;
        }

        if (Input.GetButton("Fire1"))
        {
            var v3 = Input.mousePosition;
            v3.z = 10.0f;
            v3 = Camera.main.ScreenToWorldPoint(v3);

            if (Utility.IsOver(fireRateCd))
            {
                bulletData.position = gunCannon.position;
                bulletData.direction = -lastAngle.normalized;
                BulletCreator.CreateBullet(bulletPrefab, bulletData);

                fireRateCd = Utility.StartTimer(fireRate);
                smInstance.PlaySound(SoundManager.SoundList.GUN_FIRE);
            }
        }

        if(Input.GetButtonDown("Action") && onStairs)
        {
            gmInstance.EndLevel();
            smInstance.PlaySound(SoundManager.SoundList.STAIRS);
        }

        if (Input.GetButton("slowDownTime") && !blockedSlowTimeTimer)
        {
            if (clockSound == null)
                clockSound = smInstance.PlaySound(SoundManager.SoundList.CLOCK, slowTimeTimer);

            slowTimeTimer -= Time.deltaTime / Time.timeScale;

            if (slowTimeTimer > 0.0f)
            {
                Time.timeScale = 0.5f;
                if(!isTimeSlown)
                {
                    moveSpeed += moveSpeedBonucWhileSlown;
                    isTimeSlown = true;
                }
            }
            else
            {
                Time.timeScale = 1.0f;
                blockedSlowTimeTimer = true;
                if (isTimeSlown)
                {
                    moveSpeed -= moveSpeedBonucWhileSlown;
                    isTimeSlown = false;
                }
            }
        }
        else
        {
            if (clockSound != null)
            {
                smInstance.StopSound(clockSound);
                clockSound = null;
            }

            slowTimeTimer += Time.deltaTime * slowTimeRecoveryScale;
            Time.timeScale = 1.0f;

            if (slowTimeTimer > slowTime)
            {
                slowTimeTimer = slowTime;
                blockedSlowTimeTimer = false;
            }

            if (isTimeSlown)
            {
                moveSpeed -= moveSpeedBonucWhileSlown;
                isTimeSlown = false;
            }
        }
        clock.fillAmount = slowTimeTimer / slowTime;

        if(blockedSlowTimeTimer)
            clockBackground.color = Color.red;
        else
            clockBackground.color = Color.white;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float verticale = Input.GetAxisRaw("Vertical");
        rigid.velocity = new Vector2(horizontal, verticale) * moveSpeed;
    }

    public void GetHit(int damage)
    {
        healthPoint -= damage;
        if(gmInstance.isGameRunning)
        {
            smInstance.PlaySound(SoundManager.SoundList.DEATH);
            if (healthPoint <= 0.0f)
            {
                Debug.Log("PLAYER IS DEAD");
                gmInstance.EndGame(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Stairs")
        {
            onStairs = true;
        }
        else if(collision.transform.tag == "Bullet")
        {
            BulletBehavior hitted = collision.gameObject.GetComponent<BulletBehavior>();
            Debug.Log("Bullet Hit" + hitted.isFriendly + " _ " + hitted.damage);
            if (!hitted.isFriendly)
            {
                GetHit(hitted.damage);
                Destroy(collision.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Stairs")
        {
            onStairs = false;
        }
    }
}
