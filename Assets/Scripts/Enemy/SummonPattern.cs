// SummonPattern.cs
using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "SummonPattern", menuName = "BossPatterns/Summon", order = 15)]
public class SummonPattern : ScriptableObject, ISpawnPattern
{
    [Header("Summon Settings")]
    public GameObject summonPrefab;      // 소환할 몬스터 프리팹
    public float cooldown = 5f;          // 패턴 쿨다운
    public int summonCount = 1;          // 한 번에 소환할 수 있는 개수
    public float spawnInterval = 0.2f;   // 여러 개를 소환할 때 간격

    private Transform spawnPoint;
    private float lastUsedTime = -Mathf.Infinity;
    public float Cooldown => cooldown;

    private void OnEnable()
    {
        lastUsedTime = -Mathf.Infinity;
    }

    public void SetSpawnPoint(Transform sp)
    {
        spawnPoint = sp;
    }

    public bool CanExecute(BossController boss, Transform player)
    {
        // Time 기준으로 쿨다운이 지나야 실행 가능
        return Time.time >= lastUsedTime + cooldown;
    }

    public IEnumerator Execute(BossController boss, Transform player)
    {
        lastUsedTime = Time.time;

        // 1) 소환 애니메이션 트리거(필요 시)
        boss.Animator.SetTrigger("Summon");
        yield return new WaitForSeconds(0.5f);

        // 2) summonCount만큼 순차적으로 소환
        for (int i = 0; i < summonCount; i++)
        {
            if (spawnPoint == null || summonPrefab == null)
                yield break;

            Instantiate(summonPrefab, spawnPoint.position, Quaternion.identity);
            yield return new WaitForSeconds(spawnInterval);
        }

        yield return new WaitForSeconds(0.2f);
    }
}
