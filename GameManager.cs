using UnityEngine;
using Unity.UIElements;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public BoardManager BoardManager;
    public PlayerController PlayerController;

    public TurnManager TurnManager { get; private set; }

    private int m_FoodAmount = 100;

    private VisualElement m_GameOverPanel;
    private Label m_GameOverMessage;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public UIDocument UIDoc;
    private Label m_FoodLabel;

    private int m_CurrentLevel = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Main")
        {
            TurnManager = new TurnManager();
            TurnManager.OnTick += OnTurnHappen;

            m_FoodLabel = UIDoc.rootVisualElement.Q<Label>("FoodLabel");

            m_GameOverPanel = UIDoc.rootVisualElement.Q<VisualElement>("GameOverPanel");
            m_GameOverMessage = m_GameOverPanel.Q<Label>("GameOverMessage");

            // Bắt sự kiện các nút Game Over
            var retryButton = UIDoc.rootVisualElement.Q<Button>("RetryButton");
            var menuButton = UIDoc.rootVisualElement.Q<Button>("MenuButton");

            retryButton.clicked += () =>
            {
                StartNewGame();
                m_GameOverPanel.style.visibility = Visibility.Hidden;
            };

            menuButton.clicked += () =>
            {
                SceneManager.LoadScene("Menu");
            };


            StartNewGame();
        }    
    }

    public void NewLevel()
    {
        AudioManager.Instance.PlaySfx(SoundType.SFX_LevelUp);

        BoardManager.Clean();
        BoardManager.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));

        m_CurrentLevel++;

        if (PlayerController != null)
        {
            PlayerController.ResetAnimationToIdle();
        }
    }

    public void StartNewGame()
    {
        AudioManager.Instance.PlayMusic(SoundType.BGM_Gameplay);
        

        m_GameOverPanel.style.visibility = Visibility.Hidden;

        m_CurrentLevel = 1;
        m_FoodAmount = 100;
        m_FoodLabel.text = "Food : " + m_FoodAmount;

        BoardManager.Clean();
        BoardManager.Init();

        PlayerController.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));
    }

    void OnTurnHappen()
    {
        ChangeFood(-1);
    }
    public void ChangeFood(int amount)
    {
        m_FoodAmount += amount;
        m_FoodLabel.text = "Food : " + m_FoodAmount;

        if (m_FoodAmount <= 0)
        {
            AudioManager.Instance.PlaySfx(SoundType.SFX_GameOver);

            PlayerController.GameOver();
            m_GameOverPanel.style.visibility = Visibility.Visible;
            m_GameOverMessage.text = "\n\nGame Over!\n" + m_CurrentLevel + " levels";

        }
    }

    // Update is called once per frame
}
