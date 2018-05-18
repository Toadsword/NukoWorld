using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("WorldGeneratorParams")]
    [Header("WorldGeneratorParams/Size")]
    [SerializeField] int sizeX = 40;
    [SerializeField] int sizeY = 40;

    [SerializeField] float celluleInitRepartition = 0.5f;
    [SerializeField] int sizeChunk = 10;
    [SerializeField] int nbTreasure = 1;

    [SerializeField] GameObject cellulePrefab; 
    [SerializeField] GameObject floorPrefab;

    [Header("WorldGeneratorParams/Steps")]
    [SerializeField] int numStep = 10;
    [SerializeField] int numStepAfterZone = 2;
    [SerializeField] int numStepFinish = 2;
    [SerializeField] float timeBetweenSteps = 0.5f;

    [Header("WorldGeneratorParams/Zones")]
    [SerializeField] int minZoneSize = 10;
    [SerializeField] int emptyZoneMidSize = 3;
    [SerializeField] int extWallsNotEmpty = 2;

    [Header("WorldGeneratorParams/Cell Kill or Creation")]
    [SerializeField] int minNeighborNum = 3;
    [SerializeField] int maxNeighborNum = 5;

    [Header("Sprites")]
    [SerializeField] Sprite[] spritesWallLeft;
    [SerializeField] Sprite[] spritesWallRight;
    [SerializeField] Sprite[] spritesWallBotLeft;
    [SerializeField] Sprite[] spritesWallBottomMid;
    [SerializeField] Sprite[] spritesWallBotRight;
    [SerializeField] Sprite[] spritesWallTopLeft;
    [SerializeField] Sprite[] spritesWallTopMid;
    [SerializeField] Sprite[] spritesWallTopRight;

    [SerializeField] Sprite[] spritesRaccBotLeft;
    [SerializeField] Sprite[] spritesRaccBotRight;
    [SerializeField] Sprite[] spritesRaccTopLeft;
    [SerializeField] Sprite[] spritesRaccTopRight;

    [SerializeField] Sprite[] spritesWallInside;

    [SerializeField] Sprite[] spritesFloor;
    [SerializeField] Sprite[] spritesSlate;

    public Dictionary<Vector2Int, GameObject> cells = new Dictionary<Vector2Int, GameObject>();
    public List<Vector2Int> emptyCells = new List<Vector2Int>();
    public List<List<Vector2Int>> zones = new List<List<Vector2Int>>();

    public List<Vector2Int> stairZone = new List<Vector2Int>();
    public List<Vector2Int>[] treasureZones;

    public Vector2Int[] treasurePlaces;
    public Vector2Int stairPlace = new Vector2Int(0, 0);

    bool isDone = false;
    bool hasFinishingMap = false;
    float stepTimer;
    // Use this for initialization
    void Start()
    {
        stepTimer = Utility.StartTimer(timeBetweenSteps * 5);
        GenerateRandomInit();

        treasureZones = new List<Vector2Int>[nbTreasure];
        treasurePlaces = new Vector2Int[nbTreasure];
        for (int i = 0; i < treasureZones.Length; i++)
        {
            treasureZones[i] = new List<Vector2Int>();
            treasurePlaces[i] = new Vector2Int();
        }
    }

    GameObject CreateCellule(Vector2 pos, Color color)
    {
        GameObject cellule = Instantiate(cellulePrefab);
        cellule.transform.position = pos;
        cellule.transform.parent = transform;
        cellule.GetComponent<SpriteRenderer>().color = color;
        return cellule;
    }

    GameObject CreateFloor(Vector2 pos, Color color)
    {
        GameObject cellule = Instantiate(floorPrefab);
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
                Vector2Int pos = new Vector2Int(i, j);
                bool empty = (pos.x > emptyZoneMidSize || pos.x < -emptyZoneMidSize) || (pos.y > emptyZoneMidSize || pos.y < -emptyZoneMidSize);
                bool notEmpty = (pos.x > sizeX - extWallsNotEmpty || pos.x < -sizeX + extWallsNotEmpty) || (pos.y > sizeY - extWallsNotEmpty || pos.y < -sizeY + extWallsNotEmpty);
                if (Random.Range(0.0f, 1.0f) < celluleInitRepartition && empty || notEmpty)
                {
                    cells[pos] = CreateCellule(pos, Color.black);
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
        List<Vector2Int> toDestroy = new List<Vector2Int>();
        List<Vector2Int> toCreate = new List<Vector2Int>();

        for (int i = -sizeX; i <= sizeX; i++)
        {
            for (int j = -sizeY; j <= sizeY; j++)
            {
                Vector2Int pos = new Vector2Int(i, j);
                int neighborNmb = 0;
                //check the number of neighbor
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;
                        Vector2Int neighborPos = pos + new Vector2Int(dx, dy);
                        if (cells.ContainsKey(neighborPos))
                            neighborNmb++;
                    }
                }
                //Debug.Log (neighborNmb);
                if (cells.ContainsKey(pos))
                {
                    if (neighborNmb <= minNeighborNum)
                    {
                        toDestroy.Add(pos);
                    }
                }
                else
                {
                    if (neighborNmb >= maxNeighborNum)
                    {
                        toCreate.Add(pos);
                    }
                }
            }
        }
        foreach (Vector2Int destroyedPos in toDestroy)
        {
            Destroy(cells[destroyedPos]);
            cells.Remove(destroyedPos);
            emptyCells.Add(destroyedPos);
        }
        foreach (Vector2Int createdPos in toCreate)
        {
            cells[createdPos] = CreateCellule(createdPos, Color.black);
            emptyCells.Remove(createdPos);
        }
        numStep--;
    }

    void CalculateEmptyZones()
    {
        foreach (Vector2Int emptCell in emptyCells)
        {
            bool doNewZone = true;
            if (zones.Count != 0)
            {
                foreach (List<Vector2Int> zone in zones)
                {
                    if (zone.Contains(emptCell))
                    {
                        doNewZone = false;
                        break;
                    }
                }
            }

            if (!doNewZone)
            {
                continue;
            }

            int indexZone = zones.Count;
            List<Vector2Int> newZone = new List<Vector2Int>();

            List<Vector2Int> neightborNotInZone = new List<Vector2Int>();
            neightborNotInZone.Add(emptCell);

            while (neightborNotInZone.Count > 0)
            {
                List<Vector2Int> newNeightborhood = new List<Vector2Int>();
                foreach (Vector2Int checkingCell in neightborNotInZone)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0)
                                continue;

                            Vector2Int neighborPos = checkingCell + new Vector2Int(dx, dy);
                            if (!newZone.Contains(neighborPos) && !newNeightborhood.Contains(neighborPos) && !cells.ContainsKey(neighborPos))
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
    }

    void RemoveLittleZones()
    {

        // Removing little zones and getting the biggest one
        List<List<Vector2Int>> zonesToDelete = new List<List<Vector2Int>>();
        List<Vector2Int> biggestZone = new List<Vector2Int>();
        foreach (List<Vector2Int> zone in zones)
        {
            if (biggestZone.Count < zone.Count)
                biggestZone = zone;
        }

        foreach (List<Vector2Int> zone in zones)
        {
            if (zone == biggestZone)
                continue;

            bool assigned = false;
            if (stairZone.Count == 0)
            {
                stairZone = zone;
                assigned = true;
            }
            else
            {
                for(int i = 0; i < treasureZones.Length; i++)
                {
                    if (treasureZones[i].Count == 0)
                    {
                        treasureZones[i] = zone;
                        assigned = true;
                        break;
                    }
                }
            }

            if (zone.Count < minZoneSize && !assigned)
                zonesToDelete.Add(zone);
        }

        for (int i = 0; i < treasureZones.Length; i++)
        {
            if (treasureZones[i].Count == 0)
                treasureZones[i] = biggestZone;
        }

        if (stairZone.Count == 0)
            stairZone = biggestZone;

        foreach (List<Vector2Int> zone in zonesToDelete)
        {
            foreach (Vector2Int cell in zone)
            {
                cells[cell] = CreateCellule(cell, Color.black);
                emptyCells.Remove(cell);
            }
            zones.Remove(zone);
        }
    }

    void JoinZones()
    {
        List<Vector2Int> biggestZone = new List<Vector2Int>();

        foreach (List<Vector2Int> zone in zones)
        {
            if (biggestZone.Count < zone.Count)
            {
                biggestZone = zone;
            }
        }

        foreach (List<Vector2Int> zone in zones)
        {
            if (zone.Count == biggestZone.Count)
                continue;

            Vector2Int zoneTopLeft = new Vector2Int(sizeX, sizeY);
            Vector2Int zoneBotRight = new Vector2Int(-sizeX, -sizeY);

            foreach(Vector2Int cell in zone)
            {
                if (zoneTopLeft.x > cell.x)
                    zoneTopLeft.x = cell.x;
                if (zoneTopLeft.y > cell.y)
                    zoneTopLeft.y = cell.y;
                if (zoneBotRight.x < cell.x)
                    zoneBotRight.x = cell.x;
                if (zoneBotRight.y < cell.y)
                    zoneBotRight.y = cell.y;
            }

            Vector2Int center = zoneTopLeft + zoneBotRight;
            center = new Vector2Int(center.x / 2, center.y / 2);

            Vector2Int closestCell = center;
            int multiplier = 1;
            int traveledDist = 0;
            while (closestCell == center)
            {
                for(int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;
                        Vector2Int pos = center + new Vector2Int(dx * multiplier, dy * multiplier);
                        if(emptyCells.Contains(pos) && !zone.Contains(pos) && !cells.ContainsKey(pos))
                        {
                            closestCell = pos;
                        }
                    }
                }
                multiplier++;
            }

            Vector2Int direction = closestCell - center;
            if(direction.x != 0)
                direction.x /= Mathf.Abs(direction.x);
            if (direction.y != 0)
                direction.y /= Mathf.Abs(direction.y);

            if (cells.ContainsKey(center))
            {
                traveledDist = -1;
                Vector2Int prevPos = center - direction * traveledDist;
                while (cells.ContainsKey(prevPos) && prevPos.x > -sizeX && prevPos.x < sizeX && prevPos.y > -sizeY && prevPos.y < sizeY)
                {
                    traveledDist--;
                    prevPos = center - direction * traveledDist;
                }

                if (!(prevPos.x > -sizeX && prevPos.x < sizeX && prevPos.y > -sizeY && prevPos.y < sizeY))
                    continue;
            }
            
            List<Vector2Int> toDestroy = new List<Vector2Int>();
            while (traveledDist <= multiplier + 1)
            {
                Vector2Int pos = center + direction * traveledDist;
                if(cells.ContainsKey(pos))
                {
                    toDestroy.Add(pos);
                }

                Vector2Int neightbor1 = new Vector2Int(0, 0);
                Vector2Int neightbor2 = new Vector2Int(0, 0);
                if (direction.x != 0 && direction.y != 0)
                {
                    neightbor1 = pos + new Vector2Int(direction.x, 0);
                    neightbor2 = pos + new Vector2Int(0, direction.y);
                }
                else
                {
                    Vector2Int invertDirection = new Vector2Int(direction.y, direction.x);
                    neightbor1 = pos - invertDirection;
                    neightbor2 = pos + invertDirection;
                }

                if (cells.ContainsKey(neightbor1))
                    toDestroy.Add(neightbor1);
                if (cells.ContainsKey(neightbor2))
                    toDestroy.Add(neightbor2);

                traveledDist++;
            }

            foreach (Vector2Int destroyedPos in toDestroy)
            {
                if (!cells.ContainsKey(destroyedPos))
                    continue;
                Destroy(cells[destroyedPos]);
                cells.Remove(destroyedPos);
                emptyCells.Add(destroyedPos);
            }
        }
    }

    void RemoveImperfections()
    {
        List<Vector2Int> toDestroy = new List<Vector2Int>();

        foreach (KeyValuePair<Vector2Int, GameObject> cell in cells)
        {
            int numNeightbor = 0;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    Vector2Int pos = cell.Key + new Vector2Int(dx, dy);
                    if (cells.ContainsKey(pos))
                    {
                        numNeightbor++;
                    }
                }
            }

            if (numNeightbor == 3)
            {
                toDestroy.Add(cell.Key);
            }
            else if (numNeightbor == 4)
            {
                int validOnes = 0;

                if (cells.ContainsKey(cell.Key + new Vector2Int(0, 1)))
                    validOnes++;
                if (cells.ContainsKey(cell.Key + new Vector2Int(0, -1)))
                    validOnes++;
                if (cells.ContainsKey(cell.Key + new Vector2Int(-1, 0)))
                    validOnes++;
                if (cells.ContainsKey(cell.Key + new Vector2Int(1, 0)))
                    validOnes++;

                if(validOnes == 1)
                    toDestroy.Add(cell.Key);
            }
        }

        foreach (Vector2Int destroyedPos in toDestroy)
        {
            Destroy(cells[destroyedPos]);
            cells.Remove(destroyedPos);
            emptyCells.Add(destroyedPos);
        }
    }

    void SetSprites()
    {
        foreach(Vector2Int cell in emptyCells)
        {
            Sprite[] sprites = spritesFloor; 

            GameObject obj = CreateFloor(cell, Color.white);
            int index = Random.Range(0, sprites.Length);

            obj.GetComponent<SpriteRenderer>().sprite = sprites[index];
            obj.GetComponent<SpriteRenderer>().sortingOrder = 0;
            if(index == 0)
                obj.GetComponent<SpriteRenderer>().sortingOrder = 1;

            if (stairZone.Contains(cell))
                obj.GetComponent<SpriteRenderer>().color = Color.gray;

            for (int i = 0; i < treasureZones.Length; i++)
            {
                if (treasureZones[i].Contains(cell))
                {
                    obj.GetComponent<SpriteRenderer>().color = Color.yellow;
                    continue;
                }
            }                
        }

        foreach(KeyValuePair<Vector2Int, GameObject> cell in cells)
        {
            Sprite[] sprites = spritesWallInside;
            int layoutLevel = 0;
            Vector2Int cellPos = cell.Key;

            bool isCellTop = cells.ContainsKey(cellPos + new Vector2Int(0, 1));
            bool isCellBot = cells.ContainsKey(cellPos + new Vector2Int(0, -1));
            bool isCellLeft = cells.ContainsKey(cellPos + new Vector2Int(-1, 0));
            bool isCellRight = cells.ContainsKey(cellPos + new Vector2Int(1, 0));

            bool isCellTopLeft = cells.ContainsKey(cellPos + new Vector2Int(-1, 1));
            bool isCellTopRight = cells.ContainsKey(cellPos + new Vector2Int(1, 1));
            bool isCellBotLeft = cells.ContainsKey(cellPos + new Vector2Int(-1, -1));
            bool isCellBotRight = cells.ContainsKey(cellPos + new Vector2Int(1, -1));

            if (isCellTop && isCellBot && isCellLeft && isCellRight)
            {
                // Corners
                if (!isCellTopLeft)
                    sprites = spritesRaccTopLeft;
                else if (!isCellTopRight)
                    sprites = spritesRaccTopRight;
                else if (!isCellBotLeft)
                    sprites = spritesRaccBotLeft;
                else if (!isCellBotRight)
                    sprites = spritesRaccBotRight;

                layoutLevel = 2;
            }
            else
            {
                layoutLevel = 3;
                // Walls Middle
                if (isCellTop && isCellBot && isCellLeft)
                    sprites = spritesWallRight;
                else if (isCellTop && isCellBot && isCellRight)
                    sprites = spritesWallLeft;
                else if (isCellTop && isCellRight && isCellLeft)
                    sprites = spritesWallBottomMid;
                else if (isCellBot && isCellRight && isCellLeft)
                {
                    sprites = spritesWallTopMid;
                    layoutLevel++;
                }

                //WallsCorner
                else if (isCellBot && isCellRight)
                {
                    sprites = spritesWallTopLeft;
                    layoutLevel++;
                }
                else if (isCellTop && isCellRight)
                    sprites = spritesWallBotLeft;
                else if (isCellBot && isCellLeft)
                { 
                    sprites = spritesWallTopRight;
                    layoutLevel++;
                }
                else if (isCellTop && isCellLeft)
                    sprites = spritesWallBotRight;
            }

            if (isCellTop && isCellBot && isCellLeft && isCellRight && isCellTopLeft && isCellTopRight && isCellBotLeft && isCellBotRight)
                layoutLevel = 0;

            if (sprites == null)
            {
                Debug.Log("ERROR PARSING TILE");
                return;
            }

            int index = Random.Range(0, sprites.Length);

            cell.Value.GetComponent<SpriteRenderer>().sprite = sprites[index];
            cell.Value.GetComponent<SpriteRenderer>().color = Color.white;
            cell.Value.GetComponent<SpriteRenderer>().sortingOrder = layoutLevel;
        }
    }

    void SetTreasurePlaces()
    {
        for (int i = 0; i < treasureZones.Length; i++)
        {
            int index = Random.Range(0, treasureZones[i].Count);
            treasurePlaces[i] = treasureZones[i][index];

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;
                    Vector2Int neighborPos = treasurePlaces[i] + new Vector2Int(dx, dy);
                    if (cells.ContainsKey(neighborPos))
                    {
                        Destroy(cells[neighborPos]);
                        cells.Remove(neighborPos);
                        emptyCells.Add(neighborPos);
                        treasureZones[i].Add(neighborPos);
                    }
                }
            }
            Debug.Log(treasurePlaces[i].ToString());
        }
    }

    void SetStairsPlace()
    {
        int index = Random.Range(0, stairZone.Count);
        stairPlace = stairZone[index];
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;
                Vector2Int neighborPos = stairPlace + new Vector2Int(dx, dy);
                if (cells.ContainsKey(neighborPos))
                {
                    Destroy(cells[neighborPos]);
                    cells.Remove(neighborPos);
                    emptyCells.Add(neighborPos);
                    stairZone.Add(neighborPos);
                }
            }
        }
        Debug.Log(stairPlace.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        if (Utility.IsOver(stepTimer) && numStep >= 0)
        {
            CellularStep();
            stepTimer = Utility.StartTimer(timeBetweenSteps);
        }

        if (numStep < 0 && zones.Count == 0)
        {
            CalculateEmptyZones();
            RemoveLittleZones();
        }

        if (numStep < 0 && zones.Count > 1)
        {
            JoinZones();
            
            numStep = numStepAfterZone;
            stepTimer = Utility.StartTimer(timeBetweenSteps * 2);

            zones.Clear();
            CalculateEmptyZones();
        }

        if(zones.Count == 1 && numStep < 0 && !hasFinishingMap)
        {
            hasFinishingMap = true;
            numStep = numStepFinish;
        }

        if(zones.Count == 1 && numStep < 0 && hasFinishingMap && !isDone)
        {
            RemoveImperfections();

            SetTreasurePlaces();
            SetStairsPlace();

            SetSprites();
            isDone = true;

            Debug.Log("DONE");
        }
    }
}
