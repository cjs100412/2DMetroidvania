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

    [Header("���Ÿ� ���� ����")]
    [Tooltip("�߻��� ����ü ������")]
    public GameObject rangedProjectilePrefab;
    [Tooltip("����ü �ӵ�")]
    public float rangedProjectileSpeed = 15f;
    [Tooltip("����ü �߻� ��Ÿ��")]
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

        

        // ���� �Է� �б�
        hInput = Input.GetAxisRaw("Horizontal");


        //���
        if(Input.GetKeyDown(KeyCode.X) && canDash && !isDashing)
        {
            StartCoroutine(PerformDash());
        }

        if (isDashing)
            return;

        animator.SetBool("isGround", isGrounded);

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
                animator.SetTrigger("isJumping");
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpCount++;
                isGrounded = false;  // ��� ������ ���·� ��ŷ
            }

            
        }
        float vy = rb.linearVelocity.y;
        animator.SetFloat("VerticalVelocity", vy, 0.1f /*dampTime*/, Time.deltaTime);

        // �ִϸ��̼� & ��������Ʈ ����
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
            // ���� �̵��� FixedUpdate����
            rb.linearVelocity = new Vector2(hInput * moveSpeed, rb.linearVelocity.y);
        }
        

        if (rb.IsSleeping())
            rb.WakeUp();

        float speed = Mathf.Abs(rb.linearVelocity.x);
        // �ε巯�� �Ķ���� ��ȭ: SetFloat(name, value, dampTime, deltaTime)
        animator.SetFloat("Speed", speed);

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
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            // ������ ����
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
        // 1) ��� ����
        if (!canRangedAttack) return;
        // 2) ��Ÿ�� �˻�
        if (Time.time < lastRangedTime + rangedAttackCooldown) return;
        // 3) MP �˻�
        if (gameObject.GetComponent<PlayerHealth>().currentMp < 1) return;

        // 4) �߻�
        lastRangedTime = Time.time;
        gameObject.GetComponent<PlayerHealth>().currentMp--;

        // �ִϸ����Ϳ� ranged Ʈ���Ű� �ִٸ�
        animator.SetTrigger("isRangedAttack");

        // ����ü ���� & �߻�
        Vector3 spawnPos = attackPoint.position;
        Quaternion rot = Quaternion.identity;
        var proj = Instantiate(rangedProjectilePrefab, spawnPos, rot);

        // �ٶ󺸴� ���⿡ ���� �ӵ� ���� (Y�� 0)
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