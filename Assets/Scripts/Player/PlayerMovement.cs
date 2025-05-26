using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [Header("�̵� �ӵ� & ���� ��")]
    public float moveSpeed = 10f;
    public float jumpForce = 15f;

    [Header("��� ���� Ƚ��")]
    public int maxJumpCount = 1;

    [Header("�ٴ� �˻� ���̾�")]
    public LayerMask groundLayer;

    [Header("���ݷ�, ���� ��Ÿ��")]
    public int damage = 3;
    public float attackCooldown = 2f;

    [Header("���� ���� ��ġ")]
    public Transform attackPoint;
    public float attackRadius = 2f;
    public LayerMask enemyLayer;

    [Header("���� ����")]
    public float fallMultiplier = 3f;
    public float lowJumpMultiplier = 2f;

    [Header("��� ����")]
    public float dashSpeed = 20f;         // ��� �ӵ�
    public float dashDuration = 0.2f;     // ��� ���� �ð�
    private bool isDashing = false;
    public float dashCooldown = 1f;


    public int jumpCount = 0;
    public bool isGrounded = false;
    private float hInput = 0f;
    private float lastAttackTime;
    private float lastDashTime;
    private bool canDash = false;

    public Collider2D playerCollider;
    public LayerMask platformLayer;

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
        if (GetComponent<PlayerHealth>().isDead == true) return;

        

        // ���� �Է� �б�
        hInput = Input.GetAxisRaw("Horizontal");

        //���
        if(Input.GetKeyDown(KeyCode.X) && canDash && !isDashing)
        {
            StartCoroutine(PerformDash());
        }

        if (isDashing)
            return;

        // ���� �Է� ó��
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumpCount)
        {
            if (Input.GetKey(KeyCode.DownArrow))
            {
                StartCoroutine(DropThroughPlatform());
            }
            else
            {
                // y�� �ӵ��� �缳���ؼ� ������ ���̷� ����
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpCount++;
                isGrounded = false;  // ��� ������ ���·� ��ŷ
            }

            
        }

        // �ִϸ��̼� & ��������Ʈ ����
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
        if (isDashing)
            return;

        // ���� �̵��� FixedUpdate����
        rb.linearVelocity = new Vector2(hInput * moveSpeed, rb.linearVelocity.y);

        // Better Jump: ������ �� �� ������
        if (rb.linearVelocity.y < 0)
        {
            // Physics2D.gravity.y�� ����
            float extraGravity = Physics2D.gravity.y * (fallMultiplier - 1);
            rb.linearVelocity += Vector2.up * extraGravity * Time.fixedDeltaTime;
        }
        // ���� ��ư�� ������ �� ª�� �����ǵ���
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            float extraGravity = Physics2D.gravity.y * (lowJumpMultiplier - 1);
            rb.linearVelocity += Vector2.up * extraGravity * Time.fixedDeltaTime;
        }
    }


    private IEnumerator DropThroughPlatform()
    {
        // �÷��̾� �ϴܿ� �ִ� �÷��� �ݶ��̴��� Ž��
        Vector2 origin = (Vector2)playerCollider.bounds.center - Vector2.up * (playerCollider.bounds.extents.y + 2.1f);
        Vector2 size = playerCollider.bounds.size * 0.9f;
        Collider2D[] hits = Physics2D.OverlapBoxAll(origin, size, 0f, platformLayer);

        // �ش� �÷�������� �浹 ����
        foreach (var plat in hits)
        {
            Physics2D.IgnoreCollision(playerCollider, plat, true);
            Debug.Log("�÷��� �浹 ���� on");
        }

        // �ҷ� �ϰ��Ͽ� ������ ������������ ����
        transform.position += Vector3.down * 0.2f;

        // ���� ���� �ð� (���� �÷��̿� �°� ����)
        yield return new WaitForSeconds(0.7f);

        //�浹 ����
        foreach (var plat in hits)
        {
            Physics2D.IgnoreCollision(playerCollider, plat, false);
            Debug.Log("�÷��� �浹 ���� off");
        }
        Debug.Log("��������");
    }

    private IEnumerator PerformDash()
    {
        if (Time.time >= lastDashTime + dashCooldown)
        {
            lastDashTime = Time.time;

            isDashing = true;
            animator.SetTrigger("isDash");

            // �ٶ󺸴� ���� �Ǵ� (flipX ����)
            float dir = spriteRenderer.flipX ? -1f : 1f;

            // ��� ����: ������ �ӵ��� ���� �ð� �̵�
            float timer = 0f;
            while (timer < dashDuration)
            {
                rb.linearVelocity = new Vector2(dir * dashSpeed, 0f);
                timer += Time.deltaTime;
                yield return null;
            }

            // ��� ������ ���� ����
            rb.linearVelocity = Vector2.zero;
            isDashing = false;
        }
    }

    //// ��Ʈ���� Collider2D + OnCollisionEnter/Exit�θ� ���� ����
    //void OnCollisionEnter2D(Collision2D col)
    //{
    //    if (((1 << col.gameObject.layer) & groundLayer) != 0)
    //    {
    //        isGrounded = true;
    //        jumpCount = 0;  // ���鿡 ������ ���� ī��Ʈ ����
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
        rb.linearVelocity = Vector2.zero;
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            // ������ ����
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

    public void UnlockDash()
    {
        canDash = true;
        Debug.Log("Dash UNLOCKED");
    }
}