using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource MusicSource;
    public AudioSource SfxSource;

    [Header("Audio Clips")]
    public AudioClip BGM_Menu;
    public AudioClip BGM_Gameplay;
    public AudioClip SFX_PlayerMove;
    public AudioClip SFX_PlayerAttack;
    public AudioClip SFX_PickFood;
    public AudioClip SFX_PickItem;
    public AudioClip SFX_PlayerHurt;
    public AudioClip SFX_EnemyAttack;
    public AudioClip SFX_EnemyHurt;
    public AudioClip SFX_GameOver;
    public AudioClip SFX_LevelUp;

    private Dictionary<SoundType, AudioClip> _clips;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _clips = new Dictionary<SoundType, AudioClip>
            {
                { SoundType.BGM_Menu, BGM_Menu },
                { SoundType.BGM_Gameplay, BGM_Gameplay },
                { SoundType.SFX_PlayerMove, SFX_PlayerMove },
                { SoundType.SFX_PlayerAttack, SFX_PlayerAttack },
                { SoundType.SFX_PickFood, SFX_PickFood },
                { SoundType.SFX_PickItem, SFX_PickItem },
                { SoundType.SFX_PlayerHurt, SFX_PlayerHurt },
                { SoundType.SFX_EnemyAttack, SFX_EnemyAttack },
                { SoundType.SFX_EnemyHurt, SFX_EnemyHurt },
                { SoundType.SFX_GameOver, SFX_GameOver },
                { SoundType.SFX_LevelUp, SFX_LevelUp }
            };
        }
        else Destroy(gameObject);
    }

    // Nhạc nền loop liên tục
    public void PlayMusic(SoundType bgm)
    {
        if (_clips.TryGetValue(bgm, out var clip) && MusicSource.clip != clip)
        {
            MusicSource.clip = clip;
            MusicSource.loop = true;
            MusicSource.Play();
        }
    }

    // Phát sound effect one shot
    public void PlaySfx(SoundType sfx)
    {
        if (_clips.TryGetValue(sfx, out var clip))
            SfxSource.PlayOneShot(clip);
    }
}
