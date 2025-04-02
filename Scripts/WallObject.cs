using UnityEngine;
using UnityEngine.Tilemaps;

public class WallObject : CellObject
{
    public Tile ObstacleTile;
    public Tile[] DamageTiles;
    public int MaxHealth = 3;

    private int m_HealthPoint;
    private Tile m_OriginalTile;

    

    public override void Init(Vector2Int cell)
    {
        base.Init(cell);
        m_HealthPoint = MaxHealth;

        m_OriginalTile = GameManager.Instance.BoardManager.GetCellTile(cell);
        GameManager.Instance.BoardManager.SetCellTile(cell, DamageTiles[0]);

        Debug.Log($"Wall initialized at {cell} with {m_HealthPoint} HP");
    }

    public override bool PlayerWantsToEnter()
    {
            m_HealthPoint -= 1;

            if (m_HealthPoint > 0)
            {
                GameManager.Instance.BoardManager.SetCellTile(m_Cell, DamageTiles[MaxHealth - m_HealthPoint]); // Cập nhật hình ảnh
                return false;
            }

            GameManager.Instance.BoardManager.SetCellTile(m_Cell, m_OriginalTile);
            Destroy(gameObject);
            return true;
        }
    
}
