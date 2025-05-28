using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class DashBoss : MonoBehaviour
{
    public ParticleSystem dieEffect;
    private SpriteRenderer spriteRenderer;
    CinemachineCamera cinemachineCamera;
    public GameObject wall;

    private Rigidbody2D rb;
    private BossController bossController;
    private Transform player;

    [Header("ī�޶� ����")]
    public float zoomFactor = 0.6f;
    public float zoomDuration = 2f;

    private float originalOrthoSize;

    [Header("���ο� ���")]
    public float slowTimeScale = 0.3f;
    public float slowDuration = 3f;


    public int maxHp = 100;
    private int hp;
    public int damage = 2;
    public bool isDead = false;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;  // �̵� �ӵ�
    public float minDistanceToPlayer = 4f;  // �̺��� ������ �ڷ�
    public float maxDistanceToPlayer = 8f;  // �̺��� �ָ� �ٰ���
    public float wanderRadius = 2f;  // ���� �Ÿ� �� ��ȸ �ݰ�
    private Vector2 wanderTarget;

    public bool IsBusy => bossController != null && bossController.isBusy;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        bossController = GetComponent<BossController>();
        cinemachineCamera = GameObject.FindWithTag("Cinemachine").GetComponent<CinemachineCamera>();
        hp = maxHp;

        var pgo = GameObject.FindWithTag("Player");
        if (pgo != null) player = pgo.transform;
        else Debug.LogError("Player �±� ������Ʈ�� �����ϴ�.");

        if (cinemachineCamera != null)
            originalOrthoSize = cinemachineCamera.Lens.OrthographicSize;
        else
            Debug.LogError("Cinemachine Camera�� �Ҵ���� �ʾҽ��ϴ�.");

        wanderTarget = transform.position;
    }

    void FixedUpdate()
    {
        // �׾��ų� ���� ���� ���̸� �̵� ����
        if (isDead || IsBusy)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // �÷��̾�� �Ÿ� ���
        float dist = Vector2.Distance(transform.position, player.position);
        Vector2 dir;

        if (dist < minDistanceToPlayer)
        {
            // �ʹ� ������� �ڷ�
            dir = ((Vector2)transform.position - (Vector2)player.position).normalized;
        }
        else if (dist > maxDistanceToPlayer)
        {
            // �ʹ� �־ �ٰ���
            dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        }
        else
        {
            // ���� �Ÿ� ������ ��ȸ
            if (Vector2.Distance(transform.position, wanderTarget) < 0.2f)
                ChooseNewWanderTarget();
            dir = (wanderTarget - (Vector2)transform.position).normalized;
        }

        rb.linearVelocity = dir * moveSpeed;
    }

    private void ChooseNewWanderTarget()
    {
        // ���� ��ġ ���� �ݰ� �� ���� ���� ����
        wanderTarget = (Vector2)transform.position + Random.insideUnitCircle * wanderRadius;
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
