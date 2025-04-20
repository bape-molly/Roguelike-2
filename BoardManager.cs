using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;



public class BoardManager : MonoBehaviour
{
    public class CellData
    {
        public bool Passable;
        public CellObject ContainedObject;
    }

    private CellData[,] m_BoardData;

    private Tilemap m_Tilemap;

    private Grid m_Grid;

    public ExitCellObject ExitCellPrefab;
    public int Width;
    public int Height;
    public Tile[] GroundTiles;
    public Tile[] WallTiles;
    public PlayerController Player;
    public FoodObject[] FoodPrefab;
    public List<Vector2Int> m_EmptyCellsList;
    public Enemy[] EnemyPrefab;

    public ItemObject[] ItemPrefabs;

    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
        m_Tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }
    public WallObject WallPrefab;

    // Chuyển chỉ mục ô (Vector2Int) thành vị trí thế giới


    // Start is called before the first frame update

    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return m_Tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x, cellIndex.y, 0));
    }

    public void Init()
    {
        m_Tilemap = GetComponentInChildren<Tilemap>();

        m_Grid = GetComponentInChildren<Grid>();

        m_EmptyCellsList = new List<Vector2Int>();

        m_BoardData = new CellData[Width, Height];

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();

                if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                {
                    tile = WallTiles[Random.Range(0, WallTiles.Length)];
                    m_BoardData[x, y].Passable = false;
                }
                else
                {
                    tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                    m_BoardData[x, y].Passable = true;

                    //this is a passable empty cell, add it to the list!
                    m_EmptyCellsList.Add(new Vector2Int(x, y));
                }

                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
        Player.Spawn(this, new Vector2Int(1, 1));
        m_EmptyCellsList.Remove(new Vector2Int(1, 1));

        Vector2Int endCoord = new Vector2Int(Width - 2, Height - 2);
        AddObject(Instantiate(ExitCellPrefab), endCoord);
        m_EmptyCellsList.Remove(endCoord);

        GenerateWall();
        GenerateFood();
        GenerateEnemies();
        GenerateItem();

    }

    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }

    public CellData GetCellData(Vector2Int cellIndex)
    {
        if (cellIndex.x < 0 || cellIndex.x >= Width
            || cellIndex.y < 0 || cellIndex.y >= Height)
        {
            return null;
        }

        return m_BoardData[cellIndex.x, cellIndex.y];
    }

    void AddObject(CellObject obj, Vector2Int coord)
    {
        CellData data = m_BoardData[coord.x, coord.y];
        obj.transform.position = CellToWorld(coord);
        data.ContainedObject = obj;
        obj.Init(coord);
    }

    void GenerateFood()
    {

        int totalFood = Random.Range(0, 8); // Tổng số Food trên bảng (tối đa 7)
        totalFood = Mathf.Min(totalFood, m_EmptyCellsList.Count); // Không vượt quá số ô trống

        int[] foodCounts = new int[FoodPrefab.Length];

        int totalSpawned = 0;
        while (totalSpawned < totalFood)
        {
            int index = Random.Range(0, FoodPrefab.Length);
            if (foodCounts[index] < 5) // Mỗi loại tối đa 5 lần
            {
                foodCounts[index]++;
                totalSpawned++;
            }
        }

        // Spawn Food trên bảng
        for (int i = 0; i < FoodPrefab.Length; i++)
        {
            for (int j = 0; j < foodCounts[i]; j++)
            {
                if (m_EmptyCellsList.Count == 0) break; // Hết ô trống thì dừng

                int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
                Vector2Int coord = m_EmptyCellsList[randomIndex];

                m_EmptyCellsList.RemoveAt(randomIndex);
                CellData data = m_BoardData[coord.x, coord.y];

                FoodObject newFood = Instantiate(FoodPrefab[i]); // Tạo Food mới từ Prefab

                AddObject(newFood, coord);
            }
        }

    }

    void GenerateWall()
    {
        int wallCount = Random.Range(6, 10);
        for (int i = 0; i < wallCount; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);
            
            WallObject newWall = Instantiate(WallPrefab);
            AddObject(newWall, coord);

          
        }
    }

    void GenerateEnemies()
    {
        int enemyCount = Random.Range(1, 4); // Số lượng Enemy (1–3)

        for (int i = 0; i < enemyCount; ++i)
        {
            if (m_EmptyCellsList.Count == 0)
                break;

            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);

            Enemy enemy = Instantiate(EnemyPrefab[i]);
            AddObject(enemy, coord);
        }
    }

    void GenerateItem()
    {
        if (ItemPrefabs.Length == 0 || m_EmptyCellsList.Count == 0)
            return;

        // Chỉ sinh 1 item, 1 loại ngẫu nhiên
        int itemIndex = Random.Range(0, ItemPrefabs.Length);
        int cellIndex = Random.Range(0, m_EmptyCellsList.Count);

        Vector2Int coord = m_EmptyCellsList[cellIndex];
        m_EmptyCellsList.RemoveAt(cellIndex);

        ItemObject newItem = Instantiate(ItemPrefabs[itemIndex]);
        AddObject(newItem, coord);
    }



    public void Clean()
    {
        //no board data, so exit early, nothing to clean
        if (m_BoardData == null)
            return;


        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                var cellData = m_BoardData[x, y];

                if (cellData.ContainedObject != null)
                {
                    //CAREFUL! Destroy the GameObject NOT just cellData.ContainedObject
                    //Otherwise what you are destroying is the JUST CellObject COMPONENT
                    //and not the whole gameobject with sprite
                    Destroy(cellData.ContainedObject.gameObject);
                }

                SetCellTile(new Vector2Int(x, y), null);
            }
        }
    }
}
