using UnityEngine;
using System.Collections;

[CreateAssetMenu(
    fileName = "FarRangeProjectilePattern",
    menuName = "BossPatterns/Far Range Shoot",
    order = 10)]
public class FarRangeProjectilePattern : ScriptableObject, IBossPattern
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;

    [Header("Distance & Cooldown")]
    public float minDistance = 15f;
    public float maxDistance = 40f;
    public float cooldown = 1f;

    float lastUsedTime = -Mathf.Infinity;
    public float Cooldown => cooldown;

    private void OnEnable()
    {
        lastUsedTime = -Mathf.Infinity;
    }

    public bool CanExecute(BossController boss, Transform player)
    {
        float dist = Vector2.Distance(boss.transform.position, player.position);
        bool cdOK = Time.time >= lastUsedTime + cooldown;
        bool distOK = dist >= minDistance && dist <= maxDistance;

        Debug.Log(
          $"[Debug][{GetType().Name}] " +
          $"dist={dist:0.00}, " +
          $"minD={minDistance}, maxD={maxDistance}, " +
          $"cdOK={cdOK}, distOK={distOK}, " +
          $"lastUsed={lastUsedTime:0.00}, time={Time.time:0.00}"
        );
        return Time.time >= lastUsedTime + cooldown && dist >= minDistance && dist <= maxDistance;
    }

    public IEnumerator Execute(BossController boss, Transform player)
    {
        lastUsedTime = Time.time;

        // (1) 발사 애니 동기화
        boss.Animator.SetTrigger("Shoot");
        yield return new WaitForSeconds(0.5f);

        // (2) 보스 인스턴스의 SpawnPoint 위치에서 투사체 생성
        if (boss.projectileSpawnPoint == null)
        {
            Debug.LogWarning("BossController.projectileSpawnPoint이 할당되지 않았습니다.");
            yield break;
        }

        Vector3 spawnPos = boss.projectileSpawnPoint.position;
        Quaternion spawnRot = boss.projectileSpawnPoint.rotation;
        var proj = Instantiate(projectilePrefab, spawnPos, spawnRot);

        // (3) Rigidbody2D.velocity로 발사
        var rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 dir = (player.position - spawnPos).normalized;
            rb.linearVelocity = dir * projectileSpeed;
        }

        // (4) 패턴 후처리 대기
        yield return new WaitForSeconds(0.3f);
    }
}