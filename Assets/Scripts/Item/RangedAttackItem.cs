using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider2D))]
public class RangedAttackItem : MonoBehaviour
{
    [Header("ȹ�� ����Ʈ")]
    public ParticleSystem pickupEffect;

    CinemachineCamera cinemachineCamera;

    [Header("ī�޶� ����")]
    public float zoomFactor = 0.6f;
    public float zoomDuration = 0.5f;

    [Header("���ο� ���")]
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
            Debug.LogError("Cinemachine Camera�� �Ҵ���� �ʾҽ��ϴ�.");

        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            // ���Ÿ� ���� �ر�
            player.UnlockRangedAttack();

            // ����Ʈ ���
            if (pickupEffect != null)
                Instantiate(pickupEffect, transform.position, Quaternion.identity);

            // ������ �񰡽�ȭ �� �ݶ��̴� ��Ȱ��ȭ
            col.enabled = false;
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;

            // ī�޶� �� �� ���ο� ��� ����
            StartCoroutine(DoCameraZoom());
            StartCoroutine(DoSlowMotion());

            // ��� ȿ���� ���� �� ������ ����
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

        // �� ��
        while (elapsed < zoomDuration)
        {
            float t = elapsed / zoomDuration;
            cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(originalOrthoSize, targetSize, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        cinemachineCamera.Lens.OrthographicSize = targetSize;

        // ���ο� ��� ���� ����
        yield return new WaitForSecondsRealtime(slowDuration);

        // �� �ƿ�
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
        // ���ο� ��� ����
        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = 0.02f * slowTimeScale;

        // ���� �ð����� ��ٸ�
        yield return new WaitForSecondsRealtime(slowDuration);

        // �ð� ����
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}
