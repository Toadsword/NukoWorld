using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int sizeX = 40;
    public int sizeY = 40;

    public int sizeChunk = 10;

    public GameObject cellulePrefab;

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

    public Dictionary<Vector2Int, GameObject> cellules = new Dictionary<Vector2Int, GameObject>();
    public List<Vector2Int> emptyCells = new List<Vector2Int>();
    public List<List<Vector2Int>> zones = new List<List<Vector2Int>>();

    public float celluleInitRepartition = 0.5f;

    bool doOnce = true;
    float timeBetweenSteps = 0.5f;
    float stepTimer;
    public int stepNumber = 10;
    // Use this for initialization
    void Start()
    {
        stepTimer = Utility.StartTimer(timeBetweenSteps * 5);
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
                Vector2Int pos = new Vector2Int(i, j);
                bool empty = (pos.x > 3 || pos.x < -3) || (pos.y > 3 || pos.y < -3);
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
        foreach (Vector2Int destroyedPos in toDestroy)
        {
            Destroy(cellules[destroyedPos]);
            cellules.Remove(destroyedPos);
            emptyCells.Add(destroyedPos);
        }
        foreach (Vector2Int createdPos in toCreate)
        {
            cellules[createdPos] = CreateCellule(createdPos, Color.black);
            emptyCells.Remove(createdPos);
        }
        stepNumber--;
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
    }

    void RemoveLittleZones()
    {
        // Removing little zones and getting the biggest one
        List<List<Vector2Int>> zonesToDelete = new List<List<Vector2Int>>();
        foreach (List<Vector2Int> zone in zones)
        {
            if (zone.Count < 10)
                zonesToDelete.Add(zone);
        }
        foreach (List<Vector2Int> zone in zonesToDelete)
        {
            foreach (Vector2Int cell in zone)
            {
                cellules[cell] = CreateCellule(cell, Color.black);
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
                        if(emptyCells.Contains(pos) && !zone.Contains(pos))
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
            
            /*
            if (Mathf.Sign(direction.x) == Mathf.Sign(center.x) && Mathf.Sign(direction.y) == Mathf.Sign(center.y))
            {
                direction.x *= -1;
                direction.y *= -1;
            }
            */
            if (cellules.ContainsKey(center))
            {
                traveledDist = -1;
                Vector2Int prevPos = center - direction * traveledDist;
                while (cellules.ContainsKey(prevPos))
                {
                    traveledDist--;
                    prevPos = center - direction * traveledDist;
                }
            }

            List<Vector2Int> toDestroy = new List<Vector2Int>();
            while (traveledDist <= multiplier + 1)
            {
                Vector2Int pos = center + direction * traveledDist;
                if(cellules.ContainsKey(pos))
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

                if (cellules.ContainsKey(neightbor1))
                    toDestroy.Add(neightbor1);
                if (cellules.ContainsKey(neightbor2))
                    toDestroy.Add(neightbor2);

                traveledDist++;
            }

            foreach (Vector2Int destroyedPos in toDestroy)
            {
                if (!cellules.ContainsKey(destroyedPos))
                    continue;
                Destroy(cellules[destroyedPos]);
                cellules.Remove(destroyedPos);
                emptyCells.Add(destroyedPos);
            }
        }
    }
    
    void SetSprites()
    {
        foreach(Vector2Int cell in emptyCells)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    Vector2Int neighborPos = cell + new Vector2Int(dx, dy);
                    if (cellules.ContainsKey(neighborPos))
                    {
                        AssignSprite(neighborPos);
                    }
                }
            }
        }
    }

    void AssignSprite(Vector2Int cell)
    {
        Sprite[] sprites = null;
        int layoutLevel = 0;

        bool isCellTop = cellules.ContainsKey(cell + new Vector2Int(0, 1));
        bool isCellBot = cellules.ContainsKey(cell + new Vector2Int(0, -1));
        bool isCellLeft = cellules.ContainsKey(cell + new Vector2Int(-1, 0));
        bool isCellRight = cellules.ContainsKey(cell + new Vector2Int(1, 0));

        bool isCellTopLeft = cellules.ContainsKey(cell + new Vector2Int(-1, 1));
        bool isCellTopRight = cellules.ContainsKey(cell + new Vector2Int(1, 1));
        bool isCellBotLeft = cellules.ContainsKey(cell + new Vector2Int(-1, -1));
        bool isCellBotRight = cellules.ContainsKey(cell + new Vector2Int(1, -1));

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

            layoutLevel = 1;
        }
        else
        {
            // Walls Middle
            if (isCellTop && isCellBot && isCellLeft)
                sprites = spritesWallRight;
            else if (isCellTop && isCellBot && isCellRight)
                sprites = spritesWallLeft;
            else if (isCellTop && isCellRight && isCellLeft)
                sprites = spritesWallBottomMid;
            else if (isCellBot && isCellRight && isCellLeft)
                sprites = spritesWallTopMid;

            //WallsCorner
            else if (isCellBot && isCellRight)
                sprites = spritesWallTopLeft;
            else if (isCellTop && isCellRight)
                sprites = spritesWallBotLeft;
            else if (isCellBot && isCellLeft)
                sprites = spritesWallTopRight;
            else if (isCellTop && isCellLeft)
                sprites = spritesWallBotRight;
        }

        if (sprites == null)
        {
            Debug.Log("ERROR PARSING TILE");
            return;
        }

        int index = Random.Range(0, sprites.Length - 1);

        cellules[cell].GetComponent<SpriteRenderer>().sprite = sprites[index];
        cellules[cell].GetComponent<SpriteRenderer>().color = Color.white;
        cellules[cell].GetComponent<SpriteRenderer>().sortingOrder = layoutLevel;
    }

    // Update is called once per frame
    void Update()
    {
        if (Utility.IsOver(stepTimer) && stepNumber >= 0)
        {
            CellularStep();
            stepTimer = Utility.StartTimer(timeBetweenSteps);
        }

        if (stepNumber < 0 && zones.Count == 0)
        {
            CalculateEmptyZones();
            RemoveLittleZones();
        }

        if (stepNumber < 0 && zones.Count > 1)
        {
            JoinZones();

            //Coloring little zones
            // TODO : REMOVE OR COMMENT THIS 
            stepNumber = 3;
            stepTimer = Utility.StartTimer(timeBetweenSteps * 2);

            zones.Clear();
            CalculateEmptyZones();
        }

        if(zones.Count == 1 && stepNumber < 0 && doOnce)
        {
            SetSprites();
            Debug.Log("DONE");
        }
    }
}
