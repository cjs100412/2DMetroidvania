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
    [Tooltip("낙하할 때 중력을 얼마나 강하게 할지 (1 이상)")]
    public float fallMultiplier = 2.5f;
    [Tooltip("점프 버튼을 빨리 떼었을 때 적용할 중력 배율 (1 이상)")]
    public float lowJumpMultiplier = 2f;

    private int jumpCount = 0;
    private bool isGrounded = false;
    private float hInput = 0f;
    private float lastAttackTime;



    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
    }

    void Update()
    {
        if (GetComponent<PlayerHealth>().isDead == true) return;

        // 수평 입력 읽기
        hInput = Input.GetAxisRaw("Horizontal");

        // 점프 입력 처리
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumpCount)
        {
            // y축 속도만 재설정해서 일정한 높이로 점프
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++;
            isGrounded = false;  // 즉시 비접지 상태로 마킹
        }

        // 애니메이션 & 스프라이트 반전
        animator.SetBool("isRunning", hInput != 0 && isGrounded);
        animator.SetBool("isJumping", !isGrounded && rb.linearVelocity.y > 0.1f);
        animator.SetBool("isFalling", !isGrounded && rb.linearVelocity.y < -0.1f);

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
    }

    void FixedUpdate()
    {
        // 수평 이동은 FixedUpdate에서
        rb.linearVelocity = new Vector2(hInput * moveSpeed, rb.linearVelocity.y);

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

    // 비트리거 Collider2D + OnCollisionEnter/Exit로만 접지 판정
    void OnCollisionEnter2D(Collision2D col)
    {
        if (((1 << col.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = true;
            jumpCount = 0;  // 지면에 닿으면 점프 카운트 리셋
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (((1 << col.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = false;
        }
    }

    private void OnAttack()
    {
        rb.linearVelocity = Vector2.zero;
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            // 데미지 판정
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);
            foreach (var hit in hits)
            {
                hit.GetComponent<Enemy>()?.Damaged(damage);
            }
            animator.SetTrigger("isAttack");
        }
    }

    public void UnlockDoubleJump()
    {
        maxJumpCount = 2;
        Debug.Log("Double Jump UNLOCKED");
    }
}