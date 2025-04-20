using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Dictionary<ItemType, float> m_ActiveItems = new Dictionary<ItemType, float>();

    public float MoveSpeed = 5.0f;
    private bool m_IsMoving;
    private Vector3 m_MoveTarget;

    private bool m_IsGameOver;

    private BoardManager m_Board;
    private Vector2Int m_CellPosition;
    public Vector2Int Cell => m_CellPosition;


    private Animator m_Animator;

    public void ResetAnimationToIdle()
    {
        if (m_Animator != null)
        {
            m_Animator.ResetTrigger("Attack");
            m_Animator.ResetTrigger("Hurt");
            m_Animator.SetBool("Moving", false);
            m_Animator.Play("Idle", 0, 0f);
        }
    }

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }

    public void Hurt()
    {
        if (m_Animator != null)
            m_Animator.SetTrigger("Hurt");
        AudioManager.Instance.PlaySfx(SoundType.SFX_PlayerHurt);
    }

    public void GameOver()
    {
        m_IsGameOver = true;
    }

    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        m_Board = boardManager;
        MoveTo(cell);

        //let's move to the right position...
        transform.position = m_Board.CellToWorld(cell);

        if (m_Animator != null)
        {
            m_Animator.Play("Idle", 0, 0f);
        }
    }

    public bool HasItem(ItemType type)
    {
        return m_ActiveItems.ContainsKey(type);
    }


    public void MoveTo(Vector2Int cell)
    {
        m_CellPosition = cell;
        m_IsMoving = true;
        m_MoveTarget = m_Board.CellToWorld(m_CellPosition);

        m_Animator.SetBool("Moving", m_IsMoving);

        

    }

  
    public void CollectItem(ItemType type, float duration)
    {
        if (m_ActiveItems.ContainsKey(type))
        {
            m_ActiveItems[type] += duration;
        }
        else
        {
            m_ActiveItems[type] = duration;
        }
        
        Debug.Log("Collected item: " + type + ", duration: " + m_ActiveItems[type]);
    }


    public void Init()
    {
        m_ActiveItems.Clear();

        m_IsMoving = false;
        m_IsGameOver = false;

        // Reset animation state về Idle
        if (m_Animator != null)
        {
            m_Animator.ResetTrigger("Attack");
            m_Animator.ResetTrigger("Hurt");
            m_Animator.SetBool("Moving", false);

            // Ép vào state "Idle" ngay lập tức
            m_Animator.Play("Idle", 0, 0f);
        }
    }

    public void Attack()
    {
        m_Animator.SetTrigger("Attack");
    }    

    private void Update()
    {
        if (m_IsGameOver)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                GameManager.Instance.StartNewGame();
            }

            return;
        }
            Vector2Int newCellTarget = m_CellPosition;
        bool hasMoved = false;

        if (m_IsMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_MoveTarget, MoveSpeed * Time.deltaTime);

            if (transform.position == m_MoveTarget)
            {
                m_IsMoving = false;
                m_Animator.SetBool("Moving", m_IsMoving);
                var cellData = m_Board.GetCellData(m_CellPosition);
                if (cellData.ContainedObject != null)
                    cellData.ContainedObject.PlayerEntered();
            }
            
        }
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y += 1;
            hasMoved = true;
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y -= 1;
            hasMoved = true;
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x += 1;
            hasMoved = true;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x -= 1;
            hasMoved = true;
        }

        if (hasMoved)
        {
            //check if the new position is passable, then move there if it is.
            BoardManager.CellData cellData = m_Board.GetCellData(newCellTarget);

            if (cellData != null && cellData.Passable)
            {
                AudioManager.Instance.PlaySfx(SoundType.SFX_PlayerMove);

                GameManager.Instance.TurnManager.Tick();
                var keys = new List<ItemType>(m_ActiveItems.Keys);
                foreach (var key in keys)
                {
                    m_ActiveItems[key] -= 1;
                    if (m_ActiveItems[key] <= 0)
                        m_ActiveItems.Remove(key);
                }
                // Nếu có Heart thì không trừ food
                if (!m_ActiveItems.ContainsKey(ItemType.Heart))
                {
                    GameManager.Instance.ChangeFood(-1);
                }

                if (cellData.ContainedObject == null)
                {
                    MoveTo(newCellTarget);
                }
                else
                {
                    // Luôn bật Attack animation khi cố tấn công object
                    m_Animator.SetTrigger("Attack");
                    AudioManager.Instance.PlaySfx(SoundType.SFX_PlayerAttack);


                    // Nếu object cho phép (ví dụ tường bị phá vỡ hoặc enemy chết), thì move tiếp
                    if (cellData.ContainedObject.PlayerWantsToEnter())
                    {
                        MoveTo(newCellTarget);
                        cellData.ContainedObject.PlayerEntered();
                    }
                }
            }
            else
            {
                Attack();
            }    
        }
    }
}
