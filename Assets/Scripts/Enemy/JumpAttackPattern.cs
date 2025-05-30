using UnityEngine;
using System.Collections;

[CreateAssetMenu(
    fileName = "JumpAttackPattern",
    menuName = "BossPatterns/Jump Attack",
    order = 12)]
public class JumpAttackPattern : ScriptableObject, IBossPattern
{
    [Header("Distance & Cooldown")]
    public float minDistance = 20f;   // 점프 패턴 최소 거리
    public float maxDistance = 40f;   // 점프 패턴 최대 거리
    public float cooldown = 5f;    // 재사용 대기

    [Header("Jump Speed Tuning")]
    [Tooltip("점프 초기 속도 (상승)")]
    public float jumpForce = 12f;
    [Tooltip("수평 속도 (비행 속도)")]
    public float horizontalSpeed = 15f;
    [Tooltip("점프 도중 적용할 중력 배수 (하강 속도)")]
    public float gravityMultiplier = 3f;

    [Header("Landing Damage")]
    public float damageRadius = 3f;    // 착지 후 넉백 반경
    public int damage = 5;    // 입힐 피해량

    [Header("Ground Check (Pattern Only)")]
    [Tooltip("보스 발밑 기준으로 땅을 감지할 오프셋")]
    public Vector2 groundCheckOffset = new Vector2(0, -3f);
    [Tooltip("땅 감지 반경")]
    public float groundCheckRadius = 0.2f;
    [Tooltip("땅 레이어")]
    public LayerMask groundLayer;


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
        return Time.time >= lastUsedTime + cooldown
            && dist >= minDistance
            && dist <= maxDistance;
    }

    public IEnumerator Execute(BossController boss, Transform player)
    {
        boss.isBusy = true;
        lastUsedTime = Time.time;

        // 1) 점프 애니메이션
        boss.Animator.SetTrigger("Jump");
        yield return new WaitForSeconds(0.1f); // 절반으로 줄여서 반응 빠르게

        // 2) 점프 실행
        var rb = boss.GetComponent<Rigidbody2D>();
        float originalGravity = rb.gravityScale;
        rb.gravityScale = originalGravity * gravityMultiplier;   // 중력 크게

        Vector2 dir = (player.position - boss.transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * horizontalSpeed, jumpForce);

        // 살짝 기다려서 물리 엔진 반영
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

        // 4) 착지 순간 데미지
        Collider2D[] hits = Physics2D.OverlapCircleAll(boss.transform.position, damageRadius, LayerMask.GetMask("Player"));
        foreach (var hit in hits)
            hit.GetComponent<PlayerHealth>()?.Damaged(damage);
        Debug.Log("착지");
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // 5) 클린업
        rb.gravityScale = originalGravity;     // 중력 복구
        yield return new WaitForSeconds(0.1f); // 여유 짧게

        boss.isBusy = false;
    }
}
