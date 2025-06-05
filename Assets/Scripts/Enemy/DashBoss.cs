using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class DashBoss : MonoBehaviour, IBossDeath, IProjectileSpawner
{
    [Header("===== 보스 ID 및 벽 ID (GameManager용) =====")]
    public string bossID = "DashBoss";
    public string wallID = "DashBoss_Wall";

    [Header("사망 이펙트")]
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

    [Header("카메라 줌인")]
    public float zoomFactor = 0.6f;
    public float zoomDuration = 2f;
    private float originalOrthoSize;

    [Header("슬로우 모션")]
    public float slowTimeScale = 0.3f;
    public float slowDuration = 3f;

    [Header("체력, 데미지")]
    public int maxHp = 100;
    private int hp;
    public int damage = 2;
    public bool isDead = false;

    [Header("움직임")]
    public float moveSpeed = 3f;           // 이동 속도
    public float minDistanceToPlayer = 4f;  // 이보다 가까우면 뒤로
    public float maxDistanceToPlayer = 8f;  // 이보다 멀면 다가감
    public float wanderRadius = 2f;         // 배회 반경
    private Vector2 wanderTarget;

    [Header("공격 셋팅")]
    public Transform attackPoint;
    public float attackRadius = 6f;
    public LayerMask playerLayer;

    public bool IsBusy => bossController != null && bossController.isBusy;
    public bool IsDead => isDead;

    [SerializeField] Transform projectileSpawnPoint;
    public Transform ProjectileSpawnPoint => projectileSpawnPoint;

    private void Awake()
    {
        // 1) 이미 처치된 보스인지 확인해서, 처치되었다면 벽과 자신을 파괴
        if (GameManager.I != null && GameManager.I.IsBossDefeated(bossID))
        {
            if (wall != null) Destroy(wall);
            Destroy(this.gameObject);
            return;
        }

        // 2) 플레이어 오브젝트 찾아 할당
        var pgo = GameObject.FindWithTag("Player");
        if (pgo != null)
        {
            player = pgo.transform;
            playerInventory = pgo.GetComponent<PlayerInventory>();
            playerHealth = pgo.GetComponent<PlayerHealth>();

            if (playerInventory == null)
                Debug.LogError($"[{name}] PlayerInventory 컴포넌트가 없습니다.");
            if (playerHealth == null)
                Debug.LogError($"[{name}] PlayerHealth 컴포넌트가 없습니다.");
        }
        else
        {
            Debug.LogError($"[{name}] Awake(): \"Player\" 태그의 오브젝트를 찾을 수 없습니다.");
        }

        animator = GetComponent<Animator>();
        // 3) 나머지 컴포넌트 캐싱
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogError($"[{name}] SpriteRenderer 컴포넌트가 없습니다.");

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            Debug.LogError($"[{name}] Rigidbody2D 컴포넌트가 없습니다.");

        bossController = GetComponent<BossController>();
        if (bossController == null)
            Debug.LogError($"[{name}] BossController 컴포넌트가 없습니다.");

        var camObj = GameObject.FindWithTag("Cinemachine");
        if (camObj != null)
        {
            cinemachineCamera = camObj.GetComponent<CinemachineCamera>();
            if (cinemachineCamera == null)
                Debug.LogError($"[{name}] \"Cinemachine\" 태그 오브젝트에 CinemachineCamera 컴포넌트가 없습니다.");
            else
                originalOrthoSize = cinemachineCamera.Lens.OrthographicSize;
        }
        else
        {
            Debug.LogError($"[{name}] Awake(): \"Cinemachine\" 태그의 오브젝트를 찾을 수 없습니다.");
        }

        hp = maxHp;
        wanderTarget = transform.position;
    }

    private void Update()
    {
        // 체력이 0 이하라면 Facing 처리만 하고 리턴
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
        // 죽었거나 패턴 실행 중이면 이동 정지
        if (isDead || IsBusy || player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 플레이어와의 거리 계산
        float dist = Vector2.Distance(transform.position, player.position);
        Vector2 dir;

        if (dist < minDistanceToPlayer)
        {
            // 너무 가까우면 뒤로 물러남
            dir = ((Vector2)transform.position - (Vector2)player.position).normalized;
        }
        else if (dist > maxDistanceToPlayer)
        {
            // 너무 멀면 다가감
            dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        }
        else
        {
            // 적정 거리 내에서 배회
            if (Vector2.Distance(transform.position, wanderTarget) < 0.2f)
                ChooseNewWanderTarget();
            dir = (wanderTarget - (Vector2)transform.position).normalized;
        }

        float speed = Mathf.Abs(rb.linearVelocity.x);
        // 부드러운 파라미터 변화: SetFloat(name, value, dampTime, deltaTime)
        animator.SetFloat("Speed", speed);

        // Y축 움직임 비율 조정
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
            Debug.LogWarning($"[{name}] Die(): GameManager 인스턴스를 찾을 수 없습니다.");
        }

        if (dieEffect != null)
            Instantiate(dieEffect, transform.position, Quaternion.identity);

        if (playerInventory != null)
            playerInventory.AddCoins(50);

        // 카메라 줌 및 슬로우 모션
        StartCoroutine(DoCameraZoom());
        StartCoroutine(DoSlowMotion());

        // 벽 파괴
        if (wall != null)
            Destroy(wall);

        // 일정 시간 후 보스 제거
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
        // 공격 범위 시각화 (빨간색)
        Gizmos.color = Color.red;
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
