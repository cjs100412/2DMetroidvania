using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//1. ���� ������ ���� enum�� �����մϴ�.
public enum SOUND_TYPE //������ ���� enum
{
    BGM, SFX
}
//���������� ������ enum (�� ������Ʈ������ �̰� ���)
public enum BGM
{
    Title, InGame, Boss, Shop, GameClear
}
public enum SFX
{
    Attack, Gold
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
    //2. Ŭ������ �ʿ��� �ʵ� �� ����
    [Header("����� �ͼ�")]
    public AudioMixer audioMixer;
    public string bgmParameter = "BGM"; //����� �ͼ��� ������ �̸�
    public string sfxParameter = "SFX";

    [Header("����� �ҽ�")]
    public AudioSource bgm;
    public AudioSource sfx;

    [Header("����� Ŭ��")]
    public List<BGMClip> bgm_list;
    public List<SFXClip> sfx_list;

    private Dictionary<BGM, AudioClip> bgm_dict; //BGM ������ ���� ����� Ŭ��
    private Dictionary<SFX, AudioClip> sfx_dict; //SFX ������ ���� ����� Ŭ�� 

    private float bgm_value;
    private float sfx_value;

    public Slider bgm_slider;
    public Slider sfx_slider;

    public Toggle bgm_toggle;
    public Toggle sfx_toggle;

    //3. ���� �Ŵ����� ��ü ���ӿ��� 1���� �ʿ��ϴ�.(�̱���)
    //������Ƽ ���·� ������ �ν��Ͻ�
    public static SoundManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            //��ųʸ� ����
            bgm_dict = new Dictionary<BGM, AudioClip>();
            sfx_dict = new Dictionary<SFX, AudioClip>();

            //���� ���� ���(BGM)
            foreach (var bgm in bgm_list)
            {
                bgm_dict[bgm.type] = bgm.clip;
            }
            //���� ���� ���(SFX)
            foreach (var sfx in sfx_list)
            {
                sfx_dict[sfx.type] = sfx.clip;
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
        bgm_value = Mathf.Log10(bgm_slider.value) * 20;
        audioMixer.SetFloat(bgmParameter, bgm_value);
        sfx_value = Mathf.Log10(sfx_slider.value) * 20;
        audioMixer.SetFloat(sfxParameter, sfx_value);
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

    //1. UI�� BGM, SFX Ʈ�� ��� ����

    //2. UI�� Slider�� onValueChanged �̺�Ʈ �ʿ��� ������ �Լ� ����
    public void PlayBGM(BGM bgm_type)
    {
        //bgm ��ųʸ� ��ܿ� �ش� BGM�� �����Ѵٸ� �÷��̸� �����մϴ�.
        //C# �Ű����� ������ out
        //Ÿ�� �տ� �ٽ��ϴ�. ex) void Function(out int value);
        //out �����ڰ� ���� �Ű������� ������ ������ �˴ϴ�.
        //�Լ� ���ο��� ������ ������ ���� ��������� �մϴ�.
        if (bgm_dict.TryGetValue(bgm_type, out var clip))
        {
            //���� Ŭ���� �����ϴٸ�, ��������� Ʋ������ �ִٶ�� �ؼ��� �� ����.
            if (bgm.clip == clip)
            {
                return;
            }
            bgm.clip = clip;
            bgm.loop = true;
            bgm.Play();
        }
    }

    public void PlaySFX(SFX sfx_type)
    {
        //sfx ��ųʸ� ��ܿ� �ش� SFX�� �����Ѵٸ� bgm�� 1�� �����մϴ�.
        if (sfx_dict.TryGetValue(sfx_type, out var clip))
        {
            //ȿ������ �Ͻ����� �÷��̸� �����մϴ�.
            sfx.PlayOneShot(clip);
        }
    }

    //Audio Mixer�� ���� ������ 0 db ~ - 80 d������ �����Ǿ��ֽ��ϴ�.
    public void SetBGMVolume(float volume)
    {
        bgm_toggle.isOn = true;
        bgm_value = Mathf.Log10(volume) * 20;
        audioMixer.SetFloat(bgmParameter, bgm_value);
        //�����̴� UI �ּ� ���� 0.0001�� �ش� ��ġ�� ����ϸ� -80
        //�ִ� �� 1�� ��� 0���� ���˴ϴ�.
    }

    public void SetSFXVolume(float volume)
    {
        sfx_toggle.isOn = true;
        sfx_value = Mathf.Log10(volume) * 20;
        audioMixer.SetFloat(sfxParameter, sfx_value);
    }

    //������ ������ �Ǵ� MuteBGM�� MuteSFX�� �������ּ���.
    //��Ʈ : -80 ��ġ�� mute
    //UI �߿����� Toggle
    public void MuteBGM(bool mute)
    {
        //Toggle Ű�� üũ�ߴٴ� ������ § �ڵ�
        //���� ����
        //���� ? T : F �� �ۼ��Ǹ� ������ ������ T�� �ִ� ����, �ƴϸ� F�� �ִ� ���� ó���մϴ�.
        audioMixer.SetFloat(bgmParameter, mute ? bgm_value : -80.0f);
    }

    public void MuteSFX(bool mute)
    {
        //Toggle Ű�� üũ�ߴٴ� ������ § �ڵ�
        //���� ����
        //���� ? T : F �� �ۼ��Ǹ� ������ ������ T�� �ִ� ����, �ƴϸ� F�� �ִ� ���� ó���մϴ�.
        audioMixer.SetFloat(sfxParameter, mute ? sfx_value : -80.0f);
    }

}