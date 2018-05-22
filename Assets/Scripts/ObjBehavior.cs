using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjBehavior : MonoBehaviour {

    public enum EffectName
    {
        NONE,
        TRIPLE_SHOT,
        EXPLOSIVE_MISSILES,
        RUBBER_SHOTS
    }

    [SerializeField] public ObjStats objStats;

    Vector2 basePosition;
    float maxHeightAnimation = 0.2f;

    private void Start()
    {
        basePosition = transform.position;
    }
    private void Update()
    {
        float deltaH = Mathf.Sin(Time.time) * maxHeightAnimation;
        transform.position = basePosition + new Vector2(deltaH, deltaH);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.transform.tag == "Player")
        {
            AddItemToPlayer(collision.gameObject.GetComponent<PlayerController>());
        }
        Destroy(gameObject);
    }

    public void SetObject(ObjStats newObjStats)
    {
        objStats = newObjStats;
        transform.GetComponent<SpriteRenderer>().sprite = newObjStats.sprite;
    }

    private void AddItemToPlayer(PlayerController player)
    {
        player.moveSpeed += objStats.moveSpeed;
        player.bulletSpeed += objStats.bulletSpeed;
        player.bulletScale += objStats.bulletScale;
        player.fireRate += objStats.fireRate;
        if (player.fireRate <= PlayerController.MIN_CAP_FIRE_RATE)
            player.fireRate = PlayerController.MIN_CAP_FIRE_RATE;

        player.healthPoint += objStats.healthPoint;
        player.slowTime += objStats.slowTime;
        player.slowTimeRecoveryScale += objStats.slowTimeRecoveryScale;
        player.transform.localScale += new Vector3(objStats.playerScale, objStats.playerScale, 0.0f);

        player.currentHealth += objStats.healing;
        if (player.currentHealth > player.healthPoint)
            player.currentHealth = player.healthPoint;

        if (objStats.effect != ObjBehavior.EffectName.NONE && !player.holdingItems.Contains(objStats.effect))
            player.holdingItems.Add(objStats.effect);
    }
}
