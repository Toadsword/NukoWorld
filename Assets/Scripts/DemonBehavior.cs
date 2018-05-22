using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonBehavior : MonoBehaviour {
    
    enum DemonStates
    {
        IDLE,
        RETURNING_BASE,
        CHASE_PLAYER
    }

    [Header("Prefabs")]
    [SerializeField] GameObject bulletPrefab;

    public static double Heuristic(Vector2 a, Vector2 b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    string demonName = "Demon";
    float moveSpeed = 5.0f;
    float acceleration = 3.0f;
    float bulletSpeed = 9.0f;
    float bulletScale = 0.3f;
    float fireRate = 0.4f;
    int healthPoint = 4;
    float demonNoiseTime = 10.0f;
    float demonNoiseTimer;

    float fireRateCd;

    bool isDead = false;

    bool viewPlayer = false;
    bool playerInSightRange = false;
    DemonStates state = DemonStates.IDLE;

    GameManager gmInstance;
    SoundManager smInstance;

    Rigidbody2D rigid;

    GameObject player;
    Vector2 lastAngleToPlayer;
    Vector2 lastSeenPlayerPos;
    Vector2 spawnPosition;

    BulletCreator.BulletData bulletData;

    LayerMask demonLayerMask;

    // Use this for initialization
    void Start () {
        isDead = false;
        gmInstance = FindObjectOfType<GameManager>();
        smInstance = FindObjectOfType<SoundManager>();

        spawnPosition = transform.position;
        player = FindObjectOfType<PlayerController>().gameObject;

        bulletData.damage = 1;
        bulletData.scale = bulletScale;
        bulletData.speed = bulletSpeed;
        bulletData.isFriendly = false;

        demonNoiseTimer = 0.0f;

        demonLayerMask = 1 << LayerMask.NameToLayer("Demon");
        demonLayerMask = ~demonLayerMask;

        rigid = GetComponent<Rigidbody2D>();
    }
	
	// Update is called once per frame
	void Update () {
        if(gmInstance.isGameRunning && !isDead)
        {
            if (playerInSightRange)
            {
                if (Utility.IsOver(demonNoiseTimer))
                {
                    demonNoiseTimer = Utility.StartTimer(demonNoiseTime);
                    smInstance.PlaySound(SoundManager.SoundList.DEMON_NOISE);
                }

                lastSeenPlayerPos = player.transform.position;
                lastAngleToPlayer = new Vector2(transform.position.x - lastSeenPlayerPos.x, transform.position.y - lastSeenPlayerPos.y);
                RaycastHit2D hit = Physics2D.Linecast(transform.position, lastSeenPlayerPos, demonLayerMask);
                if (hit.collider != null && hit.collider.tag == "Player")
                {
                    viewPlayer = true;
                }
                else
                {
                    viewPlayer = false;
                }
            }
            
            switch (state)
            {
                case DemonStates.IDLE:
                    if(playerInSightRange && viewPlayer)
                    {
                        ChangeState(DemonStates.CHASE_PLAYER);
                    }
                    else
                    {
                        DemonIdle();
                    }
                    break;
                case DemonStates.CHASE_PLAYER:
                    // If not obstrued by a wall
                    if (playerInSightRange && viewPlayer)
                    {
                        ShootPlayer();
                    }
                    else
                    {
                        bool isMoving = DemonMoveTo(lastSeenPlayerPos);
                        if (!isMoving)
                            ChangeState(DemonStates.RETURNING_BASE);
                    }
                    break;
                case DemonStates.RETURNING_BASE:
                    if (playerInSightRange && viewPlayer)
                    {
                        ChangeState(DemonStates.CHASE_PLAYER);
                    }
                    else
                    {
                        bool isMoving = DemonMoveTo(spawnPosition);
                        if (!isMoving)
                            ChangeState(DemonStates.IDLE);
                    }
                    DemonMoveTo(spawnPosition);
                    break;
            }
        }
	}

    private void DemonIdle()
    {
        //Cause the demon to move around
    }

    //Return if the demon is currently moving or not
    private bool DemonMoveTo(Vector2 pos)
    {
        Vector3 posToSeek = new Vector3(pos.x, pos.y, 0.0f);
        Vector3 direction = posToSeek - transform.position;
        
        direction.Normalize();
        rigid.AddForce(acceleration * direction);
        if (rigid.velocity.magnitude > moveSpeed)
        {
            rigid.velocity = rigid.velocity.normalized * moveSpeed;
        }

        if (rigid.velocity.x <= 0.1f && rigid.velocity.y <= 0.1f)
            return false;
        return true;
    }

    private void ShootPlayer()
    {
        if (Utility.IsOver(fireRateCd))
        {
            // float angle = Mathf.Atan2(lastAngleToPlayer.y, lastAngle.x) * Mathf.Rad2Deg + 90.0f;

            bulletData.position = transform.position;
            bulletData.direction = -lastAngleToPlayer.normalized;
            BulletCreator.CreateBullet(bulletPrefab, bulletData);

            fireRateCd = Utility.StartTimer(fireRate);
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
    }

    public void GetHit(int damage)
    {
        healthPoint -= damage;
        // TODO : Add animation of being hit
        // TODO : Add sound of being hit
        if(healthPoint <= 0.0f && !isDead)
        {
            isDead = true;
            GetComponent<BoxCollider2D>().isTrigger = true;
            GetComponent<SpriteRenderer>().color = Color.magenta;
            smInstance.PlaySound(SoundManager.SoundList.DEMON_DEATH);
        }
    }

    //Function used to change animations of demons and applying correct sounds
    private void ChangeState(DemonStates newState)
    {
        state = newState;
        switch(newState)
        {
            case DemonStates.IDLE:
                //Debug.Log("IDLE");
                break;
            case DemonStates.CHASE_PLAYER:
                smInstance.PlaySound(SoundManager.SoundList.DEMON_NOISE);
                //Debug.Log("CHASE_PLAYER");
                break;

            case DemonStates.RETURNING_BASE:
                ///Debug.Log("RETURNING_BASE");
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            playerInSightRange = true;
            Debug.Log("player in sight");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            playerInSightRange = false;
            Debug.Log("player not in sight");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Bullet" && !isDead)
        {
            BulletBehavior hitted = collision.gameObject.GetComponent<BulletBehavior>();
            if (hitted.isFriendly)
            {
                GetHit(hitted.damage);
                Destroy(collision.gameObject);
            }
        }
    }
}
