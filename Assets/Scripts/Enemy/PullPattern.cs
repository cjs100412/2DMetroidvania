using UnityEngine;
using System.Collections;

[CreateAssetMenu(
    fileName = "PullPattern",
    menuName = "BossPatterns/Pull Towards Boss",
    order = 13)]
public class PullPattern : ScriptableObject, IBossPattern
{
    [Header("Distance & Cooldown")]
    public float minDistance = 10f;
    public float maxDistance = 30f;
    public float cooldown = 6f;

    [Header("Pull Settings")]
    public float duration = 2f;   // 끌어당기는 시간
    public float pullForce = 5f;   // 당기는 힘 (ForceMode2D.Force)

    float lastUsedTime = -Mathf.Infinity;
    public float Cooldown => cooldown;

    private void OnEnable()
    {
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

        // (1) 끌어당기는 애니메이션 트리거
        boss.Animator.SetTrigger("Pull");

        // (2) 플레이어 Rigidbody 참조
        var prb = player.GetComponent<Rigidbody2D>();
        if (prb == null)
            yield break;

        // (3) 지정된 duration 동안 매 프레임 X축으로 힘을 가함
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // 보스 ← 플레이어 방향 (normalized)
            Vector2 dir = (boss.transform.position - player.position).normalized;
            // X축으로만 가하는 힘
            prb.AddForce(new Vector2(dir.x * pullForce, 0f),
                         ForceMode2D.Force);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // (4) 잠깐 대기 후 종료
        yield return new WaitForSeconds(0.2f);
    }
}
