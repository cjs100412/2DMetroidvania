using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider2D))]
public class DoubleJumpItem : MonoBehaviour
{
    [Header("������ ���� ID (��: DoubleJump)")]
    public string itemID = "DoubleJump";

    [Header("ȹ�� ����Ʈ")]
    public ParticleSystem pickupEffect;

    private CinemachineCamera cinemachineCamera;

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
        // 1) �̹� ȹ���ߴ��� Ȯ��
        if (GameManager.I != null && GameManager.I.IsItemCollected(itemID))
        {
            Destroy(gameObject);
            return;
        }

        // 2) ī�޶� �� ������Ʈ �ʱ�ȭ
        cinemachineCamera = GameObject.FindWithTag("Cinemachine")
                              .GetComponent<CinemachineCamera>();
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
            // 1) ������ ���� ���
            if (GameManager.I != null)
                GameManager.I.SetItemCollected(itemID);

            // 2) ���� ���� ���
            player.UnlockDoubleJump();

            // 3) ����Ʈ ���
            if (pickupEffect != null)
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFX.Item);
            }
            // 4) ������ �񰡽�ȭ �� �ݶ��̴� ��Ȱ��ȭ
            col.enabled = false;
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;

            // 5) ī�޶� �� & ���ο� ��� ����
            StartCoroutine(DoCameraZoom());
            StartCoroutine(DoSlowMotion());

            // 6) ��� ȿ���� ���� �� ������ ����
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
            cinemachineCamera.Lens.OrthographicSize =
                Mathf.Lerp(originalOrthoSize, targetSize, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        cinemachineCamera.Lens.OrthographicSize = targetSize;

        // ���ο� ��� ����
        yield return new WaitForSecondsRealtime(slowDuration);

        // �� �ƿ�
        elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            float t = elapsed / zoomDuration;
            cinemachineCamera.Lens.OrthographicSize =
                Mathf.Lerp(targetSize, originalOrthoSize, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        cinemachineCamera.Lens.OrthographicSize = originalOrthoSize;
    }

    IEnumerator DoSlowMotion()
    {
        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = 0.02f * slowTimeScale;

        yield return new WaitForSecondsRealtime(slowDuration);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}
