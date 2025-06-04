using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "JumpAttackPattern",
    menuName = "BossPatterns/Jump Attack (Max 40m)",
    order = 12)]
public class JumpAttackPattern : ScriptableObject, IBossPattern
{
    [Header("Distance & Cooldown")]
    [Tooltip("점프 패턴 최대 거리 (유효 사거리)")]
    public float maxDistance = 40f;
    public float cooldown = 5f;    // 재사용 대기
    public GameObject effect;

    [Header("Jump Speed Tuning")]
    [Tooltip("점프 초기 속도 (상승)")]
    public float jumpForce = 12f;
    [Tooltip("수평 속도 (비행 속도)")]
    public float horizontalSpeed = 15f;
    [Tooltip("점프 도중 적용할 중력 배수 (하강 속도)")]
    public float gravityMultiplier = 3f;

    [Header("Landing Damage")]
    public float damageRadius = 3f;    // 착지 후 넉백 반경
    public int damage = 5;             // 입힐 피해량

    [Header("Ground Check (Pattern Only)")]
    [Tooltip("보스 발밑 기준으로 땅을 감지할 오프셋")]
    public Vector2 groundCheckOffset = new Vector2(0, -3f);
    [Tooltip("땅 감지 반경")]
    public float groundCheckRadius = 0.2f;
    [Tooltip("땅 레이어")]
    public LayerMask groundLayer;

    // 가까이 붙었을 때 수평 이동을 생략하는 임계값
    [Header("최소 수평 이동 임계값")]
    [Tooltip("이 값(유닛)보다 플레이어와 보스 사이 거리가 작으면 수평 이동 없이 수직 점프만 수행")]
    public float minHorizontalThreshold = 0.1f;

    float lastUsedTime = -Mathf.Infinity;
    public float Cooldown => cooldown;

    private void OnEnable()
    {
        // 플레이 모드 진입 시 쿨다운 리셋
        lastUsedTime = -Mathf.Infinity;
    }

    public bool CanExecute(BossController boss, Transform player)
    {
        float dist = Vector2.Distance(boss.transform.position, player.position);
        // 쿨다운이 돌았고 최대 사거리 이내에 있을 때만 실행
        return Time.time >= lastUsedTime + cooldown
            && dist <= maxDistance;
    }

    public IEnumerator Execute(BossController boss, Transform player)
    {
        boss.isBusy = true;
        lastUsedTime = Time.time;

        // 1) 점프 애니메이션
        boss.Animator.SetTrigger("Jump");
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFX.JumpAttack);
        }
        yield return new WaitForSeconds(0.1f);

        // 2) 점프 실행
        var rb = boss.GetComponent<Rigidbody2D>();
        float originalGravity = rb.gravityScale;
        rb.gravityScale = originalGravity * gravityMultiplier;

        // 플레이어 현재 위치 방향으로 수평 속도 설정하되,
        // 거리가 minHorizontalThreshold보다 작으면 수평 성분 0 처리
        Vector2 toPlayer = player.position - boss.transform.position;
        float distance = toPlayer.magnitude;
        float dirX = 0f;
        if (distance > minHorizontalThreshold)
        {
            dirX = toPlayer.normalized.x;
        }
        rb.linearVelocity = new Vector2(dirX * horizontalSpeed, jumpForce);

        // 물리 엔진 적용을 위해 한 프레임 대기
        yield return new WaitForFixedUpdate();

        // 3) 착지 대기
        bool landed = false;
        float timer = 0f, maxWait = 2f;
        while (timer < maxWait)
        {
            Vector2 checkPos = (Vector2)boss.transform.position + groundCheckOffset;
            if (Physics2D.OverlapCircle(checkPos, groundCheckRadius, groundLayer) != null)
            {
                landed = true;
                break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        if (!landed)
        {
            // 실제 착지까지 대기
            do { yield return null; }
            while (Physics2D.OverlapCircle((Vector2)boss.transform.position + groundCheckOffset, groundCheckRadius, groundLayer) == null);
        }
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFX.JumpAttack);
        }

        // 4) 착지 순간 데미지
        Collider2D[] hits = Physics2D.OverlapCircleAll(boss.transform.position, damageRadius, LayerMask.GetMask("Player"));
        foreach (var hit in hits)
        {
            hit.GetComponent<PlayerHealth>()?.Damaged(damage);
        }
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // 5) 이펙트 생성
        if (effect != null)
        {
            Instantiate(effect, boss.transform.position, Quaternion.identity);
        }

        // 6) 중력 복구 및 마무리 대기
        rb.gravityScale = originalGravity;
        yield return new WaitForSeconds(0.1f);

        boss.isBusy = false;
    }

}
