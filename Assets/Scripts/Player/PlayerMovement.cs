using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [Header("�̵� �ӵ�/���� ��")]
    public float moveSpeed = 10f;
    public float jumpForce = 15f;

    [Header("�ٴ� üũ")]
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
        //�Է� & �̵�
        h = Input.GetAxisRaw("Horizontal");
        Vector2 Velocity = rb.linearVelocity;
        Velocity.x = h * moveSpeed;

        //����
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Velocity.y = jumpForce;
        }
        rb.linearVelocity = Velocity;

        //�÷��̾� ��������Ʈ �¿� ����
        if (h > 0)
            spriteRenderer.flipX = false;   // ������ �ٶ󺸱�
        else if (h < 0)
            spriteRenderer.flipX = true;    // ���� �ٶ󺸱�

        //�ٴ� üũ
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        //�޸��� ����
        bool isRunning = h != 0 && isGrounded;
        animator.SetBool("isRunning", isRunning);

        //���� ���� �б�
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