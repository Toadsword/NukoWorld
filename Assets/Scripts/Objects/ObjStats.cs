using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjStats", menuName = "Object", order = 3)]
public class ObjStats : ScriptableObject {
    public float moveSpeed = 0.0f;
    public float bulletSpeed = 0.0f;
    public int bulletDamage = 0;
    public float bulletScale = 0.0f;
    public float fireRate = 0.0f;
    public int healthPoint = 0;
    public int healing = 0;
    public float slowTime = 0.0f;
    public float slowTimeRecoveryScale = 0.0f;
    public float playerScale = 0.0f;
    public ObjBehavior.EffectName effect = ObjBehavior.EffectName.NONE;
    public Sprite sprite;
    public string name;
    public string description;
}
