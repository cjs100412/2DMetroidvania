using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class GrappleBoss : MonoBehaviour, IBossDeath, IProjectileSpawner
{
    public ParticleSystem dieEffect;
    private SpriteRenderer spriteRenderer;
    CinemachineCamera cinemachineCamera;
    public GameObject wall;

    private Rigidbody2D rb;
    private BossController bossController;
    private GameObject player;

    [Header("===== 보스 ID 및 벽 ID (GameManager용) =====")]
    public string bossID = "GrappleBoss";

    public string wallID = "GrappleBoss_Wall";

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

    private PlayerHealth playerHealth;
    private PlayerInventory playerInventory;

    public bool IsBusy => bossController != null && bossController.isBusy;
    public bool IsDead => isDead;

    [SerializeField] Transform projectileSpawnPoint;
    public Transform ProjectileSpawnPoint => projectileSpawnPoint;

    private void Awake()
    {
        if (GameManager.I != null && GameManager.I.IsBossDefeated(bossID))
        {
            if (wall != null) Destroy(wall);
            Destroy(this.gameObject);
            return;
        }

        playerInventory = player.GetComponent<PlayerInventory>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        bossController = GetComponent<BossController>();
        cinemachineCamera = GameObject.FindWithTag("Cinemachine").GetComponent<CinemachineCamera>();
        hp = maxHp;

        var pgo = GameObject.FindWithTag("Player");
        if (pgo != null) player = pgo;
        else Debug.LogError("Player 태그 오브젝트가 없습니다.");

        if (cinemachineCamera != null)
            originalOrthoSize = cinemachineCamera.Lens.OrthographicSize;
        else
            Debug.LogError("Cinemachine Camera가 할당되지 않았습니다.");

        playerHealth = player.GetComponent<PlayerHealth>();

    }

    private void Update()
    {
        // 체력이 0 이하거나 공격 중이라면 상태 전환 로직을 건너뛰고 방향 처리만 수행
        if (hp <= 0)
        {
            HandleFacing();
            return;
        }

        // 플레이어가 사망했으면 아무 동작도 하지 않음
        if (playerHealth.isDead)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
        foreach (var hit in hits)
            hit.GetComponent<PlayerHealth>()?.Damaged(damage);

        HandleFacing();
    }

    private void HandleFacing()
    {
        SetScaleX(player.transform.position.x > transform.position.x ? 6f : -6f);

    }

    private void SetScaleX(float x)
    {
        Vector3 s = transform.localScale;
        s.x = x;
        transform.localScale = s;
    }

    void FixedUpdate()
    {
        // 죽으면 이동 정지
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (IsBusy)
            return;

        // 플레이어와 거리 계산
        Vector2 diff = (Vector2)player.transform.position - (Vector2)transform.position;
        float distSq = diff.sqrMagnitude;
        Vector2 toPlayer = diff.normalized;

        // 그 방향으로 속도 설정

        toPlayer.y = 0; // y축은 이동하지 않는다
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
        Color original = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
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
        playerInventory.AddCoins(50);

        // 카메라 줌 및 슬로우 모션 시작
        StartCoroutine(DoCameraZoom());
        StartCoroutine(DoSlowMotion());

        // 모든 효과가 끝난 후 아이템 제거
        float totalDuration = zoomDuration * 2 + slowDuration + 0.1f;
        Destroy(gameObject, totalDuration);
        Destroy(wall);
    }


    IEnumerator DoCameraZoom()
    {
        if (cinemachineCamera == null)
            yield break;

        float targetSize = originalOrthoSize * zoomFactor;
        float elapsed = 0f;

        // 줌 인
        while (elapsed < zoomDuration)
        {
            float t = elapsed / zoomDuration;
            cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(originalOrthoSize, targetSize, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        cinemachineCamera.Lens.OrthographicSize = targetSize;

        // 슬로우 모션 동안 유지
        yield return new WaitForSecondsRealtime(slowDuration);

        // 줌 아웃
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


    IEnumerator DoSlowMotion()
    {
        // 슬로우 모션 시작
        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = 0.02f * slowTimeScale;

        // 실제 시간으로 기다림
        yield return new WaitForSecondsRealtime(slowDuration);

        // 시간 복구
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
