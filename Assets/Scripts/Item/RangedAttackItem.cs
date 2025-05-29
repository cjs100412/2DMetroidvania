using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider2D))]
public class RangedAttackItem : MonoBehaviour
{
    [Header("획득 이펙트")]
    public ParticleSystem pickupEffect;

    CinemachineCamera cinemachineCamera;

    [Header("카메라 줌인")]
    public float zoomFactor = 0.6f;
    public float zoomDuration = 0.5f;

    [Header("슬로우 모션")]
    public float slowTimeScale = 0.3f;
    public float slowDuration = 1.5f;

    private float originalOrthoSize;
    private Collider2D col;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        cinemachineCamera = GameObject.FindWithTag("Cinemachine").GetComponent<CinemachineCamera>();

        if (cinemachineCamera != null)
            originalOrthoSize = cinemachineCamera.Lens.OrthographicSize;
        else
            Debug.LogError("Cinemachine Camera가 할당되지 않았습니다.");

        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            // 원거리 공격 해금
            player.UnlockRangedAttack();

            // 이펙트 재생
            if (pickupEffect != null)
                Instantiate(pickupEffect, transform.position, Quaternion.identity);

            // 아이템 비가시화 및 콜라이더 비활성화
            col.enabled = false;
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;

            // 카메라 줌 및 슬로우 모션 시작
            StartCoroutine(DoCameraZoom());
            StartCoroutine(DoSlowMotion());

            // 모든 효과가 끝난 후 아이템 제거
            float totalDuration = zoomDuration * 2 + slowDuration + 0.1f;
            Destroy(gameObject, totalDuration);
        }
    }

    IEnumerator DoCameraZoom()
    {
        if (cinemachineCamera == null)
            yield break;

        float targetSize = originalOrthoSize * zoomFactor;
        float elapsed = 0f;

        // 줌 인
        while (elapsed < zoomDuration)
        {
            float t = elapsed / zoomDuration;
            cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(originalOrthoSize, targetSize, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        cinemachineCamera.Lens.OrthographicSize = targetSize;

        // 슬로우 모션 동안 유지
        yield return new WaitForSecondsRealtime(slowDuration);

        // 줌 아웃
        elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            float t = elapsed / zoomDuration;
            cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(targetSize, originalOrthoSize, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        cinemachineCamera.Lens.OrthographicSize = originalOrthoSize;
    }

    IEnumerator DoSlowMotion()
    {
        // 슬로우 모션 시작
        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = 0.02f * slowTimeScale;

        // 실제 시간으로 기다림
        yield return new WaitForSecondsRealtime(slowDuration);

        // 시간 복구
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}
