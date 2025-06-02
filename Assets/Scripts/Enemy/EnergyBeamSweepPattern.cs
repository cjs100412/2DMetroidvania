using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "EnergyBeamSweepPattern", menuName = "BossPatterns/Energy Beam Sweep", order = 15)]
public class EnergyBeamSweepPattern : ScriptableObject, IBossPattern
{
    [Header("Beam Settings")]
    public GameObject beamPrefab;
    public float sweepAngle = 90f;
    public float sweepDuration = 2f;

    [Header("Distance & Cooldown")]
    public float minDistance = 0f;
    public float maxDistance = 50f;
    public float cooldown = 5f;

    float lastUsedTime = -Mathf.Infinity;
    public float Cooldown => cooldown;

    private void OnEnable() => lastUsedTime = -Mathf.Infinity;

    public bool CanExecute(BossController boss, Transform player)
    {
        float dist = Vector2.Distance(boss.transform.position, player.position);
        return Time.time >= lastUsedTime + cooldown && dist >= minDistance && dist <= maxDistance;
    }

    public IEnumerator Execute(BossController boss, Transform player)
    {
        lastUsedTime = Time.time;

        // 1) 빔 애니메이션 트리거
        boss.Animator.SetTrigger("Beam");
        yield return new WaitForSeconds(0.5f);

        // 2) 빔 인스턴스 생성
        var beam = Instantiate(beamPrefab, boss.transform.position, Quaternion.identity);
        beam.transform.localScale = new Vector3(4f, 32f, 1f);
        // 3) 스윕 동작
        float elapsed = 0f;
        while (elapsed < sweepDuration)
        {
            float t = elapsed / sweepDuration;
            float angle = -sweepAngle / 2 + sweepAngle * t;
            beam.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(beam);
        yield return new WaitForSeconds(0.2f);
    }
}
