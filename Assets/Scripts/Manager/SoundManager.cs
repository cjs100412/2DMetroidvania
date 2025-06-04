using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // EventTrigger�� ����ϱ� ���� �ʿ�

// 1. ���� ������ ���� enum�� �����մϴ�.
public enum SOUND_TYPE
{
    BGM, SFX
}

// ���������� ������ enum (�� ������Ʈ������ �̰� ���)
public enum BGM
{
    Title, InGame, Boss, Shop, GameClear, LastBoss
}

public enum SFX
{
    Attack, Gold, Run, UpgradeAttack, UIPopup, UIExit, GameStart, EnemyDead, EnemyDamaged, BossDamaged,
    GrappleBossDead ,LastBossDead, CheckPoint, Portal, RangeAttack, AoeExplosion, JumpAttack, FarRange, PullAndSpray, Beam, Item, Hiddenplace
}

[Serializable]
public class BGMClip
{
    public BGM type;
    public AudioClip clip;
}

[Serializable]
public class SFXClip
{
    public SFX type;
    public AudioClip clip;
}

public class SoundManager : MonoBehaviour
{
    // 2. Ŭ������ �ʿ��� �ʵ� �� ����
    [Header("����� �ͼ�")]
    public AudioMixer audioMixer;
    public string bgmParameter = "BGM"; // ����� �ͼ��� ������ �̸�
    public string sfxParameter = "SFX";

    [Header("����� �ҽ�")]
    public AudioSource bgm;
    public AudioSource sfx;

    [Header("����� Ŭ��")]
    public List<BGMClip> bgm_list;
    public List<SFXClip> sfx_list;

    private Dictionary<BGM, AudioClip> bgm_dict; // BGM ������ ���� ����� Ŭ��
    private Dictionary<SFX, AudioClip> sfx_dict; // SFX ������ ���� ����� Ŭ��

    private float bgm_value;
    private float sfx_value;

    [Header("UI �����̴� & ���")]
    public Slider bgm_slider;
    public Slider sfx_slider;
    public Toggle bgm_toggle;
    public Toggle sfx_toggle;

    // 3. ���� �Ŵ����� ��ü ���ӿ��� 1���� �ʿ��ϴ�.(�̱���)
    public static SoundManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // ��ųʸ� ����
            bgm_dict = new Dictionary<BGM, AudioClip>();
            sfx_dict = new Dictionary<SFX, AudioClip>();

            // ���� ���� ���(BGM)
            foreach (var bgmItem in bgm_list)
            {
                if (!bgm_dict.ContainsKey(bgmItem.type))
                    bgm_dict[bgmItem.type] = bgmItem.clip;
            }
            // ���� ���� ���(SFX)
            foreach (var sfxItem in sfx_list)
            {
                if (!sfx_dict.ContainsKey(sfxItem.type))
                    sfx_dict[sfxItem.type] = sfxItem.clip;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // ���� �ε�� ������ OnSceneLoaded ȣ��
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // 1) ���� �� �����̴��� Inspector�� ������ ��(���� 0.0001 ~ 1)�� �о �ͼ� �������� ����
        if (bgm_slider != null)
        {
            bgm_value = Mathf.Log10(Mathf.Max(bgm_slider.value, 0.0001f)) * 20f;
            audioMixer.SetFloat(bgmParameter, bgm_value);
            // BGM �����̴� ���� �ٲ� ������ SetBGMVolume�� ȣ��ǵ��� ����
            bgm_slider.onValueChanged.AddListener(SetBGMVolume);
        }

        if (sfx_slider != null)
        {
            sfx_value = Mathf.Log10(Mathf.Max(sfx_slider.value, 0.0001f)) * 20f;
            audioMixer.SetFloat(sfxParameter, sfx_value);
            // SFX �����̴� ���� �ٲ� ������ SetSFXVolume�� ȣ��ǵ��� ����
            sfx_slider.onValueChanged.AddListener(SetSFXVolume);

            // 2) sfx_slider�� Pointer Up �̺�Ʈ(���콺 ���� ����)�� �ɾ
            //    �׶� ���� ȿ������ �� ���� ����ϵ��� ����
            AttachPointerUpEventToSlider(sfx_slider, () =>
            {
                // ���콺 ��ư�� ���� ���� ȣ��� �ݹ�
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(SFX.Attack);
                }
            });
        }
    }

    // ���� �ε�� �� ȣ��Ǵ� �ݹ�
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "TitleScene":
                PlayBGM(BGM.Title);
                break;

            case "StartScene":
            case "ZoneA":
            case "ZoneB":
                PlayBGM(BGM.InGame);
                break;

            case "BossScene":
                PlayBGM(BGM.LastBoss);
                break;

            case "DashBossScene":
            case "DoubleJumpBossScene":
            case "GrappleBossScene":
                PlayBGM(BGM.Boss);
                break;

            case "ShopScene":
                PlayBGM(BGM.Shop);
                break;

            case "GameClearScene":
                PlayBGM(BGM.GameClear);
                break;

            default:
                PlayBGM(BGM.InGame);
                break;
        }
    }

    // BGM ���
    public void PlayBGM(BGM bgm_type)
    {
        if (bgm_dict.TryGetValue(bgm_type, out var clip))
        {
            // �̹� ���� Ŭ���� Ʋ�� ������ ����
            if (bgm.clip == clip)
                return;

            bgm.clip = clip;
            bgm.loop = true;
            bgm.Play();
        }
    }

    // SFX ���
    public void PlaySFX(SFX sfx_type)
    {
        if (sfx_dict.TryGetValue(sfx_type, out var clip))
        {
            sfx.PlayOneShot(clip);
        }
    }

    // 2. UI�� Slider�� onValueChanged �̺�Ʈ �ʿ��� ������ �Լ� ����
    // Audio Mixer�� ���� ������ 0 db ~ -80 db����(0.0001 ~ 1 ������ ����)�� �����Ǿ� �ֽ��ϴ�.
    public void SetBGMVolume(float volume)
    {
        // ����� ���� ���·� ���� ����
        if (bgm_toggle != null) bgm_toggle.isOn = false;

        // volume�� 0�̸� ���Ұ�(-80db)�� Ŭ����
        float v = Mathf.Max(volume, 0.0001f);
        bgm_value = Mathf.Log10(v) * 20f;
        audioMixer.SetFloat(bgmParameter, bgm_value);
    }

    public void SetSFXVolume(float volume)
    {
        // ����� ���� ���·� ���� ����
        if (sfx_toggle != null) sfx_toggle.isOn = false;

        // volume�� 0�̸� ���Ұ�(-80db)�� Ŭ����
        float v = Mathf.Max(volume, 0.0001f);
        sfx_value = Mathf.Log10(v) * 20f;
        audioMixer.SetFloat(sfxParameter, sfx_value);

        // �� Input.GetMouseButtonUp(0) �� üũ���� ����.
        //    ��� �����̴� ��ü�� PointerUp �̺�Ʈ���� PlaySFX�� ȣ���մϴ�.
    }

    // Mute ���: toggle�� üũ ���¿� ���� 0dB Ȥ�� -80dB (��ǻ� ���Ұ�) ����
    public void MuteBGM(bool mute)
    {
        audioMixer.SetFloat(bgmParameter, mute ? -80f : bgm_value);
    }

    public void MuteSFX(bool mute)
    {
        audioMixer.SetFloat(sfxParameter, mute ? -80f : sfx_value);
    }

    // ---------------------------------------------------------------------------------
    // �����̴�(GameObject)�� EventTrigger�� ���̰�, PointerUp �̺�Ʈ�� �߰��ϴ� ���� �޼���
    private void AttachPointerUpEventToSlider(Slider slider, Action onPointerUpCallback)
    {
        if (slider == null || onPointerUpCallback == null)
            return;

        // 1) Slider GameObject�� EventTrigger ������Ʈ�� �ִ��� Ȯ���ϰ�, ������ �߰�
        EventTrigger trigger = slider.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = slider.gameObject.AddComponent<EventTrigger>();
        }

        // 2) PointerUp�� Entry�� ����
        var entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };

        // 3) �̺�Ʈ�� �߻����� �� ȣ��� �ݹ� ���
        entry.callback.AddListener((data) =>
        {
            onPointerUpCallback.Invoke();
        });

        // 4) Ʈ���� ����Ʈ�� �߰�
        trigger.triggers.Add(entry);
    }
}
