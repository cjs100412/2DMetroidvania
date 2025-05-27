using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class DashBoss : MonoBehaviour
{
    public ParticleSystem dieEffect;
    private SpriteRenderer spriteRenderer;
    CinemachineCamera cinemachineCamera;
    public GameObject wall;

    [Header("ī�޶� ����")]
    public float zoomFactor = 0.6f;
    public float zoomDuration = 2f;

    [Header("���ο� ���")]
    public float slowTimeScale = 0.3f;
    public float slowDuration = 3f;

    private float originalOrthoSize;

    public int maxHp = 100;
    private int hp;
    public int damage = 2;
    private bool isDead = false;


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cinemachineCamera = GameObject.FindWithTag("Cinemachine").GetComponent<CinemachineCamera>();

        if (cinemachineCamera != null)
            originalOrthoSize = cinemachineCamera.Lens.OrthographicSize;
        else
            Debug.LogError("Cinemachine Camera�� �Ҵ���� �ʾҽ��ϴ�.");

        hp = maxHp;
    }


    // ���� �Ծ��� �� ȣ��
    public void Damaged(int amount)
    {
        if (isDead) return;
        hp -= amount;
        StartCoroutine(RedFlash());

        if (hp <= 0)
            Die();
    }

    private IEnumerator RedFlash()
    {
        Color original = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = original;
    }

    private void Die()
    {
        isDead = true;
        if (dieEffect != null)
            Instantiate(dieEffect, transform.position, Quaternion.identity);

        // ī�޶� �� �� ���ο� ��� ����
        StartCoroutine(DoCameraZoom());
        StartCoroutine(DoSlowMotion());

        // ��� ȿ���� ���� �� ������ ����
        float totalDuration = zoomDuration * 2 + slowDuration + 0.1f;
        Destroy(gameObject, totalDuration);
        Destroy(wall);
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
