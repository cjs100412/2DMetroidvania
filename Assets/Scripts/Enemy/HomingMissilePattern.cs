using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "HomingMissilePattern", menuName = "BossPatterns/Homing Missile", order = 13)]
public class HomingMissilePattern : ScriptableObject, ISpawnPattern
{
    [Header("Missile Settings")]
    public GameObject missilePrefab;
    public float missileSpeed = 8f;
    public float turnSpeed = 200f;
    public int missileCount = 3;
    public float spawnInterval = 0.2f;

    [Header("Distance & Cooldown")]
    public float minDistance = 10f;
    public float maxDistance = 50f;
    public float cooldown = 3f;

    private Transform spawnPoint;
    float lastUsedTime = -Mathf.Infinity;
    public float Cooldown => cooldown;

    private void OnEnable() => lastUsedTime = -Mathf.Infinity;
    public void SetSpawnPoint(Transform sp) => spawnPoint = sp;

    public bool CanExecute(BossController boss, Transform player)
    {
        float dist = Vector2.Distance(boss.transform.position, player.position);
        return Time.time >= lastUsedTime + cooldown && dist >= minDistance && dist <= maxDistance;
    }

    public IEnumerator Execute(BossController boss, Transform player)
    {
        lastUsedTime = Time.time;

        // 1) 발사 애니메이션
        boss.Animator.SetTrigger("LaunchMissiles");
        yield return new WaitForSeconds(0.3f);

        // 2) 미사일 연속 생성
        for (int i = 0; i < missileCount; i++)
        {
            if (spawnPoint == null) yield break;
            var proj = Instantiate(missilePrefab, spawnPoint.position, spawnPoint.rotation);
            // HomingProjectile 스크립트가 달려 있어야 합니다
            var homing = proj.GetComponent<HomingProjectile>();
            if (homing != null)
            {
                homing.Init(player, missileSpeed, turnSpeed);
            }
            yield return new WaitForSeconds(spawnInterval);
        }

        yield return new WaitForSeconds(0.2f);
    }
}
