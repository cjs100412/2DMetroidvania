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

    [Header("바닥 검사 레이어")]
    public LayerMask groundLayer;

    [Header("공격력, 공격 쿨타임")]
    public int damage = 3;
    public float attackCooldown = 2f;

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

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (playerCollider == null)
            playerCollider = GetComponent<Collider2D>();
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

        if (isDashing)
            return;

        animator.SetBool("isGround", isGrounded);

        // 점프 입력 처리
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumpCount)
        {
            if (Input.GetKey(KeyCode.DownArrow))
            {
                StartCoroutine(DropThroughPlatform());
            }
            else
            {
                // y축 속도만 재설정해서 일정한 높이로 점프
                animator.SetTrigger("isJumping");
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpCount++;
                isGrounded = false;  // 즉시 비접지 상태로 마킹
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
        if (!isKnockback)
        {
            // 수평 이동은 FixedUpdate에서
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


    private IEnumerator DropThroughPlatform()
    {
        // 플레이어 하단에 있는 플랫폼 콜라이더들 탐지
        Vector2 origin = (Vector2)playerCollider.bounds.center - Vector2.up * (playerCollider.bounds.extents.y + 2.1f);
        Vector2 size = playerCollider.bounds.size * 0.9f;
        Collider2D[] hits = Physics2D.OverlapBoxAll(origin, size, 0f, platformLayer);

        // 해당 플랫폼들과의 충돌 무시
        foreach (var plat in hits)
        {
            Physics2D.IgnoreCollision(playerCollider, plat, true);
            Debug.Log("플랫폼 충돌 무시 on");
        }

        // 소량 하강하여 완전히 빠져나오도록 유도
        transform.position += Vector3.down * 0.2f;

        // 무시 유지 시간 (게임 플레이에 맞게 조절)
        yield return new WaitForSeconds(0.7f);

        //충돌 복원
        foreach (var plat in hits)
        {
            Physics2D.IgnoreCollision(playerCollider, plat, false);
            Debug.Log("플랫폼 충돌 무시 off");
        }
        Debug.Log("하향점프");
    }

    private IEnumerator PerformDash()
    {
        if (Time.time >= lastDashTime + dashCooldown)
        {
            lastDashTime = Time.time;

            isDashing = true;
            animator.SetTrigger("isDash");

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

    //// 비트리거 Collider2D + OnCollisionEnter/Exit로만 접지 판정
    //void OnCollisionEnter2D(Collision2D col)
    //{
    //    if (((1 << col.gameObject.layer) & groundLayer) != 0)
    //    {
    //        isGrounded = true;
    //        jumpCount = 0;  // 지면에 닿으면 점프 카운트 리셋
    //    }
    //}

    //void OnCollisionExit2D(Collision2D col)
    //{
    //    if (((1 << col.gameObject.layer) & groundLayer) != 0)
    //    {
    //        isGrounded = false;
    //    }
    //}

    private void OnAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            // 데미지 판정
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);
            foreach (var hit in hits)
            {
                hit.GetComponent<Enemy>()?.Damaged(damage);
                hit.GetComponent<DashBoss>()?.Damaged(damage);
                hit.GetComponent<DoubleJumpBoss>()?.Damaged(damage);
            }
            animator.SetTrigger("isAttack");
        }
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
        animator.SetTrigger("isRangedAttack");

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
}