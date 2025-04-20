using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject menuPanel;
    public GameObject optionsPanel;
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Button playButton, optionsButton, quitButton, backButton;

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 0.2f;

    void Start()
    {
        // Bind sự kiện:
        playButton.onClick.AddListener(OnPlay);
        optionsButton.onClick.AddListener(() => SwitchPanel(optionsPanel));
        quitButton.onClick.AddListener(OnQuit);
        backButton.onClick.AddListener(() => SwitchPanel(menuPanel));

        // Khởi tạo giá trị sliders từ AudioManager:
        bgmSlider.value = AudioManager.Instance.MusicSource.volume;
        sfxSlider.value = AudioManager.Instance.SfxSource.volume;
        bgmSlider.onValueChanged.AddListener(v => AudioManager.Instance.MusicSource.volume = v);
        sfxSlider.onValueChanged.AddListener(v => AudioManager.Instance.SfxSource.volume = v);

        // Start nhạc menu:
        AudioManager.Instance.PlayMusic(SoundType.BGM_Menu);

        // Đảm bảo panels:
        menuPanel.SetActive(true);
        optionsPanel.SetActive(false);
    }

    void SwitchPanel(GameObject panelToShow)
    {
        menuPanel.SetActive(panelToShow == menuPanel);
        optionsPanel.SetActive(panelToShow == optionsPanel);
    }

    public void OnPlay()
    {
        StartCoroutine(FadeAndLoad("Main"));
    }

    public void OnQuit()
    {
        Application.Quit();
    }

    public IEnumerator FadeAndLoad(string Main)
    {
        // Fade to black
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.SmoothStep(0, 1, t / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, a);
            yield return null;
        }
        // Load scene
        SceneManager.LoadScene("Main");


    }
}
