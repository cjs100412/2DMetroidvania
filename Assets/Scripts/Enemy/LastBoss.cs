using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class LastBoss : MonoBehaviour, IBossDeath, IProjectileSpawner
{
    public ParticleSystem dieEffect;
    private SpriteRenderer spriteRenderer;
    CinemachineCamera cinemachineCamera;
    public GameObject wall;

    private Rigidbody2D rb;
    private BossController bossController;
    private GameObject player;

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
    public float minDistanceToPlayer = 4f;  // 이보다 가까우면 뒤로
    public float maxDistanceToPlayer = 8f;  // 이보다 멀면 다가감
    public float wanderRadius = 2f;  // 적정 거리 내 배회 반경
    private Vector2 wanderTarget;

    private PlayerHealth playerHealth;

    public bool IsBusy => bossController != null && bossController.isBusy;
    public bool IsDead => isDead;

    [SerializeField] Transform projectileSpawnPoint;
    public Transform ProjectileSpawnPoint => projectileSpawnPoint;

    private void Awake()
    {

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

        HandleFacing();
    }

    private void HandleFacing()
    {
        SetScaleX(player.transform.position.x > transform.position.x ? 2f : -2f);

    }

    private void SetScaleX(float x)
    {
        Vector3 s = transform.localScale;
        s.x = x;
        transform.localScale = s;
    }

    void FixedUpdate()
    {
        // 죽었거나 패턴 실행 중이면 이동 정지
        if (isDead || IsBusy)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 플레이어와 거리 계산
        float dist = Vector2.Distance(transform.position, player.transform.position);
        Vector2 dir;

        if (dist < minDistanceToPlayer)
        {
            // 너무 가까워서 뒤로
            dir = ((Vector2)transform.position - (Vector2)player.transform.position).normalized;
        }
        else if (dist > maxDistanceToPlayer)
        {
            // 너무 멀어서 다가감
            dir = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
        }
        else
        {
            // 적정 거리 내에서 배회
            if (Vector2.Distance(transform.position, wanderTarget) < 0.2f)
                ChooseNewWanderTarget();
            dir = (wanderTarget - (Vector2)transform.position).normalized;
        }

        rb.linearVelocity = dir * moveSpeed;
    }
    private void ChooseNewWanderTarget()
    {
        // 현재 위치 기준 반경 내 랜덤 지점 선택
        wanderTarget = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * wanderRadius;
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
        isDead = true;
        if (dieEffect != null)
            Instantiate(dieEffect, transform.position, Quaternion.identity);

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
}
