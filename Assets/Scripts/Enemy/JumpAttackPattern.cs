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

    [Header("Jump Settings")]
    public float jumpForce = 12f;   // Y축 점프 세기
    public float horizontalSpeed = 15f;   // X축 이동 속도

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
        lastUsedTime = Time.time;

        // 1) 점프 애니메이션
        boss.Animator.SetTrigger("Jump");
        // 애니메이션 타이밍에 맞춰 약간 대기
        yield return new WaitForSeconds(0.2f);

        // 2) 실제 점프 이동
        var rb = boss.GetComponent<Rigidbody2D>();
        Vector2 dir = (player.position - boss.transform.position).normalized;
        // X축 속도, Y축 점프력 설정
        rb.linearVelocity = new Vector2(dir.x * horizontalSpeed, jumpForce);

        // 3) 착지 대기
        //    보통 점프 후 착지까지 걸리는 시간을 대략 estimate
        Vector2 checkPos;
        yield return new WaitUntil(() =>
        {
            // 보스 발밑 위치 계산
            checkPos = (Vector2)boss.transform.position + groundCheckOffset;
            return Physics2D.OverlapCircle(checkPos, groundCheckRadius, groundLayer) != null;
        });

        // 4) 착지 지점 반경 내 플레이어 데미지
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            boss.transform.position,
            damageRadius,
            LayerMask.GetMask("Player")
        );
        foreach (var hit in hits)
        {
            hit.GetComponent<PlayerHealth>()?.Damaged(damage);
        }

        // 5) 후처리 대기 (약간의 여유)
        yield return new WaitForSeconds(0.2f);
    }
}
