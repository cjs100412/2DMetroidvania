using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "FarRangeProjectilePattern", menuName = "BossPatterns/Far Range Shoot", order = 10)]
public class FarRangeProjectilePattern : ScriptableObject, IBossPattern
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public Transform spawnPoint;
    public float projectileSpeed = 8f;

    [Header("Distance & Cooldown")]
    public float minDistance = 5f;     // 최소 사거리
    public float maxDistance = 20f;    // 최대 사거리
    public float cooldown = 3f;

    float lastUsedTime = -Mathf.Infinity;
    public float Cooldown => cooldown;

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
        // 1) 발사 애니메이션
        boss.Animator.SetTrigger("Shoot");
        yield return new WaitForSeconds(0.5f);  // 애니 타이밍 맞춤

        // 2) 투사체 생성 & 발사
        Vector2 dir = (player.position - boss.transform.position).normalized;
        var proj = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
        proj.GetComponent<Rigidbody2D>().linearVelocity = dir * projectileSpeed;

        // 3) 후처리 텀
        yield return new WaitForSeconds(0.3f);
    }
}