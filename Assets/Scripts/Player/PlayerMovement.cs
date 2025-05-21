using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [Header("이동 속도/점프 힘")]
    public float moveSpeed = 10f;
    public float jumpForce = 15f;

    [Header("바닥 체크")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private float h;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        //입력 & 이동
        h = Input.GetAxisRaw("Horizontal");
        Vector2 Velocity = rb.linearVelocity;
        Velocity.x = h * moveSpeed;

        //점프
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Velocity.y = jumpForce;
        }
        rb.linearVelocity = Velocity;

        //플레이어 스프라이트 좌우 반전
        if (h > 0)
            spriteRenderer.flipX = false;   // 오른쪽 바라보기
        else if (h < 0)
            spriteRenderer.flipX = true;    // 왼쪽 바라보기

        //바닥 체크
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        //달리기 상태
        bool isRunning = h != 0 && isGrounded;
        animator.SetBool("isRunning", isRunning);

        //공중 상태 분기
        if (isGrounded)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
        }
        else
        {
            if (rb.linearVelocity.y > 0.1f)
            {
                animator.SetBool("isJumping", true);
                animator.SetBool("isFalling", false);
            }
            else if (rb.linearVelocity.y < -0.1f)
            {
                animator.SetBool("isJumping", false);
                animator.SetBool("isFalling", true);
            }
        }
    }


}