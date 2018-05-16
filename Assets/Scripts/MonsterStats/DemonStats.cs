using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DemonStats", menuName = "Demon", order = 1)]
public class DemonStats : ScriptableObject
{
    public string demonName = "Basic Demon";
    public float moveSpeed = 3.0f;
    public float bulletSpeed = 6.0f;
    public float bulletScale = 1.0f;
    public float fireRate = 2.0f;
    public int healthPoint = 3;
    public bool doTripleShot = false;
    public Sprite sprite;
}