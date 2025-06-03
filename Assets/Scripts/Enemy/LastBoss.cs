using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class LastBoss : MonoBehaviour, IBossDeath, IProjectileSpawner
{
    [Header("===== 보스 ID 및 벽 ID (GameManager용) =====")]
    public string bossID = "LastBoss";
    public string wallID = "LastBoss_Wall";

    [Header("사망 이펙트")]
    public GameObject dieEffect;
    private SpriteRenderer spriteRenderer;
    private CinemachineCamera cinemachineCamera;
    public GameObject wall;

    private Rigidbody2D rb;
    private BossController bossController;
    private Transform player;
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
    public float minDistanceToPlayer = 4f; // 이보다 가까우면 뒤로 움직임
    public float maxDistanceToPlayer = 8f; // 이보다 멀면 다가감
    public float wanderRadius = 2f;        // 배회 반경
    private Vector2 wanderTarget;

    public bool IsBusy => bossController != null && bossController.isBusy;
    public bool IsDead => isDead;

    [SerializeField] Transform projectileSpawnPoint;
    public Transform ProjectileSpawnPoint => projectileSpawnPoint;

    private void Awake()
    {
        // 1) 이미 처치된 보스인지 확인 → 처치되었다면 벽과 자신을 파괴하고 종료
        if (GameManager.I != null && GameManager.I.IsBossDefeated(bossID))
        {
            if (wall != null) Destroy(wall);
            Destroy(this.gameObject);
            return;
        }

        // 2) Player 오브젝트 찾기 → 그다음 playerHealth 할당
        var pgo = GameObject.FindWithTag("Player");
        if (pgo != null)
        {
            player = pgo.transform;
            playerHealth = pgo.GetComponent<PlayerHealth>();
            if (playerHealth == null)
                Debug.LogError($"[{name}] PlayerHealth 컴포넌트가 없습니다.");
        }
        else
        {
            Debug.LogError($"[{name}] Awake(): \"Player\" 태그 오브젝트를 찾을 수 없습니다.");
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
            Debug.LogError($"[{name}] Awake(): \"Cinemachine\" 태그 오브젝트를 찾을 수 없습니다.");
        }

        hp = maxHp;
        wanderTarget = transform.position;
    }

    private void Update()
    {
        // 체력이 0 이하이면 Facing 처리만 하고 리턴
        if (hp <= 0)
        {
            HandleFacing();
            return;
        }

        // 플레이어가 죽었으면 아무 동작 안 함
        if (playerHealth != null && playerHealth.isDead)
            return;

        HandleFacing();
    }

    private void HandleFacing()
    {
        if (player == null) return;
        float xScale = (player.position.x > transform.position.x) ? 2f : -2f;
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

        // 플레이어와의 거리 계산
        float dist = Vector2.Distance(transform.position, player.position);
        Vector2 dir;

        if (dist < minDistanceToPlayer)
        {
            // 너무 가까워서 뒤로 물러남
            dir = ((Vector2)transform.position - (Vector2)player.position).normalized;
        }
        else if (dist > maxDistanceToPlayer)
        {
            // 너무 멀어서 다가감
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

        // Y축 이동을 0으로 고정하고, X축으로만 이동
        dir.y = 0;
        rb.linearVelocity = dir * moveSpeed;
    }

    private void ChooseNewWanderTarget()
    {
        wanderTarget = (Vector2)transform.position + Random.insideUnitCircle * wanderRadius;
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
        animator.SetTrigger("isDead");
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

        // 카메라 줌 및 슬로우 모션 시작
        StartCoroutine(DoCameraZoom());
        StartCoroutine(DoSlowMotion());

        // 벽 오브젝트 파괴
        if (wall != null)
            Destroy(wall);

        // 일정 시간 뒤 보스 오브젝트 삭제
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

}
