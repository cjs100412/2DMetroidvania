using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

// IBossDeath �������̽�(����) ����
public class DoubleJumpBoss : MonoBehaviour, IBossDeath
{
    [Header("===== ���� ID �� �� ID (GameManager��) =====")]
    public string bossID = "DoubleJumpBoss";                               

    public string wallID = "DoubleJumpBoss_Wall";

    [Header("��� ����Ʈ (Particle Prefab)")]
    public GameObject dieEffect;     // Inspector���� �Ҵ��ؾ� ��

    private SpriteRenderer spriteRenderer;
    private CinemachineCamera cinemachineCamera;
    public GameObject wall;              // ��� �� ������ �� ������Ʈ (Inspector)

    private Rigidbody2D rb;
    private BossController bossController;
    private GameObject player;
    private Animator animator;

    [Header("ī�޶� ����")]
    public float zoomFactor = 0.6f;
    public float zoomDuration = 2f;

    private float originalOrthoSize;

    [Header("���ο� ���")]
    public float slowTimeScale = 0.3f;
    public float slowDuration = 3f;

    [Header("ü��, ������")]
    public int maxHp = 100;
    private int hp;
    public int damage = 2;
    public bool isDead = false;

    [Header("������")]
    public float moveSpeed = 3f;               // �̵� �ӵ�

    [Header("���� ����")]
    public float attackRange = 5f;
    public float attackCooldown = 2f;
    public Transform attackPoint;              // Inspector���� �巡���� ��
    public float attackRadius = 6f;
    public LayerMask playerLayer;

    private PlayerInventory playerInventory;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    public float playerKnockbackForce = 10f;

    private float lastAttackTime;
    private bool isAttacking;

    public bool IsBusy => bossController != null && bossController.isBusy;
    public bool IsDead => isDead;


    private void Awake()
    {
        if (GameManager.I != null && GameManager.I.IsBossDefeated(bossID))
        {
            // �� ������Ʈ�� �Բ� ����
            if (wall != null)
            {
                Destroy(wall);
            }
            // ���� �ڽ� ����
            Destroy(this.gameObject);
            return;
        }

        // 1) Player �±׷� ������Ʈ ã��
        var pgo = GameObject.FindWithTag("Player");
        if (pgo != null)
        {
            player = pgo;

            // PlayerInventory, PlayerHealth ������Ʈ ��������
            playerInventory = player.GetComponent<PlayerInventory>();
            if (playerInventory == null)
                Debug.LogError($"[{name}] Player ������Ʈ�� PlayerInventory ������Ʈ�� �����ϴ�!");

            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null)
                Debug.LogError($"[{name}] Player ������Ʈ�� PlayerHealth ������Ʈ�� �����ϴ�!");

            playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement == null)
                Debug.LogError($"[{name}] Player ������Ʈ�� PlayerMovement ������Ʈ�� �����ϴ�!");
        }
        else
        {
            Debug.LogError($"[{name}] Awake(): \"Player\" �±׸� ���� ������Ʈ�� ã�� �� �����ϴ�.");
        }
        animator = GetComponent<Animator>();
        // 2) ��������Ʈ ������, ������ٵ�, BossController ��������
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogError($"[{name}] SpriteRenderer ������Ʈ�� �� ������Ʈ�� �����ϴ�!");

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            Debug.LogError($"[{name}] Rigidbody2D ������Ʈ�� �� ������Ʈ�� �����ϴ�!");

        bossController = GetComponent<BossController>();
        if (bossController == null)
            Debug.LogError($"[{name}] BossController ������Ʈ�� �� ������Ʈ�� �����ϴ�!");

        // 3) Cinemachine ī�޶� �������� (Tag="Cinemachine"�� ������Ʈ + CinemachineCamera ������Ʈ)
        var camObj = GameObject.FindWithTag("Cinemachine");
        if (camObj != null)
        {
            cinemachineCamera = camObj.GetComponent<CinemachineCamera>();
            if (cinemachineCamera == null)
                Debug.LogError($"[{name}] \"Cinemachine\" �±� ������Ʈ���� CinemachineCamera ������Ʈ�� �����ϴ�!");
            else
                originalOrthoSize = cinemachineCamera.Lens.OrthographicSize;
        }
        else
        {
            Debug.LogError($"[{name}] Awake(): \"Cinemachine\" �±׸� ���� ������Ʈ�� ã�� �� �����ϴ�.");
        }

        // 4) �ʱ⿡ ü�� ����
        hp = maxHp;

        // 5) attackPoint�� �Ҵ�Ǿ����� Ȯ��
        if (attackPoint == null)
            Debug.LogError($"[{name}] Inspector�� attackPoint(Transform)�� �Ҵ���� �ʾҽ��ϴ�!");
    }


    private void Update()
    {
        // ü���� 0 �����̰ų� ���� ���̸� �̵�/���� ���� �ǳʶٱ�. ��� �ٶ󺸴� ���⸸ ó��
        if (hp <= 0 || isAttacking)
        {
            HandleFacing();
            return;
        }

        // PlayerHealth�� null�̸� ���ܸ� ���� ���� �ٷ� ����
        if (playerHealth == null)
            return;

        // �÷��̾� ��� �� �ƹ� ���� �� ��
        if (playerHealth.isDead)
            return;

        HandleFacing();
    }


    private void HandleFacing()
    {
        // player�� null���� �ٽ� �� �� üũ
        if (player != null)
        {
            float scaleX = (player.transform.position.x > transform.position.x) ? 6f : -6f;
            SetScaleX(scaleX);
        }
    }

    private void SetScaleX(float x)
    {
        var s = transform.localScale;
        s.x = x;
        transform.localScale = s;
    }


    private void FixedUpdate()
    {
        // �׾����� �̵��� ������ ���߰� ����
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // bossController�� �ٻڸ� ����
        if (IsBusy)
            return;

        // player�� null�̸� ����
        if (player == null)
            return;

        // �÷��̾���� �Ÿ� ���
        Vector2 diff = (Vector2)player.transform.position - (Vector2)transform.position;
        float distSq = diff.sqrMagnitude;
        Vector2 toPlayer = diff.normalized;

        

        // ���� Y�� �ӵ� �����ϸ鼭, ���� �������� X������ �̵�
        Vector2 vel = rb.linearVelocity;
        if (distSq <= 40f)
        {
            vel.x = toPlayer.x * moveSpeed;
        }
        else
        {
            vel.x = 0f;
        }
        rb.linearVelocity = vel;

        float speed = Mathf.Abs(rb.linearVelocity.x);
        // �ε巯�� �Ķ���� ��ȭ: SetFloat(name, value, dampTime, deltaTime)
        animator.SetFloat("Speed", speed);

        // ���� ���� ���̸� TryAttack()
        if (distSq <= attackRange * attackRange)
        {
            TryAttack();
        }
    }


    private void TryAttack()
    {
        if (isAttacking)
            return;

        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;
        StartCoroutine(PerformAttack());
    }


    private IEnumerator PerformAttack()
    {
        Debug.Log($"[{name}] DoubleJumpBoss TryAttack");

        isAttacking = true;
        // ���� �߿��� �̵� X
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        animator.SetTrigger("isAttack");
        // Ÿ�� Ÿ�ֱ̹��� ���
        yield return new WaitForSeconds(0.5f);

        // ���� ����: OverlapCircleAll
        if (attackPoint != null)
        {

            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
            foreach (var hit in hits)
            {
                Vector2 directionToEnemy = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                directionToEnemy.y = 0f;
                var ph = hit.GetComponent<PlayerHealth>();
                if (ph != null)
                    ph.Damaged(damage);

                //�÷��̾� �˹�
                Rigidbody2D playerRb = hit.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    // Impulse ���� �ܹ��� ���� ���� �ش�
                    playerRb.AddForce(directionToEnemy * playerKnockbackForce, ForceMode2D.Impulse);
                    playerMovement.isKnockback = true;
                    StartCoroutine(playerMovement.ResetPlayerKnockback());
                }
            }
        }
        else
        {
            Debug.LogWarning($"[{name}] PerformAttack: attackPoint�� null�̾ �浹 ������ ���� ���մϴ�.");
        }

        // ���� ����
        isAttacking = false;
    }


    // �ܺο��� ���ط�(amount)�� �־��� �� ȣ��
    public void Damaged(int amount)
    {
        if (isDead)
            return;

        hp -= amount;

        // RedFlash �ڷ�ƾ ����. spriteRenderer�� null�̸� ���� ����.
        if (spriteRenderer != null)
        {
            StartCoroutine(RedFlash());
        }
        else
        {
            Debug.LogWarning($"[{name}] Damaged: spriteRenderer�� null�̶� RedFlash�� �������� �ʽ��ϴ�.");
        }

        if (hp <= 0)
            Die();
    }


    private IEnumerator RedFlash()
    {
        if (spriteRenderer == null)
            yield break;

        Color original = spriteRenderer.color;
        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(0.2f);

        if (spriteRenderer != null)
            spriteRenderer.color = original;
    }


    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        animator.SetTrigger("isDead");
        if (GameManager.I != null)
        {
            GameManager.I.SetBossDefeated(bossID);
            GameManager.I.SetWallDestroyed(wallID);
        }
        else
        {
            Debug.LogWarning($"[{name}] Die(): GameManager �ν��Ͻ��� ã�� �� �����ϴ�. ����/�� ���°� ������� �ʽ��ϴ�.");
        }

        // ��� ����Ʈ
        if (dieEffect != null)
        {
            Instantiate(dieEffect, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($"[{name}] Die(): dieEffect(Prefab)�� Inspector�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        // �÷��̾�� ����(����) ����
        if (playerInventory != null)
            playerInventory.AddCoins(50);
        else
            Debug.LogWarning($"[{name}] Die(): playerInventory�� null�̶� ���� ������ ���� ���߽��ϴ�.");

        // ī�޶� ���� + ���ο� ���
        if (cinemachineCamera != null)
            StartCoroutine(DoCameraZoom());
        else
            Debug.LogWarning($"[{name}] Die(): cinemachineCamera�� null�̶� ī�޶� ȿ���� �������� ���߽��ϴ�.");

        StartCoroutine(DoSlowMotion());

        // wall�� null�� �ƴϸ� �ı�
        if (wall != null)
        {
            Destroy(wall);
        }
        else
        {
            Debug.LogWarning($"[{name}] Die(): wall(GameObject)�� Inspector�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        // ���� �ð� �� �� ������Ʈ �ı�
        float totalDuration = zoomDuration + slowDuration;
        Destroy(gameObject, totalDuration);
    }


    private IEnumerator DoCameraZoom()
    {
        if (cinemachineCamera == null)
            yield break;

        float targetSize = originalOrthoSize * zoomFactor;
        float elapsed = 0f;

        // �� ��
        while (elapsed < zoomDuration)
        {
            float t = elapsed / zoomDuration;
            cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(originalOrthoSize, targetSize, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        cinemachineCamera.Lens.OrthographicSize = targetSize;

        // ���ο� ��� ���� ����
        yield return new WaitForSecondsRealtime(slowDuration);

        // �� �ƿ�
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


    private IEnumerator DoSlowMotion()
    {
        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = 0.02f * slowTimeScale;

        yield return new WaitForSecondsRealtime(slowDuration);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
