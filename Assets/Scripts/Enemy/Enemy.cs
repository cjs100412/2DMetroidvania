using System.Collections;
using UnityEngine;

// Require Rigidbody2D, Collider2D, SpriteRenderer 컴포넌트가 반드시 함께 있어야 함
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    // 최대 체력
    public int maxHp = 10;
    // 현재 체력 (내부에서 관리)
    private int hp;
    // 적이 플레이어에게 입히는 데미지
    public int damage = 1;

    [Header("넉백 세기")]
    public float knockbackForce = 8f;
    [Tooltip("넉백 상태 지속 시간")]
    public float knockbackDuration = 0.2f;

    [Header("Patrol Settings")]
    // 순찰(평상시 이동) 속도
    public float patrolSpeed = 2f;

    [Header("Chase Settings")]
    // 플레이어를 추격하기 시작하는 거리
    public float chaseRange = 10f;
    // 추격 시 이동 속도
    public float chaseSpeed = 5f;

    [Header("Attack Settings")]
    // 공격 사거리
    public float attackRange = 2f;
    // 공격 쿨타임(초)
    public float attackCooldown = 1f;
    // 공격 판정 발생 위치 Transform
    public Transform attackPoint;
    // 공격 판정 반경
    public float attackRadius = 0.5f;
    // 공격 판정에 사용할 레이어 마스크(플레이어 레이어)
    public LayerMask playerLayer;

    [Header("Edge & Wall Detection")]
    // 낭떠러지/벽 감지에 사용할 레이어 마스크(땅 레이어)
    public LayerMask groundLayer;
    [Tooltip("발끝에서 얼마나 더 앞쪽을 검사할지")]
    public float edgeDetectHorizOffset = 0.05f;
    [Tooltip("발밑에서 얼마나 더 아래를 검사할지")]
    public float edgeDetectVertOffset = 0.05f;
    [Tooltip("땅 감지 레이 길이")]
    public float edgeDetectRayLength = 0.2f;

    [Header("Player Reference")]
    // 유니티 에디터에서 드래그로 할당 필요
    private Transform playerTransform;
    private PlayerHealth playerHealth;
    private PlayerInventory playerInventory;

    // AI 상태 정의: 순찰, 추격, 공격
    private enum State { Patrol, Chase, Attack }
    private State state = State.Patrol;         // 초기 상태를 Patrol로 설정
    private Vector2 patrolDir = Vector2.right;  // 순찰 방향 (초기값 오른쪽)

    // 캐싱된 컴포넌트 참조
    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer spriteRenderer;

    // 내부 상태 변수
    private float lastAttackTime; // 마지막 공격 시각 저장
    private bool isAttacking;     // 공격 중 플래그
    private bool isKnockback = false;
    void Awake()
    {
        var player = GameObject.FindWithTag("Player");
        playerTransform = player.transform;
        playerHealth = player.GetComponent<PlayerHealth>();
        playerInventory = player.GetComponent<PlayerInventory>();

        // 컴포넌트들을 캐싱하여 GetComponent 호출 최소화
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 플레이어 참조가 Inspector에 할당되지 않았다면 오류 띄우고 스크립트 비활성화
        if (playerTransform == null || playerHealth == null)
        {
            Debug.LogError($"[{name}] Player Transform & Health must be assigned!", this);
            enabled = false;
            return;
        }

        // 현재 체력을 최대 체력으로 초기화
        hp = maxHp;
    }

    void Update()
    {
        if (isKnockback)
            return;
        // 체력이 0 이하거나 공격 중이라면 상태 전환 로직을 건너뛰고 방향 처리만 수행
        if (hp <= 0 || isAttacking)
        {
            HandleFacing();
            return;
        }

        // 플레이어가 사망했으면 아무 동작도 하지 않음
        if (playerHealth.isDead)
            return;

        // 현재 상태를 거리 기준으로 갱신
        UpdateState();

        // 공격 상태면 공격 시도
        if (state == State.Attack)
            TryAttack();

        // 상태와 관계없이 향하는 방향으로 스프라이트 뒤집기
        HandleFacing();
    }

    void FixedUpdate()
    {
        if (isKnockback)
            return;
        // 체력이 0 이거나 공격 중이면 이동 로직을 멈춤
        if (hp <= 0 || isAttacking)
            return;

        // 상태에 따라 물리 이동 처리
        switch (state)
        {
            case State.Patrol:
                FixedPatrol();
                break;
            case State.Chase:
                FixedChase();
                break;
        }
    }

    // 상태 판정: 플레이어와의 거리로 상태 전환
    private void UpdateState()
    {
        float distSqr = (playerTransform.position - transform.position).sqrMagnitude;
        float attackRangeSqr = attackRange * attackRange;
        float chaseRangeSqr = chaseRange * chaseRange;

        if (distSqr <= attackRangeSqr)
            state = State.Attack;
        else if (distSqr <= chaseRangeSqr)
            state = State.Chase;
        else
            state = State.Patrol;
    }

    // 순찰 상태에서 호출: 좌우로 이동하고 에지/벽 감지
    private void FixedPatrol()
    {
        // 수평 속도 설정, 수직 속도는 기존 중력/관성을 유지
        rb.linearVelocity = new Vector2(patrolDir.x * patrolSpeed, rb.linearVelocity.y);

        // 낭떠러지 검사용 레이 시작점 계산: 발끝 + 오프셋
        Vector2 origin = new Vector2(
            transform.position.x + patrolDir.x * (col.bounds.extents.x + edgeDetectHorizOffset),
            transform.position.y - (col.bounds.extents.y + edgeDetectVertOffset)
        );

        // 아래 방향으로 레이캐스트하여 땅이 있는지 검사
        RaycastHit2D groundHit = Physics2D.Raycast(origin, Vector2.down, edgeDetectRayLength, groundLayer);
        bool isGroundAhead = groundHit.collider != null;

        // 벽이 있는지 검사: 몸통 높이 약간 위에서 앞쪽으로 레이캐스트
        RaycastHit2D wallHit = Physics2D.Raycast(
            (Vector2)transform.position + Vector2.up * 0.1f,
            patrolDir,
            col.bounds.extents.x + edgeDetectHorizOffset,
            groundLayer
        );

        // 에디터에서만 시각화용 레이 그리기
#if UNITY_EDITOR
        Debug.DrawRay(origin, Vector2.down * edgeDetectRayLength, Color.red);
        Debug.DrawRay(transform.position + Vector3.up * 0.1f,
                      patrolDir * (col.bounds.extents.x + edgeDetectHorizOffset),
                      Color.blue);
#endif

        // 낭떠러지이거나 벽이 있으면 방향 전환
        if (!isGroundAhead || wallHit.collider != null)
            FlipDirection();
    }

    // 추격 상태에서 호출: 플레이어를 향해 이동
    private void FixedChase()
    {
        Vector2 dir = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * chaseSpeed, rb.linearVelocity.y);
    }

    // 공격 시도: 쿨타임이 지나야 실행
    private void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;
        StartCoroutine(PerformAttack());
    }

    // 실제 공격 처리 코루틴
    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        state = State.Attack;

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
        state = State.Chase;
    }

    // 피해 입었을 때 호출
    public void Damaged(int amount)
    {
        hp -= amount;
        StartCoroutine(RedFlash());

        if (playerTransform != null && rb != null)
        {
            Vector2 dir = ((Vector2)transform.position - (Vector2)playerTransform.position).normalized;
            rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);

            // 넉백 상태 진입
            isKnockback = true;
            StartCoroutine(ResetEnemyKnockback());
        }

        if (hp <= 0)
            Die();
    }
    private IEnumerator ResetEnemyKnockback()
    {
        yield return new WaitForSeconds(knockbackDuration);
        isKnockback = false;
    }
    // 피격 시 깜박임 효과 코루틴
    private IEnumerator RedFlash()
    {
        Color original = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = original;
    }

    // 사망 처리
    private void Die()
    {
        if(playerHealth.currentMp < 5)
            playerHealth.currentMp++;

        playerInventory.AddCoins(5);

        StopAllCoroutines();  // 진행 중인 코루틴 정지
        enabled = false;      // 스크립트 비활성화
        rb.linearVelocity = Vector2.zero;  // 관성 제거
        Destroy(gameObject, 0.2f);        // 0.2초 후 오브젝트 제거
    }

    // 진행 방향 반전 및 스프라이트 뒤집기
    private void FlipDirection()
    {
        patrolDir *= -1f;
        SetScaleX(patrolDir.x > 0f ? 1f : -1f);
    }

    // 상태에 따라 바라보는 방향 처리
    private void HandleFacing()
    {
        if (state == State.Chase || state == State.Attack)
            SetScaleX(playerTransform.position.x > transform.position.x ? 1f : -1f);
    }

    // localScale.x 설정 헬퍼
    private void SetScaleX(float x)
    {
        Vector3 s = transform.localScale;
        s.x = x;
        transform.localScale = s;
    }

    // 에디터에서 선택 시 범위 시각화
    void OnDrawGizmosSelected()
    {
        // 추격 범위 시각화 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        // 공격 범위 시각화 (빨간색)
        Gizmos.color = Color.red;
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);

        // 낭떠러지 레이 시각화 (파란색)
        if (col != null)
        {
            Vector2 origin = new Vector2(
                transform.position.x + patrolDir.x * (col.bounds.extents.x + edgeDetectHorizOffset),
                transform.position.y - (col.bounds.extents.y + edgeDetectVertOffset)
            );
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(origin, origin + Vector2.down * edgeDetectRayLength);
        }
    }
}
