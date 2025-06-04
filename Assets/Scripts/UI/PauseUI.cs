using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    [Header("Pause UI 에 배치된 슬라이더/토글")]
    public Slider bgmSlider;
    public Toggle bgmToggle;
    public Slider sfxSlider;
    public Toggle sfxToggle;

    void Start()
    {
        // 1) SoundManager 싱글톤 인스턴스를 가져온다
        var sm = SoundManager.Instance;
        if (sm == null)
        {
            Debug.LogWarning("PauseUI: SoundManager.Instance가 null 입니다. SoundManager가 씬에 존재하는지 확인해 주세요.");
            return;
        }

        // 2) PauseUI 슬라이더/토글에 초기값 세팅
        //    (SoundManager 쪽에 이미 연결된 bgm_slider/sfx_slider 값이 남아 있으므로 복사해서 사용)
        if (sm.bgm_slider != null)
            bgmSlider.value = sm.bgm_slider.value;
        if (sm.bgm_toggle != null)
            bgmToggle.isOn = sm.bgm_toggle.isOn;

        if (sm.sfx_slider != null)
            sfxSlider.value = sm.sfx_slider.value;
        if (sm.sfx_toggle != null)
            sfxToggle.isOn = sm.sfx_toggle.isOn;

        // 3) PauseUI 슬라이더/토글에 이벤트 리스너 연결
        //    → 값이 바뀔 때마다 SoundManager의 SetBGMVolume/SetSFXVolume, Mute 함수를 호출
        bgmSlider.onValueChanged.AddListener(sm.SetBGMVolume);
        bgmToggle.onValueChanged.AddListener(sm.MuteBGM);

        sfxSlider.onValueChanged.AddListener(sm.SetSFXVolume);
        sfxToggle.onValueChanged.AddListener(sm.MuteSFX);

        bgmSlider.onValueChanged.AddListener(_ => { if (bgmToggle != null) bgmToggle.isOn = false; });
        sfxSlider.onValueChanged.AddListener(_ => { if (sfxToggle != null) sfxToggle.isOn = false; });
        AttachPointerUpEventToSlider(sfxSlider, () =>
        {
            // 마우스 버튼을 떼는 순간 호출될 콜백
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFX.Attack);
            }
        });
    }

    void OnDestroy()
    {
        // 씬 전환이나 UI 닫힐 때 이벤트 중복 방지를 위해 RemoveListener
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

        // 1) Slider GameObject에 EventTrigger 컴포넌트가 있는지 확인하고, 없으면 추가
        EventTrigger trigger = slider.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = slider.gameObject.AddComponent<EventTrigger>();
        }

        // 2) PointerUp용 Entry를 생성
        var entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };

        // 3) 이벤트가 발생했을 때 호출될 콜백 등록
        entry.callback.AddListener((data) =>
        {
            onPointerUpCallback.Invoke();
        });

        // 4) 트리거 리스트에 추가
        trigger.triggers.Add(entry);
    }
}
