using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

// IBossDeath 인터페이스(생략) 가정
public class DoubleJumpBoss : MonoBehaviour, IBossDeath
{
    [Header("===== 보스 ID 및 벽 ID (GameManager용) =====")]
    public string bossID = "DoubleJumpBoss";                               

    public string wallID = "DoubleJumpBoss_Wall";

    [Header("사망 이펙트 (Particle Prefab)")]
    public GameObject dieEffect;     // Inspector에서 할당해야 함

    private SpriteRenderer spriteRenderer;
    private CinemachineCamera cinemachineCamera;
    public GameObject wall;              // 사망 시 삭제할 벽 오브젝트 (Inspector)

    private Rigidbody2D rb;
    private BossController bossController;
    private GameObject player;
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
    public float moveSpeed = 3f;               // 이동 속도

    [Header("공격 셋팅")]
    public float attackRange = 5f;
    public float attackCooldown = 2f;
    public Transform attackPoint;              // Inspector에서 드래그할 것
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
            // 벽 오브젝트도 함께 제거
            if (wall != null)
            {
                Destroy(wall);
            }
            // 보스 자신 제거
            Destroy(this.gameObject);
            return;
        }

        // 1) Player 태그로 오브젝트 찾기
        var pgo = GameObject.FindWithTag("Player");
        if (pgo != null)
        {
            player = pgo;

            // PlayerInventory, PlayerHealth 컴포넌트 가져오기
            playerInventory = player.GetComponent<PlayerInventory>();
            if (playerInventory == null)
                Debug.LogError($"[{name}] Player 오브젝트에 PlayerInventory 컴포넌트가 없습니다!");

            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null)
                Debug.LogError($"[{name}] Player 오브젝트에 PlayerHealth 컴포넌트가 없습니다!");

            playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement == null)
                Debug.LogError($"[{name}] Player 오브젝트에 PlayerMovement 컴포넌트가 없습니다!");
        }
        else
        {
            Debug.LogError($"[{name}] Awake(): \"Player\" 태그를 가진 오브젝트를 찾을 수 없습니다.");
        }
        animator = GetComponent<Animator>();
        // 2) 스프라이트 렌더러, 리지드바디, BossController 가져오기
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogError($"[{name}] SpriteRenderer 컴포넌트가 이 오브젝트에 없습니다!");

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            Debug.LogError($"[{name}] Rigidbody2D 컴포넌트가 이 오브젝트에 없습니다!");

        bossController = GetComponent<BossController>();
        if (bossController == null)
            Debug.LogError($"[{name}] BossController 컴포넌트가 이 오브젝트에 없습니다!");

        // 3) Cinemachine 카메라 가져오기 (Tag="Cinemachine"인 오브젝트 + CinemachineCamera 컴포넌트)
        var camObj = GameObject.FindWithTag("Cinemachine");
        if (camObj != null)
        {
            cinemachineCamera = camObj.GetComponent<CinemachineCamera>();
            if (cinemachineCamera == null)
                Debug.LogError($"[{name}] \"Cinemachine\" 태그 오브젝트에는 CinemachineCamera 컴포넌트가 없습니다!");
            else
                originalOrthoSize = cinemachineCamera.Lens.OrthographicSize;
        }
        else
        {
            Debug.LogError($"[{name}] Awake(): \"Cinemachine\" 태그를 가진 오브젝트를 찾을 수 없습니다.");
        }

        // 4) 초기에 체력 세팅
        hp = maxHp;

        // 5) attackPoint가 할당되었는지 확인
        if (attackPoint == null)
            Debug.LogError($"[{name}] Inspector에 attackPoint(Transform)가 할당되지 않았습니다!");
    }


    private void Update()
    {
        // 체력이 0 이하이거나 공격 중이면 이동/공격 로직 건너뛰기. 대신 바라보는 방향만 처리
        if (hp <= 0 || isAttacking)
        {
            HandleFacing();
            return;
        }

        // PlayerHealth가 null이면 예외를 막기 위해 바로 리턴
        if (playerHealth == null)
            return;

        // 플레이어 사망 시 아무 동작 안 함
        if (playerHealth.isDead)
            return;

        HandleFacing();
    }


    private void HandleFacing()
    {
        // player가 null인지 다시 한 번 체크
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
        // 죽었으면 이동을 무조건 멈추고 리턴
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // bossController가 바쁘면 리턴
        if (IsBusy)
            return;

        // player가 null이면 리턴
        if (player == null)
            return;

        // 플레이어와의 거리 계산
        Vector2 diff = (Vector2)player.transform.position - (Vector2)transform.position;
        float distSq = diff.sqrMagnitude;
        Vector2 toPlayer = diff.normalized;

        

        // 기존 Y축 속도 보존하면서, 범위 내에서만 X축으로 이동
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
        // 부드러운 파라미터 변화: SetFloat(name, value, dampTime, deltaTime)
        animator.SetFloat("Speed", speed);

        // 공격 범위 내이면 TryAttack()
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
        // 공격 중에는 이동 X
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        animator.SetTrigger("isAttack");
        // 타격 타이밍까지 대기
        yield return new WaitForSeconds(0.5f);

        // 공격 판정: OverlapCircleAll
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

                //플레이어 넉백
                Rigidbody2D playerRb = hit.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    // Impulse 모드로 단번에 힘을 가해 준다
                    playerRb.AddForce(directionToEnemy * playerKnockbackForce, ForceMode2D.Impulse);
                    playerMovement.isKnockback = true;
                    StartCoroutine(playerMovement.ResetPlayerKnockback());
                }
            }
        }
        else
        {
            Debug.LogWarning($"[{name}] PerformAttack: attackPoint가 null이어서 충돌 판정을 하지 못합니다.");
        }

        // 공격 종료
        isAttacking = false;
    }


    // 외부에서 피해량(amount)을 주었을 때 호출
    public void Damaged(int amount)
    {
        if (isDead)
            return;

        hp -= amount;

        // RedFlash 코루틴 실행. spriteRenderer가 null이면 예외 방지.
        if (spriteRenderer != null)
        {
            StartCoroutine(RedFlash());
        }
        else
        {
            Debug.LogWarning($"[{name}] Damaged: spriteRenderer가 null이라 RedFlash를 실행하지 않습니다.");
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
            Debug.LogWarning($"[{name}] Die(): GameManager 인스턴스를 찾을 수 없습니다. 보스/벽 상태가 저장되지 않습니다.");
        }

        // 사망 이펙트
        if (dieEffect != null)
        {
            Instantiate(dieEffect, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($"[{name}] Die(): dieEffect(Prefab)가 Inspector에 할당되지 않았습니다.");
        }

        // 플레이어에게 보상(코인) 지급
        if (playerInventory != null)
            playerInventory.AddCoins(50);
        else
            Debug.LogWarning($"[{name}] Die(): playerInventory가 null이라 코인 지급을 하지 못했습니다.");

        // 카메라 줌인 + 슬로우 모션
        if (cinemachineCamera != null)
            StartCoroutine(DoCameraZoom());
        else
            Debug.LogWarning($"[{name}] Die(): cinemachineCamera가 null이라 카메라 효과를 실행하지 못했습니다.");

        StartCoroutine(DoSlowMotion());

        // wall이 null이 아니면 파괴
        if (wall != null)
        {
            Destroy(wall);
        }
        else
        {
            Debug.LogWarning($"[{name}] Die(): wall(GameObject)가 Inspector에 할당되지 않았습니다.");
        }

        // 일정 시간 뒤 몹 오브젝트 파괴
        float totalDuration = zoomDuration + slowDuration;
        Destroy(gameObject, totalDuration);
    }


    private IEnumerator DoCameraZoom()
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
