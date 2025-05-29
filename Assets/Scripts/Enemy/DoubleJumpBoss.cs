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

    [Header("카메라 줌인")]
    public float zoomFactor = 0.6f;
    public float zoomDuration = 2f;

    private float originalOrthoSize;

    [Header("슬로우 모션")]
    public float slowTimeScale = 0.3f;
    public float slowDuration = 3f;

    [Header("체력, 데미지")]
    public int maxHp = 100;
    private int hp;
    public int damage = 2;

    public bool isDead = false;

    [Header("움직임")]
    public float moveSpeed = 3f;  // 이동 속도

    [Header("공격 셋팅")]
    public float attackRange = 5f;
    public float attackCooldown = 2f;
    public Transform attackPoint;
    public float attackRadius = 6f;
    public LayerMask playerLayer;

    private PlayerHealth playerHealth;

    private float lastAttackTime; // 마지막 공격 시각 저장
    private bool isAttacking;     // 공격 중 플래그

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
        else Debug.LogError("Player 태그 오브젝트가 없습니다.");

        if (cinemachineCamera != null)
            originalOrthoSize = cinemachineCamera.Lens.OrthographicSize;
        else
            Debug.LogError("Cinemachine Camera가 할당되지 않았습니다.");

        playerHealth = player.GetComponent<PlayerHealth>();

    }

    private void Update()
    {
        // 체력이 0 이하거나 공격 중이라면 상태 전환 로직을 건너뛰고 방향 처리만 수행
        if (hp <= 0 || isAttacking)
        {
            HandleFacing();
            return;
        }

        // 플레이어가 사망했으면 아무 동작도 하지 않음
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
        // 죽으면 이동 정지
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (IsBusy)
            return;

        // 플레이어와 거리 계산
        Vector2 diff = (Vector2)player.transform.position - (Vector2)transform.position;
        float distSq = diff.sqrMagnitude;
        Vector2 toPlayer = diff.normalized;

        // 그 방향으로 속도 설정

        toPlayer.y = 0; // y축은 이동하지 않는다
        rb.linearVelocity = toPlayer * moveSpeed;

        if (distSq <= attackRange * attackRange)
        {
            TryAttack();
        }
    }


    // 공격 시도: 쿨타임이 지나야 실행
    private void TryAttack()
    {
        if (isAttacking)
            return;

        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;
        StartCoroutine(PerformAttack());
    }

    // 실제 공격 처리 코루틴
    private IEnumerator PerformAttack()
    {
        Debug.Log("DoubleJumpBoss TryAttack");
        isAttacking = true;

        // 공격 중에는 수평 이동 멈춤, 수직 속도 유지
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // 타격 타이밍까지 대기 (0.2초)
        yield return new WaitForSeconds(0.2f);

        // OverlapCircleAll로 데미지 판정
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
        foreach (var hit in hits)
            hit.GetComponent<PlayerHealth>()?.Damaged(damage);

        // 공격 종료 후 상태 복귀
        isAttacking = false;
    }

    // 피해 입었을 때 호출
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

        // 카메라 줌 및 슬로우 모션 시작
        StartCoroutine(DoCameraZoom());
        StartCoroutine(DoSlowMotion());

        // 모든 효과가 끝난 후 아이템 제거
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

    void OnDrawGizmosSelected()
    {
        // 공격 범위 시각화 (빨간색)
        Gizmos.color = Color.red;
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);

    }
}
