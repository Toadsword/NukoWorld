using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelParams", menuName = "Level", order = 2)]
public class LevelParams : ScriptableObject
{
    //Size
    public int sizeX = 40;
    public int sizeY = 40;
    public float celluleInitRepartition = 0.5f;

    //Treasures
    public int nbTreasure = 1;
     
    //Zones
    public int minZoneSize = 10;
    public int emptyZoneMidSize = 3;
    public int extWallsNotEmpty = 2;
           
    //Neighbor
    public int minNeighborNum = 3;
    public int maxNeighborNum = 5;

    //DemonSpawn
    public Vector2Int chunkSize = new Vector2Int(16, 16);
    public int numDemonsPerChunk = 2;
    public int maxLevelDemon = 4;
}
           
           
           

           
           