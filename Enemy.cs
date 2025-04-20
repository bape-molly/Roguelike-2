using UnityEngine;
using UnityEditor.U2D.Aseprite;
using System.Runtime.InteropServices.WindowsRuntime;

public class Enemy : CellObject
{
    // Số máu tối đa của enemy.
    public int Health = 3;

    // Số máu hiện tại (được khởi tạo khi Init).
    private int m_CurrentHealth;

    [Header("Animation State Names")]
    [Tooltip("Enemy1-Idle, Enemy2-Idle, Enemy3-Idle")]
    [SerializeField] private string _idleStateName = "EnemyIdle";

    private Animator m_Animator;

    // Smooth movement fields
    private bool m_IsMoving = false;
    private Vector3 m_MoveTarget;
    public float MoveSpeed = 3f;

    private bool m_GotAttackedThisTurn = false;

    // Đăng ký sự kiện Turn từ TurnManager.
    private void Awake()
    {
        m_Animator = GetComponent<Animator>();

        // Đảm bảo GameManager và TurnManager không null trước khi đăng ký.
        if (GameManager.Instance != null && GameManager.Instance.TurnManager != null)
        {
            GameManager.Instance.TurnManager.OnTick += TurnHappened;
        }
    }

    // Hủy đăng ký sự kiện khi enemy bị phá hủy.
    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.TurnManager != null)
        {
            GameManager.Instance.TurnManager.OnTick -= TurnHappened;
        }
    }

    // Khởi tạo enemy: lưu vị trí ban đầu và đặt lại máu.
    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        m_CurrentHealth = Health;

        if (m_Animator != null)
        {
            
            // Reset mọi trigger/bool
            m_Animator.ResetTrigger("Attack");
            m_Animator.ResetTrigger("Hurt");
            m_Animator.SetBool("Moving", m_IsMoving);

            // Chạy state Idle đúng theo tên bạn đã cấu hình
            m_Animator.Play(_idleStateName, 0, 0f);
        }
    }

    // Khi Player muốn đi vào ô chứa enemy, enemy nhận sát thương.
    // Trả về false vì enemy không cho phép người chơi đi vào ô đó.

    void Update()
    {
        if (m_IsMoving)
        {
            // Di chuyển mượt về target
            transform.position = Vector3.MoveTowards(transform.position, m_MoveTarget, MoveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, m_MoveTarget) < 0.001f)
            {
                m_IsMoving = false;
                if (m_Animator != null) m_Animator.SetBool("Moving", m_IsMoving);
            }
        }
    }

    private bool isDead = false;
    public override bool PlayerWantsToEnter()
    {
        

        int damage = GameManager.Instance.PlayerController.HasItem(ItemType.Sword) ? 2 : 1;
        m_CurrentHealth -= damage;

        if (m_Animator != null)
        {
            m_Animator.SetTrigger("Hurt");
            AudioManager.Instance.PlaySfx(SoundType.SFX_EnemyHurt);

        }

        m_GotAttackedThisTurn = true;
        if (m_CurrentHealth <= 0)
        {
            isDead = true;
            Destroy(gameObject);
        }
        return false;
    }

    // Di chuyển enemy đến ô target nếu ô đó khả dụng.
    private bool MoveTo(Vector2Int coord)
    {
        var board = GameManager.Instance.BoardManager;
        var targetCell = board.GetCellData(coord);

        // Kiểm tra ô đích: phải tồn tại, passable và không chứa đối tượng nào.
        if (targetCell == null || !targetCell.Passable || targetCell.ContainedObject != null)
        {
            return false;
        }

        // Cập nhật CellData
        board.GetCellData(m_Cell).ContainedObject = null;
        targetCell.ContainedObject = this;
        m_Cell = coord;

        // Bắt đầu smooth movement (không teleport nữa)
        m_MoveTarget = board.CellToWorld(coord);
        m_IsMoving = true;

        if (m_Animator != null)
            m_Animator.SetBool("Moving", m_IsMoving);

        return true;
    }

    // Hàm được gọi mỗi khi TurnManager tick xảy ra.
    void TurnHappened()
    {
        if (isDead) return;
        if (m_GotAttackedThisTurn)
        {
            m_GotAttackedThisTurn = false; // Reset flag cho lượt sau
            return; // Bỏ lượt tấn công lần này
        }

        // Nếu đang di chuyển, chờ đến khi xong (hoặc bạn có thể skip lượt)
        if (m_IsMoving) return;

        // Lấy vị trí của Player từ PlayerController (giả sử có public property Cell).
        Vector2Int playerCell = GameManager.Instance.PlayerController.Cell;

        // Tính khoảng cách theo trục X và Y.
        int xDist = playerCell.x - m_Cell.x;
        int yDist = playerCell.y - m_Cell.y;
        int absXDist = Mathf.Abs(xDist);
        int absYDist = Mathf.Abs(yDist);

        // Nếu enemy kề sát Player (chỉ cách 1 ô theo X hoặc Y) thì tấn công.
        if ((xDist == 0 && absYDist == 1) || (yDist == 0 && absXDist == 1))
        {
            m_Animator.SetTrigger("Attack");
            AudioManager.Instance.PlaySfx(SoundType.SFX_EnemyAttack);

            if (GameManager.Instance.PlayerController.HasItem(ItemType.Shield))
            {
                Debug.Log("Shield blocked attack");
                return;
            }

            // Ví dụ: tấn công giảm Food của Player (hoặc có thể thay bằng logic sát thương Player).
            GameManager.Instance.ChangeFood(-Health);

            GameManager.Instance.PlayerController.Hurt();

        }
        else
        {
            // Nếu chưa kề sát, enemy sẽ di chuyển gần Player.
            // Ưu tiên di chuyển theo trục có khoảng cách lớn hơn.
            if (absXDist > absYDist)
            {
                if (!TryMoveInX(xDist))
                {
                    // Nếu di chuyển theo X thất bại, thử di chuyển theo Y.
                    TryMoveInY(yDist);
                }
            }
            else
            {
                if (!TryMoveInY(yDist))
                {
                    // Nếu di chuyển theo Y thất bại, thử di chuyển theo X.
                    TryMoveInX(xDist);
                }
            }
        }

        if (m_Animator != null)
        {
            m_Animator.SetBool("Moving", m_IsMoving);
        }
    }

    // Thử di chuyển theo trục X: nếu xDist dương thì sang phải, ngược lại sang trái.
    bool TryMoveInX(int xDist)
    {
        if (xDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.right);
        }
        else if (xDist < 0)
        {
            return MoveTo(m_Cell + Vector2Int.left);
        }
        return false;
    }

    // Thử di chuyển theo trục Y: nếu yDist dương thì lên trên, ngược lại xuống dưới.
    bool TryMoveInY(int yDist)
    {
        if (yDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.up);
        }
        else if (yDist < 0)
        {
            return MoveTo(m_Cell + Vector2Int.down);
        }
        return false;
    }
}
