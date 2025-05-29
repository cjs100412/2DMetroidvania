using UnityEngine;
using System.Collections;

[CreateAssetMenu(
    fileName = "PullAndSprayPattern",
    menuName = "BossPatterns/Pull + Upper Spray",
    order = 14)]
public class PullAndSprayPattern : ScriptableObject, IBossPattern, ISpawnPattern
{
    [Header("Distance & Cooldown")]
    public float minDistance = 10f;
    public float maxDistance = 30f;
    public float cooldown = 8f;

    [Header("Pull Settings")]
    public float duration = 2f;   // 끌어당기는 시간
    public float pullForce = 5f;   // 당기는 힘

    [Header("Spray Settings")]
    public GameObject bulletPrefab;
    public int bulletCount = 16;    // 180°를 나눌 분할 수
    public float bulletSpeed = 12f;   // 발사 속도
    public float sprayDelay = 1f;    // 끌어당기기 중 몇 초 뒤에 발사할지

    float lastUsedTime = -Mathf.Infinity;
    Transform spawnPoint;

    public float Cooldown => cooldown;

    private void OnEnable()
    {
        lastUsedTime = -Mathf.Infinity;
    }

    public void SetSpawnPoint(Transform t)
    {
        spawnPoint = t;
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

        // 1) 끌어당기기 애니
        boss.Animator.SetTrigger("Pull");
        yield return null;

        // 2) Pull + Spray 동시 진행
        var prb = player.GetComponent<Rigidbody2D>();
        float elapsed = 0f;
        bool sprayed = false;

        while (elapsed < duration)
        {
            // (2-a) 당기기 힘
            Vector2 dir = (boss.transform.position - player.position).normalized;
            prb.AddForce(new Vector2(dir.x * pullForce, 0f),
                         ForceMode2D.Force);

            // (2-b) sprayDelay 시점에 한 번만 180° 스프레이
            if (!sprayed && elapsed >= sprayDelay)
            {
                sprayed = true;
                FireUpperHalfCircle();
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3) 약간의 여유
        yield return new WaitForSeconds(0.2f);
    }

    private void FireUpperHalfCircle()
    {
        if (spawnPoint == null || bulletPrefab == null) return;

        // 180° 범위, 위쪽을 향해 bulletCount 분할
        for (int i = 0; i < bulletCount; i++)
        {
            float t = (float)i / (bulletCount - 1);
            float angle = Mathf.Lerp(180f, 0f, t);
            float rad = angle * Mathf.Deg2Rad;

            Vector3 pos = spawnPoint.position;
            var proj = Instantiate(bulletPrefab, pos, Quaternion.identity);
            var rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 v = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * bulletSpeed;
                rb.linearVelocity = v;
            }
        }
    }
}
