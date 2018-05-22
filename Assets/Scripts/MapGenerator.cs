using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    private const int MAX_PASSAGE_LENGHT_CHECK = 29;
    private const float BASE_CHANCE_OF_SPAWN = 0.2f;

    [Header("WorldGeneratorParams")]
    [SerializeField] LevelParams[] levelParams;
    [SerializeField] int levelNum = 1;

    [Header("WorldGeneratorParams/Size")]
    [SerializeField] int sizeX = 40;
    [SerializeField] int sizeY = 40;

    [SerializeField] float celluleInitRepartition = 0.5f;
    [SerializeField] int nbTreasure = 1;

    [SerializeField] GameObject cellulePrefab; 
    [SerializeField] GameObject floorPrefab;

    [Header("WorldGeneratorParams/Steps")]
    [SerializeField] int numStep = 12;
    [SerializeField] int numStepAfterZone = 2;
    [SerializeField] int numStepFinish = 20;
    [SerializeField] float timeBetweenSteps = 0.1f;

    [Header("WorldGeneratorParams/Zones")]
    [SerializeField] int minZoneSize = 10;
    [SerializeField] int emptyZoneMidSize = 3;
    [SerializeField] int extWallsNotEmpty = 2;

    [Header("WorldGeneratorParams/Cell Kill or Creation")]
    [SerializeField] int minNeighborNum = 3;
    [SerializeField] int maxNeighborNum = 5;

    [Header("DemonSpawn")]
    [SerializeField] Vector2Int chunkSize = new Vector2Int(16, 16);
    [SerializeField] int numDemonsPerChunk;
    /* NOT WORKING, ONLY FOR STATS */
    // 1 == only Basics
    // 2 == Basics + rush
    // 3 == Basics + High
    // 4 == All     
    [SerializeField] int maxLevelDemon;

    [Header("Objects")]
    [SerializeField] ObjStats[] objects;

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

    [Header("Prefabs")]
    [SerializeField] GameObject stairPrefab;
    [SerializeField] GameObject treasurePrefab;

    public Dictionary<Vector2Int, GameObject> cells = new Dictionary<Vector2Int, GameObject>();
    public List<Vector2Int> emptyCells = new List<Vector2Int>();
    public List<List<Vector2Int>> zones = new List<List<Vector2Int>>();

    public List<Vector2Int> entranceZone = new List<Vector2Int>();
    public Vector2Int entrancePlace = new Vector2Int(0, 0);

    public List<Vector2Int> stairZone = new List<Vector2Int>();
    public Vector2Int stairPlace = new Vector2Int(0, 0);

    public List<Vector2Int>[] treasureZones;
    public Vector2Int[] treasurePlaces;

    public bool isLevelSet = false;
    bool isDone = false;
    bool hasFinishedMap = false;
    float stepTimer;

    SceneManagement smInstance;
    DemonCreator dcInstance;

    // Use this for initialization
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        smInstance = FindObjectOfType<SceneManagement>();
        dcInstance = FindObjectOfType<DemonCreator>();

        stepTimer = Utility.StartTimer(timeBetweenSteps * 5);
        isLevelSet = false;
        if(smInstance == null)
            SetLevel(levelNum);
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

    GameObject CreateSlate(Vector2 pos)
    {
        GameObject slate = Instantiate(floorPrefab);
        slate.transform.position = pos;
        slate.transform.parent = transform;
        int index = Random.Range(0, spritesSlate.Length);
        slate.GetComponent<SpriteRenderer>().sprite = spritesSlate[index];
        slate.GetComponent<SpriteRenderer>().sortingOrder = 2;
        return slate;
    }

    void SpawnDemons()
    {
        int multiplierX = 0;
        int multiplierY = 0;
        int demonCreated = 0;

        while (multiplierX * chunkSize.x < sizeX * 2 && multiplierY * chunkSize.y < sizeY * 2)
        {
            // Don't spawn a demon on the player !
            if(chunkSize.x * multiplierX < entrancePlace.x &&  entrancePlace.x < chunkSize.x * (multiplierX + 1) &&
               chunkSize.y * multiplierY < entrancePlace.y && entrancePlace.y < chunkSize.y * (multiplierY + 1))
                multiplierX++;

            for (int dx = -sizeX + chunkSize.x * multiplierX; dx <= -sizeX + chunkSize.x * (multiplierX + 1); dx++)
            {
                for (int dy = -sizeY + chunkSize.y * multiplierY; dy <= -sizeY + chunkSize.y * (multiplierY + 1); dy++)
                {
                    Vector2Int pos = new Vector2Int(dx, dy);
                    if (emptyCells.Contains(pos) && Random.Range(0.0f, 1.0f) < BASE_CHANCE_OF_SPAWN)
                    {
                        //Spawn Demon at this place
                        dcInstance.CreateDemon(new Vector3(pos.x, pos.y, 0), DemonCreator.DemonTypes.BASIC_DEMON);
                        demonCreated++;
                    }
                    if (demonCreated == numDemonsPerChunk)
                        break;
                }
                if (demonCreated == numDemonsPerChunk)
                    break;
            }
            if ((multiplierX + 1) * chunkSize.x > sizeX * 2)
            {
                multiplierX = 0;
                multiplierY++;
            }
            else
            {
                multiplierX++;
            }

            demonCreated = 0;
        }
    }

    void GenerateRandomInit()
    {
        for (int i = -sizeX - 1 - extWallsNotEmpty; i <= sizeX + 1 + extWallsNotEmpty; i++)
        {
            for (int j = -sizeY - 1 - extWallsNotEmpty; j <= sizeY + 1 + extWallsNotEmpty; j++)
            {
                Vector2Int pos = new Vector2Int(i, j);
                bool empty = (pos.x > emptyZoneMidSize || pos.x < -emptyZoneMidSize) || (pos.y > emptyZoneMidSize || pos.y < -emptyZoneMidSize);
                bool notEmpty = (pos.x > sizeX || pos.x < -sizeX) || (pos.y > sizeY || pos.y < -sizeY);
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
            if(entranceZone.Count == 0)
            {
                entranceZone = zone;
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
            if (!assigned && stairZone.Count == 0)
            {
                stairZone = zone;
                assigned = true;
            }

            if (zone.Count < minZoneSize && !assigned)
                zonesToDelete.Add(zone);
        }

        if (entranceZone.Count == 0)
            entranceZone = biggestZone;

        if (stairZone.Count == 0)
            stairZone = biggestZone;

        for (int i = 0; i < treasureZones.Length; i++)
        {
            if (treasureZones[i].Count == 0)
                treasureZones[i] = biggestZone;
        }

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

                if (multiplier > MAX_PASSAGE_LENGHT_CHECK)
                    break;
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

                    if (traveledDist < MAX_PASSAGE_LENGHT_CHECK)
                        break;
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

            /* DEBUG COLORING TILES TO SHOW ZONES
            if (entranceZone.Contains(cell))
                obj.GetComponent<SpriteRenderer>().color = Color.green;

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
            */
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

    GameObject SetPlace(ref List<Vector2Int> varZone,ref Vector2Int varPlace, GameObject prefab)
    {
        int index = Random.Range(0, varZone.Count);
        varPlace = varZone[index];
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;
                Vector2Int neighborPos = varPlace + new Vector2Int(dx, dy);
                if (cells.ContainsKey(neighborPos))
                {
                    Destroy(cells[neighborPos]);
                    cells.Remove(neighborPos);
                    emptyCells.Add(neighborPos);
                    varZone.Add(neighborPos);
                }
            }
        }
        CreateSlate(varPlace);

        if(prefab != null)
        {
            GameObject obj = Instantiate(prefab, (Vector2)varPlace, prefab.transform.rotation);
            obj.transform.parent = transform;
            return obj;
        }
        return null;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLevelSet)
            return;

        if (Utility.IsOver(stepTimer) && numStep >= 0)
        {
            CellularStep();
            stepTimer = Utility.StartTimer(timeBetweenSteps);
        }

        if (Utility.IsOver(stepTimer) && numStep < 0 && zones.Count == 0)
        {
            CalculateEmptyZones();
            RemoveLittleZones();
            stepTimer = Utility.StartTimer(timeBetweenSteps);
        }

        if (Utility.IsOver(stepTimer) && numStep < 0 && zones.Count > 1)
        {
            JoinZones();
            
            numStep = numStepAfterZone;
            stepTimer = Utility.StartTimer(timeBetweenSteps * 2);

            zones.Clear();
            CalculateEmptyZones();
        }

        if(Utility.IsOver(stepTimer) && zones.Count == 1 && numStep < 0 && !hasFinishedMap)
        {
            hasFinishedMap = true;
            numStep = numStepFinish;
            stepTimer = Utility.StartTimer(timeBetweenSteps);
        }

        if(Utility.IsOver(stepTimer) && zones.Count == 1 && numStep < 0 && hasFinishedMap && !isDone)
        {
            RemoveImperfections();

            SetPlace(ref entranceZone, ref entrancePlace, null);
            SetPlace(ref stairZone, ref stairPlace, stairPrefab);
            for (int i = 0; i < treasureZones.Length; i++)
            {
                GameObject obj = SetPlace(ref treasureZones[i], ref treasurePlaces[i], treasurePrefab);
                int index = Random.Range(0, objects.Length);
                obj.GetComponent<ObjBehavior>().SetObject(objects[index]);
            }
            SpawnDemons();

            SetSprites();
            isDone = true;

            Debug.Log("DONE");
            smInstance.ChangeScene(SceneManagement.Scenes.GAME);
        }
    }

    public void SetLevel(int level)
    {
        if (levelParams[level] != null)
        {
            sizeX = levelParams[level].sizeX;
            sizeY = levelParams[level].sizeY;

            celluleInitRepartition = levelParams[level].celluleInitRepartition;
            nbTreasure = levelParams[level].nbTreasure;

            minZoneSize = levelParams[level].minZoneSize;
            emptyZoneMidSize = levelParams[level].emptyZoneMidSize;
            extWallsNotEmpty = levelParams[level].extWallsNotEmpty;

            minNeighborNum = levelParams[level].minNeighborNum;
            maxNeighborNum = levelParams[level].maxNeighborNum;

            chunkSize = levelParams[level].chunkSize;
            numDemonsPerChunk = levelParams[level].numDemonsPerChunk;
            maxLevelDemon = levelParams[level].maxLevelDemon;
        }
        levelNum = level;

        treasureZones = new List<Vector2Int>[nbTreasure];
        treasurePlaces = new Vector2Int[nbTreasure];
        for (int i = 0; i < treasureZones.Length; i++)
        {
            treasureZones[i] = new List<Vector2Int>();
            treasurePlaces[i] = new Vector2Int();
        }

        GenerateRandomInit();
        isLevelSet = true;
    }
}
