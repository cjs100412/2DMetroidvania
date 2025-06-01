using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class GrappleBoss : MonoBehaviour, IBossDeath, IProjectileSpawner
{
    [Header("===== 보스 ID 및 벽 ID (GameManager용) =====")]
    public string bossID = "GrappleBoss";
    public string wallID = "GrappleBoss_Wall";

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
    public float moveSpeed = 3f;  // 이동 속도

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
        // 1) 이미 처치된 보스인지 확인
        if (GameManager.I != null && GameManager.I.IsBossDefeated(bossID))
        {
            if (wall != null) Destroy(wall);
            Destroy(this.gameObject);
            return;
        }

        // 2) 플레이어 찾기 → 그다음 playerInventory / playerHealth 할당
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
            Debug.LogError($"[{name}] Awake(): \"Player\" 태그 오브젝트를 찾을 수 없습니다.");
        }

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
            Debug.LogError($"[{name}] Awake(): \"Cinemachine\" 태그 오브젝트를 찾을 수 없습니다.");
        }

        hp = maxHp;
    }

    private void Update()
    {
        // 체력이 0 이하라면 Facing만 수행 후 리턴
        if (hp <= 0)
        {
            HandleFacing();
            return;
        }

        // 플레이어가 죽었거나 playerHealth가 null이면 아무 동작 안 함
        if (playerHealth != null && playerHealth.isDead)
            return;

        // 공격 판정: 매 프레임에 범위 내 플레이어가 있으면 피해 주기
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

    private void HandleFacing()
    {
        if (player == null) return;
        float xScale = (player.position.x > transform.position.x) ? 6f : -6f;
        Vector3 s = transform.localScale;
        s.x = xScale;
        transform.localScale = s;
    }

    private void FixedUpdate()
    {
        // 죽었거나 패턴 실행 중이면 이동 정지
        if (isDead || IsBusy || player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 플레이어와 거리 계산
        Vector2 diff = (Vector2)player.position - (Vector2)transform.position;
        Vector2 toPlayer = diff.normalized;

        // Y축 이동은 하지 않음
        toPlayer.y = 0;
        rb.linearVelocity = toPlayer * moveSpeed;
    }

    // 피해 입었을 때 호출
    public void Damaged(int amount)
    {
        if (isDead) return;
        hp -= amount;
        StartCoroutine(RedFlash());

        if (hp <= 0)
            Die();
    }

    private IEnumerator RedFlash()
    {
        if (spriteRenderer == null) yield break;

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

        if (GameManager.I != null)
        {
            GameManager.I.SetBossDefeated(bossID);
            GameManager.I.SetWallDestroyed(wallID);
        }
        else
        {
            Debug.LogWarning($"[{name}] Die(): GameManager가 null이라 보스/벽 상태가 저장되지 않습니다.");
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

        // 일정 시간 뒤 보스 오브젝트 파괴
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
