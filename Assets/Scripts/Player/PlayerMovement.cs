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

    [Header("���� ����Ʈ")]
    public GameObject AttackEffect;
    public GameObject AttackUpgradeEffect;

    [Header("�ٴ� �˻� ���̾�")]
    public LayerMask groundLayer;

    [Header("���ݷ�, ���� ��Ÿ��")]
    public int damage = 5;
    public float attackCooldown = 0.5f;

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

    [Header("�˹� ����")]
    [Tooltip("�÷��̾ ���� �� �޴� �˹� ����")]
    public float playerKnockbackForce = 5f;
    [Tooltip("���� ���� �˹� ����")]
    public float enemyKnockbackForce = 10f;
    [Tooltip("�÷��̾ �˹���� ���� �ִ� �ð�(��)")]
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

        // ������ ���� ���� ���� Ȯ�� & ���� ���� ������
        if (GameManager.I != null)
        {
            // ���ݷ� ���
            if (GameManager.I.IsBoughtAttackPower() && !appliedPowerUpgrade)
            {
                damage += 5;
                appliedPowerUpgrade = true;
            }

            // ���� ���� ���
            if (GameManager.I.IsBoughtAttackRange() && !appliedRangeUpgrade)
            {
                attackRadius += 1.5f;
                appliedRangeUpgrade = true;
            }

            // ���� �ӵ� ���
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


        // ���� �Է� �б�
        hInput = Input.GetAxisRaw("Horizontal");


        //���
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
            // ���� ��(���� ��� ��)�� ���� ����
            if (rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
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
        if (isKnockback)
            return;
        if (GetComponent<GrappleLauncher>().isAttached)
            return;


        if (!horizontalLocked)
        {
            // ���� �̵�
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

    private void OnAttack()
    {
        // 1) ��Ÿ�� �˻�
        if (Time.time < lastAttackTime + attackCooldown)
            return;

        // 2) ���� �ִϸ��̼� Ʈ���� (�ִϸ����Ϳ� isAttack Ʈ���Ű� �־�� ��)
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

        // 3) ���������� ������ ���� �ð� ����
        lastAttackTime = Time.time;

        // 4) OverlapCircleAll�� ���� ���� ���� ��� ���� ������
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);

        if (hits.Length == 0)
            return;

        // 5) ���� ����: �� ��Ʈ ���(Enemy��)�� ������ & �˹� ����
        foreach (var hitCollider in hits)
        {
            // 5-1) �ٶ󺸴� ������ ���: (�� ��ġ - �÷��̾� ��ġ).normalized
            Vector2 directionToEnemy = ((Vector2)hitCollider.transform.position - (Vector2)transform.position).normalized;
            directionToEnemy.y = 0f;
            // 5-2) ������ ������ �ֱ�
            //    (�������� hitCollider.GetComponent<Enemy>()?.Damaged(damage) �����θ� �ϼ��� �ٵ�,
            //     ������ Damaged �Ŀ� �˹� ������ ������ ���� ȣ���ϰų�, 
            //     AddForce�� �ɾ� �ݴϴ�.)
            hitCollider.GetComponent<Enemy>()?.Damaged(damage);
            hitCollider.GetComponent<DashBoss>()?.Damaged(damage);
            hitCollider.GetComponent<DoubleJumpBoss>()?.Damaged(damage);
            hitCollider.GetComponent<GrappleBoss>()?.Damaged(damage);
            hitCollider.GetComponent<LastBoss>()?.Damaged(damage);

            // 5-3) ������ ū �˹� �ֱ�
            Rigidbody2D enemyRb = hitCollider.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                // Impulse ���� �ܹ��� ���� ���� �ش�
                enemyRb.AddForce(directionToEnemy * enemyKnockbackForce, ForceMode2D.Impulse);
            }

            // 5-4) �÷��̾�� ���� �˹� �ֱ� (�� �ݴ� ��������)
            //    �� directionToEnemy ������ �����̹Ƿ�, -directionToEnemy�� �÷��̾� ��(�ݴ�)
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
        // ��� ����ϴ� ���� FixedUpdate���� �̵��� skip�ȴ�.
        yield return new WaitForSeconds(playerKnockbackDuration);
        isKnockback = false;
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
        //animator.SetTrigger("isRangedAttack");
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFX.RangeAttack);
        }
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
        // ���� ���� �ð�ȭ (������)
        Gizmos.color = Color.red;
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}