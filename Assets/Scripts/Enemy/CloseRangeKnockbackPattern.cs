using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "CloseRangeKnockbackPattern",
                 menuName = "BossPatterns/Close Range Knockback",
                 order = 11)]
public class CloseRangeKnockbackPattern : ScriptableObject, IBossPattern
{
    [Header("Knockback Settings")]
    public float knockbackForce = 500f;
    public float closeDistance = 3f;
    public float cooldown = 4f;

    float lastUsedTime = -Mathf.Infinity;
    public float Cooldown => cooldown;

    public bool CanExecute(BossController boss, Transform player)
    {
        float dist = Vector2.Distance(boss.transform.position, player.position);
        return Time.time >= lastUsedTime + cooldown
            && dist <= closeDistance;
    }

    public IEnumerator Execute(BossController boss, Transform player)
    {
        lastUsedTime = Time.time;
        // 1) 넉백 애니메이션 트리거
        boss.Animator.SetTrigger("Knockback");
        yield return new WaitForSeconds(0.3f);

        // 2) 플레이어 넉백
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 dir = (player.position - boss.transform.position).normalized;
            rb.AddForce(dir * knockbackForce);
        }

        // 3) 후처리 텀
        yield return new WaitForSeconds(0.2f);
    }
}