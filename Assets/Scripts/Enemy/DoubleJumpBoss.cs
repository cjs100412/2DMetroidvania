using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class DoubleJumpBoss : MonoBehaviour, IBossDeath
{
    public ParticleSystem dieEffect;
    private SpriteRenderer spriteRenderer;
    CinemachineCamera cinemachineCamera;
    public GameObject wall;

    private Rigidbody2D rb;
    private BossController bossController;
    private GameObject player;

    [Header("ī�޶� ����")]
    public float zoomFactor = 0.6f;
    public float zoomDuration = 2f;

    private float originalOrthoSize;

    [Header("���ο� ���")]
    public float slowTimeScale = 0.3f;
    public float slowDuration = 3f;

    [Header("ü��, ������")]
    public int maxHp = 100;
    private int hp;
    public int damage = 2;

    public bool isDead = false;

    [Header("������")]
    public float moveSpeed = 3f;  // �̵� �ӵ�

    [Header("���� ����")]
    public float attackRange = 5f;
    public float attackCooldown = 2f;
    public Transform attackPoint;
    public float attackRadius = 6f;
    public LayerMask playerLayer;

    private PlayerHealth playerHealth;

    private float lastAttackTime; // ������ ���� �ð� ����
    private bool isAttacking;     // ���� �� �÷���

    public bool IsBusy => bossController != null && bossController.isBusy;
    public bool IsDead => isDead;

    private void Awake()
    {

        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        bossController = GetComponent<BossController>();
        cinemachineCamera = GameObject.FindWithTag("Cinemachine").GetComponent<CinemachineCamera>();
        hp = maxHp;

        var pgo = GameObject.FindWithTag("Player");
        if (pgo != null) player = pgo;
        else Debug.LogError("Player �±� ������Ʈ�� �����ϴ�.");

        if (cinemachineCamera != null)
            originalOrthoSize = cinemachineCamera.Lens.OrthographicSize;
        else
            Debug.LogError("Cinemachine Camera�� �Ҵ���� �ʾҽ��ϴ�.");

        playerHealth = player.GetComponent<PlayerHealth>();

    }

    private void Update()
    {
        // ü���� 0 ���ϰų� ���� ���̶�� ���� ��ȯ ������ �ǳʶٰ� ���� ó���� ����
        if (hp <= 0 || isAttacking)
        {
            HandleFacing();
            return;
        }

        // �÷��̾ ��������� �ƹ� ���۵� ���� ����
        if (playerHealth.isDead)
            return;

        HandleFacing();
    }

    private void HandleFacing()
    {
        SetScaleX(player.transform.position.x > transform.position.x ? 6f : -6f);

    }

    private void SetScaleX(float x)
    {
        Vector3 s = transform.localScale;
        s.x = x;
        transform.localScale = s;
    }

    void FixedUpdate()
    {
        // ������ �̵� ����
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (IsBusy)
            return;

        // �÷��̾�� �Ÿ� ���
        Vector2 diff = (Vector2)player.transform.position - (Vector2)transform.position;
        float distSq = diff.sqrMagnitude;
        Vector2 toPlayer = diff.normalized;

        // �� �������� �ӵ� ����

        toPlayer.y = 0; // y���� �̵����� �ʴ´�
        rb.linearVelocity = toPlayer * moveSpeed;

        if (distSq <= attackRange * attackRange)
        {
            TryAttack();
        }
    }


    // ���� �õ�: ��Ÿ���� ������ ����
    private void TryAttack()
    {
        if (isAttacking)
            return;

        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;
        StartCoroutine(PerformAttack());
    }

    // ���� ���� ó�� �ڷ�ƾ
    private IEnumerator PerformAttack()
    {
        Debug.Log("DoubleJumpBoss TryAttack");
        isAttacking = true;

        // ���� �߿��� ���� �̵� ����, ���� �ӵ� ����
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // Ÿ�� Ÿ�ֱ̹��� ��� (0.2��)
        yield return new WaitForSeconds(0.2f);

        // OverlapCircleAll�� ������ ����
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
        foreach (var hit in hits)
            hit.GetComponent<PlayerHealth>()?.Damaged(damage);

        // ���� ���� �� ���� ����
        isAttacking = false;
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

    void OnDrawGizmosSelected()
    {
        // ���� ���� �ð�ȭ (������)
        Gizmos.color = Color.red;
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);

    }
}
