using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [Header("이동 속도 & 점프 힘")]
    public float moveSpeed = 10f;
    public float jumpForce = 15f;

    [Header("허용 점프 횟수")]
    public int maxJumpCount = 1;

    [Header("공격 이펙트")]
    public GameObject AttackEffect;
    public GameObject AttackUpgradeEffect;

    [Header("바닥 검사 레이어")]
    public LayerMask groundLayer;

    [Header("공격력, 공격 쿨타임")]
    public int damage = 5;
    public float attackCooldown = 0.5f;

    [Header("공격 판정 위치")]
    public Transform attackPoint;
    public float attackRadius = 2f;
    public LayerMask enemyLayer;

    [Header("점프 보정")]
    public float fallMultiplier = 3f;
    public float lowJumpMultiplier = 2f;

    [Header("대시 세팅")]
    public float dashSpeed = 20f;         // 대시 속도
    public float dashDuration = 0.2f;     // 대시 지속 시간
    private bool isDashing = false;
    public float dashCooldown = 1f;

    [Header("원거리 공격 세팅")]
    [Tooltip("발사할 투사체 프리팹")]
    public GameObject rangedProjectilePrefab;
    [Tooltip("투사체 속도")]
    public float rangedProjectileSpeed = 15f;
    [Tooltip("투사체 발사 쿨타임")]
    public float rangedAttackCooldown = 1f;

    [Header("넉백 세기")]
    [Tooltip("플레이어가 맞을 때 받는 넉백 세기")]
    public float playerKnockbackForce = 5f;
    [Tooltip("적이 받을 넉백 세기")]
    public float enemyKnockbackForce = 10f;
    [Tooltip("플레이어가 넉백당해 멈춰 있는 시간(초)")]
    public float playerKnockbackDuration = 0.2f;


    private bool horizontalLocked = false;

    public void LockHorizontal() => horizontalLocked = true;
    public void UnlockHorizontal() => horizontalLocked = false;


    public int jumpCount = 0;
    public bool isGrounded = false;
    private float hInput = 0f;
    private float lastAttackTime;
    private float lastDashTime;
    private bool canDash = false;
    public bool isKnockback = false;
    public bool canRangedAttack = false;
    private float lastRangedTime = -Mathf.Infinity;

    public Collider2D playerCollider;
    public LayerMask platformLayer;
    public PlayerUI playerUI;

    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private bool appliedPowerUpgrade = false;
    private bool appliedRangeUpgrade = false;
    private bool appliedSpeedUpgrade = false;
    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (playerCollider == null)
            playerCollider = GetComponent<Collider2D>();

        // ─── 상점 구매 정보 확인 & 스탯 보정 ───
        if (GameManager.I != null)
        {
            // 공격력 상승
            if (GameManager.I.IsBoughtAttackPower() && !appliedPowerUpgrade)
            {
                damage += 5;
                appliedPowerUpgrade = true;
            }

            // 공격 범위 상승
            if (GameManager.I.IsBoughtAttackRange() && !appliedRangeUpgrade)
            {
                attackRadius += 1.5f;
                appliedRangeUpgrade = true;
            }

            // 공격 속도 상승
            if (GameManager.I.IsBoughtAttackSpeed() && !appliedSpeedUpgrade)
            {
                attackCooldown = Mathf.Max(0.1f, attackCooldown - 0.2f);
                appliedSpeedUpgrade = true;
            }
        }
    }

    void Update()
    {
        if (gameObject.GetComponent<PlayerHealth>().isDead == true) return;


        // 수평 입력 읽기
        hInput = Input.GetAxisRaw("Horizontal");


        //대시
        if(Input.GetKeyDown(KeyCode.X) && canDash && !isDashing)
        {
            StartCoroutine(PerformDash());
        }

        if (isDashing || isKnockback)
            return;

        animator.SetBool("isGround", isGrounded);

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumpCount)
        {
            animator.SetTrigger("isJumping");
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++;
            isGrounded = false;
            UnlockHorizontal();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            // 점프 중(위로 상승 중)일 때만 적용
            if (rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            }
        }

        float vy = rb.linearVelocity.y;
        animator.SetFloat("VerticalVelocity", vy, 0.1f /*dampTime*/, Time.deltaTime);

        // 애니메이션 & 스프라이트 반전
        //animator.SetBool("isRunning", hInput != 0 && isGrounded);
        //animator.SetBool("isJumping", !isGrounded && rb.linearVelocity.y > 0.1f);
        //animator.SetBool("isFalling", !isGrounded && rb.linearVelocity.y < -0.1f);

        if (hInput > 0)
        {
            spriteRenderer.flipX = false;
            attackPoint.localPosition = new Vector3(
                Mathf.Abs(attackPoint.localPosition.x),
                attackPoint.localPosition.y,
                0f
            );
        }
        else if (hInput < 0)
        {
            spriteRenderer.flipX = true;
            attackPoint.localPosition = new Vector3(
                -Mathf.Abs(attackPoint.localPosition.x),
                attackPoint.localPosition.y,
                0f
            );
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            OnAttack();
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            OnRangedAttack();
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
            return;
        if (isKnockback)
            return;
        if (GetComponent<GrappleLauncher>().isAttached)
            return;


        if (!horizontalLocked)
        {
            // 수평 이동
            rb.linearVelocity = new Vector2(hInput * moveSpeed, rb.linearVelocity.y);
        }
        
        

        if (rb.IsSleeping())
            rb.WakeUp();

        float speed = Mathf.Abs(rb.linearVelocity.x);
        // 부드러운 파라미터 변화: SetFloat(name, value, dampTime, deltaTime)
        animator.SetFloat("Speed", speed);

        // Better Jump: 떨어질 때 더 빠르게
        if (rb.linearVelocity.y < 0)
        {
            // Physics2D.gravity.y는 음수
            float extraGravity = Physics2D.gravity.y * (fallMultiplier - 1);
            rb.linearVelocity += Vector2.up * extraGravity * Time.fixedDeltaTime;
        }
        // 점프 버튼을 떼었을 때 짧게 점프되도록
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            float extraGravity = Physics2D.gravity.y * (lowJumpMultiplier - 1);
            rb.linearVelocity += Vector2.up * extraGravity * Time.fixedDeltaTime;
        }
    }

    public void PlayFootstep()
    {
        SoundManager.Instance.PlaySFX(SFX.Run);
    }

    private IEnumerator PerformDash()
    {
        if (Time.time >= lastDashTime + dashCooldown)
        {
            lastDashTime = Time.time;

            isDashing = true;
            animator.SetTrigger("isDash");

            UnlockHorizontal();

            // 바라보는 방향 판단 (flipX 기준)
            float dir = spriteRenderer.flipX ? -1f : 1f;

            // 대시 시작: 지정된 속도로 일정 시간 이동
            float timer = 0f;
            while (timer < dashDuration)
            {
                rb.linearVelocity = new Vector2(dir * dashSpeed, 0f);
                timer += Time.deltaTime;
                yield return null;
            }

            // 대시 끝내고 관성 제거
            rb.linearVelocity = Vector2.zero;
            isDashing = false;
        }
    }

    private void OnAttack()
    {
        // 1) 쿨타임 검사
        if (Time.time < lastAttackTime + attackCooldown)
            return;

        // 2) 공격 애니메이션 트리거 (애니메이터에 isAttack 트리거가 있어야 함)
        animator.SetTrigger("isAttack");
        
        if (appliedRangeUpgrade)
        {
            GameObject ef = Instantiate(AttackUpgradeEffect, attackPoint.position, Quaternion.identity);
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFX.UpgradeAttack);
            }
            if (transform.localScale.x < 0f)
            {
                Vector3 s = ef.transform.localScale;
                s.x *= -1f;
                ef.transform.localScale = s;
            }
        }
        else
        {
            GameObject ef = Instantiate(AttackEffect, attackPoint.position, Quaternion.identity);
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFX.Attack);
            }
            if (transform.localScale.x < 0f)
            {
                Vector3 s = ef.transform.localScale;
                s.x *= -1f;
                ef.transform.localScale = s;
            }
        }

        // 3) 최종적으로 마지막 공격 시간 갱신
        lastAttackTime = Time.time;

        // 4) OverlapCircleAll로 공격 범위 내의 모든 적을 가져옴
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);

        if (hits.Length == 0)
            return;

        // 5) 공격 성공: 각 히트 대상(Enemy들)에 데미지 & 넉백 적용
        foreach (var hitCollider in hits)
        {
            // 5-1) 바라보는 방향을 계산: (적 위치 - 플레이어 위치).normalized
            Vector2 directionToEnemy = ((Vector2)hitCollider.transform.position - (Vector2)transform.position).normalized;
            directionToEnemy.y = 0f;
            // 5-2) 적에게 데미지 주기
            //    (기존에는 hitCollider.GetComponent<Enemy>()?.Damaged(damage) 식으로만 하셨을 텐데,
            //     지금은 Damaged 후에 넉백 로직을 별도로 직접 호출하거나, 
            //     AddForce를 걸어 줍니다.)
            hitCollider.GetComponent<Enemy>()?.Damaged(damage);
            hitCollider.GetComponent<DashBoss>()?.Damaged(damage);
            hitCollider.GetComponent<DoubleJumpBoss>()?.Damaged(damage);
            hitCollider.GetComponent<GrappleBoss>()?.Damaged(damage);
            hitCollider.GetComponent<LastBoss>()?.Damaged(damage);

            // 5-3) 적에게 큰 넉백 주기
            Rigidbody2D enemyRb = hitCollider.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                // Impulse 모드로 단번에 힘을 가해 준다
                enemyRb.AddForce(directionToEnemy * enemyKnockbackForce, ForceMode2D.Impulse);
            }

            // 5-4) 플레이어에게 작은 넉백 주기 (적 반대 방향으로)
            //    → directionToEnemy 방향이 적쪽이므로, -directionToEnemy는 플레이어 쪽(반대)
            if (rb != null)
            {
                rb.AddForce(-directionToEnemy * playerKnockbackForce, ForceMode2D.Impulse);
                isKnockback = true;
                StartCoroutine(ResetPlayerKnockback());
            }
        }
    }
    public IEnumerator ResetPlayerKnockback()
    {
        // 잠시 대기하는 동안 FixedUpdate에서 이동이 skip된다.
        yield return new WaitForSeconds(playerKnockbackDuration);
        isKnockback = false;
    }
    private void OnRangedAttack()
    {
        // 1) 언락 여부
        if (!canRangedAttack) return;
        // 2) 쿨타임 검사
        if (Time.time < lastRangedTime + rangedAttackCooldown) return;
        // 3) MP 검사
        if (gameObject.GetComponent<PlayerHealth>().currentMp < 1) return;

        // 4) 발사
        lastRangedTime = Time.time;
        gameObject.GetComponent<PlayerHealth>().currentMp--;

        // 애니메이터에 ranged 트리거가 있다면
        //animator.SetTrigger("isRangedAttack");
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFX.RangeAttack);
        }
        // 투사체 생성 & 발사
        Vector3 spawnPos = attackPoint.position;
        Quaternion rot = Quaternion.identity;
        var proj = Instantiate(rangedProjectilePrefab, spawnPos, rot);

        // 바라보는 방향에 맞춰 속도 설정 (Y축 0)
        float dir = spriteRenderer.flipX ? -1f : 1f;
        var prb = proj.GetComponent<Rigidbody2D>();
        if (prb != null)
            prb.linearVelocity = new Vector2(dir * rangedProjectileSpeed, 0f);
    }
    public void ApplyShopUpgrades()
    {
        if (GameManager.I == null) return;
        if (GameManager.I.IsBoughtAttackPower() && !appliedPowerUpgrade)
        {
            damage += 5;
            appliedPowerUpgrade = true;
        }
        if (GameManager.I.IsBoughtAttackRange() && !appliedRangeUpgrade)
        {
            attackRadius += 1.5f;
            appliedRangeUpgrade = true;
        }
        if (GameManager.I.IsBoughtAttackSpeed() && !appliedSpeedUpgrade)
        {
            attackCooldown = Mathf.Max(0.1f, attackCooldown - 0.2f);
            appliedSpeedUpgrade = true;
        }
    }
    public void UnlockDoubleJump()
    {
        maxJumpCount = 2;
        Debug.Log("Double Jump UNLOCKED");
    }

    public void UnlockDash()
    {
        canDash = true;
        Debug.Log("Dash UNLOCKED");
    }

    public void UnlockRangedAttack()
    {
        canRangedAttack = true;
        playerUI.ShowMpSlider();
        Debug.Log("RangedAttack UNLOCKED");
    }

    void OnDrawGizmosSelected()
    {
        // 공격 범위 시각화 (빨간색)
        Gizmos.color = Color.red;
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}