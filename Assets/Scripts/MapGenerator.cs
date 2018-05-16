using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Chronometer
{
    public float period;
    public float time;

    public Chronometer(float initialTime, float constPeriod)
    {
        period = constPeriod;
        time = initialTime;
    }
    public void Update(float deltaTime)
    {
        if (time >= 0)
            time -= deltaTime;
    }
    public bool Over()
    {
        return time <= 0;
    }
    public void SetPeriod(float newPeriod)
    {
        if (newPeriod < time)
        {
            time = -1.0f;
        }
        period = newPeriod;
    }
    public void Reset()
    {
        time = period;
    }
    public float Current()
    {
        return (period - time) / period;
    }
    public float CurrentTime()
    {
        return period - time;
    }
    public float Remain()
    {
        return time;
    }
}

public class MapGenerator : MonoBehaviour
{
    public int sizeX = 5;
    public int sizeY = 5;

    public GameObject cellulePrefab;

    public Dictionary<Vector2, GameObject> cellules = new Dictionary<Vector2, GameObject>();
    public List<Vector2> emptyCells = new List<Vector2>();
    public List<List<Vector2>> zones = new List<List<Vector2>>();

    public float celluleInitRepartition = 0.5f;

    bool doOnce = true;
    Chronometer stepTimer = new Chronometer(5.0f, 1.0f);
    public int stepNumber = 10;
    // Use this for initialization
    void Start()
    {
        GenerateRandomInit();
    }

    GameObject CreateCellule(Vector2 pos, Color color)
    {
        GameObject cellule = Instantiate(cellulePrefab);
        cellule.transform.position = pos;
        cellule.transform.parent = transform;
        cellule.GetComponent<SpriteRenderer>().color = color;
        return cellule;
    }

    void GenerateRandomInit()
    {
        for (int i = -sizeX - 1; i <= sizeX + 1; i++)
        {
            for (int j = -sizeY - 1; j <= sizeY + 1; j++)
            {
                Vector2 pos = new Vector2(i, j);
                bool empty = (pos.x > 2 || pos.x < -2) || (pos.y > 2 || pos.y < -2);
                bool notEmpty = (pos.x > sizeX - 2 || pos.x < -sizeX + 2) || (pos.y > sizeY - 2 || pos.y < -sizeY + 2);
                if (Random.Range(0.0f, 1.0f) < celluleInitRepartition && empty || notEmpty)
                {
                    cellules[pos] = CreateCellule(pos, Color.black);
                }
                else
                {
                    emptyCells.Add(pos);
                }
            }
        }
    }

    void CellularStep()
    {
        List<Vector2> toDestroy = new List<Vector2>();
        List<Vector2> toCreate = new List<Vector2>();

        for (int i = -sizeX; i <= sizeX; i++)
        {
            for (int j = -sizeY; j <= sizeY; j++)
            {
                Vector2 pos = new Vector2(i, j);
                int neighborNmb = 0;
                //check the number of neighbor
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;
                        Vector2 neighborPos = pos + new Vector2(dx, dy);
                        if (cellules.ContainsKey(neighborPos))
                            neighborNmb++;
                    }
                }
                //Debug.Log (neighborNmb);
                if (cellules.ContainsKey(pos))
                {
                    if (neighborNmb < 4)
                    {
                        toDestroy.Add(pos);
                    }
                }
                else
                {
                    if (neighborNmb > 4)
                    {
                        toCreate.Add(pos);
                    }
                }
            }
        }
        foreach (Vector2 destroyedPos in toDestroy)
        {
            Destroy(cellules[destroyedPos]);
            cellules.Remove(destroyedPos);
            emptyCells.Add(destroyedPos);
        }
        foreach (Vector2 createdPos in toCreate)
        {
            cellules[createdPos] = CreateCellule(createdPos, Color.black);
            emptyCells.Remove(createdPos);
        }
        stepNumber--;
    }

    // Update is called once per frame
    void Update()
    {
        stepTimer.Update(Time.deltaTime);
        if (stepTimer.Over() && stepNumber >= 0)
        {
            CellularStep();
            stepTimer.Reset();
        }

        if(stepNumber < 0 && doOnce)
        {
            doOnce = false;
            foreach (Vector2 emptCell in emptyCells)
            {
                bool doNewZone = true;
                if (zones.Count != 0)
                {
                    foreach(List<Vector2> zone in zones)
                    {
                        if(zone.Contains(emptCell))
                        {
                            doNewZone = false;
                            break;
                        }
                    }
                }

                if(!doNewZone)
                {
                    continue;
                }

                int indexZone = zones.Count;
                List<Vector2> newZone = new List<Vector2>();

                List<Vector2> neightborNotInZone = new List<Vector2>();
                neightborNotInZone.Add(emptCell);

                while (neightborNotInZone.Count > 0)
                {
                    List<Vector2> newNeightborhood = new List<Vector2>();
                    foreach(Vector2 checkingCell in neightborNotInZone)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                if (dx == 0 && dy == 0)
                                    continue;

                                Vector2 neighborPos = checkingCell + new Vector2(dx, dy);
                                if (!newZone.Contains(neighborPos) && !newNeightborhood.Contains(neighborPos) && !cellules.ContainsKey(neighborPos))
                                {
                                    newZone.Add(neighborPos);
                                    newNeightborhood.Add(neighborPos);
                                }
                            }
                        }
                    }
                    neightborNotInZone = newNeightborhood;
                }
                zones.Add(newZone);
            }
            Debug.Log("Num Zones : " + zones.Count);
            Debug.Log("Num Empt Cells : " + emptyCells.Count);

            List<List<Vector2>> zonesToDelete = new List<List<Vector2>>();
            foreach (List<Vector2> zone in zones)
            {
                if (zone.Count < 50)
                    zonesToDelete.Add(zone);
            }

            foreach(List<Vector2> zone in zonesToDelete)
            {
                foreach (Vector2 cell in zone)
                {
                    cellules[cell] = CreateCellule(cell, Color.black);
                }
                zones.Remove(zone);
            }

            foreach (List<Vector2> zone in zones)
            {
                Color color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);
                foreach (Vector2 cell in zone)
                {
                    CreateCellule(cell, color);
                }
                Debug.Log(color.ToString() + " : " + zone.Count);
            }
        }
    }
}
