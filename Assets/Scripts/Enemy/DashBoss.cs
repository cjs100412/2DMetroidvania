using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class DashBoss : MonoBehaviour, IBossDeath, IProjectileSpawner
{
    [Header("===== ���� ID �� �� ID (GameManager��) =====")]
    public string bossID = "DashBoss";
    public string wallID = "DashBoss_Wall";

    [Header("��� ����Ʈ")]
    public GameObject dieEffect;
    private SpriteRenderer spriteRenderer;
    private CinemachineCamera cinemachineCamera;
    public GameObject wall;

    private Rigidbody2D rb;
    private BossController bossController;
    private Transform player;
    private PlayerInventory playerInventory;
    private PlayerHealth playerHealth;
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
    public float moveSpeed = 3f;           // �̵� �ӵ�
    public float minDistanceToPlayer = 4f;  // �̺��� ������ �ڷ�
    public float maxDistanceToPlayer = 8f;  // �̺��� �ָ� �ٰ���
    public float wanderRadius = 2f;         // ��ȸ �ݰ�
    private Vector2 wanderTarget;

    [Header("���� ����")]
    public Transform attackPoint;
    public float attackRadius = 6f;
    public LayerMask playerLayer;

    public bool IsBusy => bossController != null && bossController.isBusy;
    public bool IsDead => isDead;

    [SerializeField] Transform projectileSpawnPoint;
    public Transform ProjectileSpawnPoint => projectileSpawnPoint;

    private void Awake()
    {
        // 1) �̹� óġ�� �������� Ȯ���ؼ�, óġ�Ǿ��ٸ� ���� �ڽ��� �ı�
        if (GameManager.I != null && GameManager.I.IsBossDefeated(bossID))
        {
            if (wall != null) Destroy(wall);
            Destroy(this.gameObject);
            return;
        }

        // 2) �÷��̾� ������Ʈ ã�� �Ҵ�
        var pgo = GameObject.FindWithTag("Player");
        if (pgo != null)
        {
            player = pgo.transform;
            playerInventory = pgo.GetComponent<PlayerInventory>();
            playerHealth = pgo.GetComponent<PlayerHealth>();

            if (playerInventory == null)
                Debug.LogError($"[{name}] PlayerInventory ������Ʈ�� �����ϴ�.");
            if (playerHealth == null)
                Debug.LogError($"[{name}] PlayerHealth ������Ʈ�� �����ϴ�.");
        }
        else
        {
            Debug.LogError($"[{name}] Awake(): \"Player\" �±��� ������Ʈ�� ã�� �� �����ϴ�.");
        }

        animator = GetComponent<Animator>();
        // 3) ������ ������Ʈ ĳ��
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogError($"[{name}] SpriteRenderer ������Ʈ�� �����ϴ�.");

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            Debug.LogError($"[{name}] Rigidbody2D ������Ʈ�� �����ϴ�.");

        bossController = GetComponent<BossController>();
        if (bossController == null)
            Debug.LogError($"[{name}] BossController ������Ʈ�� �����ϴ�.");

        var camObj = GameObject.FindWithTag("Cinemachine");
        if (camObj != null)
        {
            cinemachineCamera = camObj.GetComponent<CinemachineCamera>();
            if (cinemachineCamera == null)
                Debug.LogError($"[{name}] \"Cinemachine\" �±� ������Ʈ�� CinemachineCamera ������Ʈ�� �����ϴ�.");
            else
                originalOrthoSize = cinemachineCamera.Lens.OrthographicSize;
        }
        else
        {
            Debug.LogError($"[{name}] Awake(): \"Cinemachine\" �±��� ������Ʈ�� ã�� �� �����ϴ�.");
        }

        hp = maxHp;
        wanderTarget = transform.position;
    }

    private void Update()
    {
        // ü���� 0 ���϶�� Facing ó���� �ϰ� ����
        if (hp <= 0)
        {
            HandleFacing();
            return;
        }

        if (playerHealth != null && playerHealth.isDead)
            return;

        if (attackPoint != null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
            foreach (var hit in hits)
            {
                hit.GetComponent<PlayerHealth>()?.Damaged(damage);
            }
        }
        HandleFacing();
    }

    private void FixedUpdate()
    {
        // �׾��ų� ���� ���� ���̸� �̵� ����
        if (isDead || IsBusy || player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // �÷��̾���� �Ÿ� ���
        float dist = Vector2.Distance(transform.position, player.position);
        Vector2 dir;

        if (dist < minDistanceToPlayer)
        {
            // �ʹ� ������ �ڷ� ������
            dir = ((Vector2)transform.position - (Vector2)player.position).normalized;
        }
        else if (dist > maxDistanceToPlayer)
        {
            // �ʹ� �ָ� �ٰ���
            dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        }
        else
        {
            // ���� �Ÿ� ������ ��ȸ
            if (Vector2.Distance(transform.position, wanderTarget) < 0.2f)
                ChooseNewWanderTarget();
            dir = (wanderTarget - (Vector2)transform.position).normalized;
        }

        float speed = Mathf.Abs(rb.linearVelocity.x);
        // �ε巯�� �Ķ���� ��ȭ: SetFloat(name, value, dampTime, deltaTime)
        animator.SetFloat("Speed", speed);

        // Y�� ������ ���� ����
        dir.y *= 0.1f;
        rb.linearVelocity = dir * moveSpeed;
    }

    private void ChooseNewWanderTarget()
    {
        wanderTarget = (Vector2)transform.position + Random.insideUnitCircle * wanderRadius;
    }

    private void HandleFacing()
    {
        if (player == null) return;
        float xScale = player.position.x > transform.position.x ? 3f : -3f;
        Vector3 s = transform.localScale;
        s.x = xScale;
        transform.localScale = s;
    }

    public void Damaged(int amount)
    {
        if (isDead) return;
        hp -= amount;
        StartCoroutine(RedFlash());
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFX.BossDamaged);
        }
        if (hp <= 0) Die();
    }

    private IEnumerator RedFlash()
    {
        Color original = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        if (spriteRenderer != null)
            spriteRenderer.color = original;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        animator.SetTrigger("isDead");
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFX.BossDead);
        }
        if (GameManager.I != null)
        {
            GameManager.I.SetBossDefeated(bossID);
            GameManager.I.SetWallDestroyed(wallID);
        }
        else
        {
            Debug.LogWarning($"[{name}] Die(): GameManager �ν��Ͻ��� ã�� �� �����ϴ�.");
        }

        if (dieEffect != null)
            Instantiate(dieEffect, transform.position, Quaternion.identity);

        if (playerInventory != null)
            playerInventory.AddCoins(50);

        // ī�޶� �� �� ���ο� ���
        StartCoroutine(DoCameraZoom());
        StartCoroutine(DoSlowMotion());

        // �� �ı�
        if (wall != null)
            Destroy(wall);

        // ���� �ð� �� ���� ����
        float totalDuration = zoomDuration * 2 + slowDuration + 0.1f;
        Destroy(gameObject, totalDuration);
    }

    private IEnumerator DoCameraZoom()
    {
        if (cinemachineCamera == null) yield break;

        float targetSize = originalOrthoSize * zoomFactor;
        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            float t = elapsed / zoomDuration;
            cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(originalOrthoSize, targetSize, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        cinemachineCamera.Lens.OrthographicSize = targetSize;

        yield return new WaitForSecondsRealtime(slowDuration);

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

    void OnDrawGizmosSelected()
    {
        // ���� ���� �ð�ȭ (������)
        Gizmos.color = Color.red;
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
