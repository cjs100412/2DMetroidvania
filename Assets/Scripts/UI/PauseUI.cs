using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    [Header("Pause UI �� ��ġ�� �����̴�/���")]
    public Slider bgmSlider;
    public Toggle bgmToggle;
    public Slider sfxSlider;
    public Toggle sfxToggle;

    void Start()
    {
        // 1) SoundManager �̱��� �ν��Ͻ��� �����´�
        var sm = SoundManager.Instance;
        if (sm == null)
        {
            Debug.LogWarning("PauseUI: SoundManager.Instance�� null �Դϴ�. SoundManager�� ���� �����ϴ��� Ȯ���� �ּ���.");
            return;
        }

        // 2) PauseUI �����̴�/��ۿ� �ʱⰪ ����
        //    (SoundManager �ʿ� �̹� ����� bgm_slider/sfx_slider ���� ���� �����Ƿ� �����ؼ� ���)
        if (sm.bgm_slider != null)
            bgmSlider.value = sm.bgm_slider.value;
        if (sm.bgm_toggle != null)
            bgmToggle.isOn = sm.bgm_toggle.isOn;

        if (sm.sfx_slider != null)
            sfxSlider.value = sm.sfx_slider.value;
        if (sm.sfx_toggle != null)
            sfxToggle.isOn = sm.sfx_toggle.isOn;

        // 3) PauseUI �����̴�/��ۿ� �̺�Ʈ ������ ����
        //    �� ���� �ٲ� ������ SoundManager�� SetBGMVolume/SetSFXVolume, Mute �Լ��� ȣ��
        bgmSlider.onValueChanged.AddListener(sm.SetBGMVolume);
        bgmToggle.onValueChanged.AddListener(sm.MuteBGM);

        sfxSlider.onValueChanged.AddListener(sm.SetSFXVolume);
        sfxToggle.onValueChanged.AddListener(sm.MuteSFX);

        bgmSlider.onValueChanged.AddListener(_ => { if (bgmToggle != null) bgmToggle.isOn = false; });
        sfxSlider.onValueChanged.AddListener(_ => { if (sfxToggle != null) sfxToggle.isOn = false; });
        AttachPointerUpEventToSlider(sfxSlider, () =>
        {
            // ���콺 ��ư�� ���� ���� ȣ��� �ݹ�
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFX.Attack);
            }
        });
    }

    void OnDestroy()
    {
        // �� ��ȯ�̳� UI ���� �� �̺�Ʈ �ߺ� ������ ���� RemoveListener
        var sm = SoundManager.Instance;
        if (sm != null)
        {
            bgmSlider.onValueChanged.RemoveListener(sm.SetBGMVolume);
            bgmToggle.onValueChanged.RemoveListener(sm.MuteBGM);
            sfxSlider.onValueChanged.RemoveListener(sm.SetSFXVolume);
            sfxToggle.onValueChanged.RemoveListener(sm.MuteSFX);
        }
        bgmSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();
    }

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
