using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "AoeDropSpellPattern", menuName = "BossPatterns/AOE Drop Spell", order = 14)]
public class AoeDropSpellPattern : ScriptableObject, IBossPattern
{
    [Header("Spell Settings")]
    public GameObject spellPrefab;
    public float dropHeight = 10f;
    public float dropTime = 2f;
    public float explodeRadius = 3f;
    public int damage = 5;

    [Header("Distance & Cooldown")]
    public float minDistance = 0f;
    public float maxDistance = 40f;
    public float cooldown = 4f;

    public GameObject explodeEffect;

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

        // 1) 준비 애니메이션
        boss.Animator.SetTrigger("isAttack");
        yield return new WaitForSeconds(0.5f);

        // 2) 공중에서 생성
        //Vector3 spawnPos = new Vector3(player.transform.position.x,
        //                               boss.transform.position.y + dropHeight,
        //                               boss.transform.position.z);
        // 플레이어 위치에 생성
        Vector3 spawnPos = new Vector3(player.transform.position.x,
                                       player.transform.position.y,
                                       0);
        var go = Instantiate(spellPrefab, spawnPos, Quaternion.identity);

        // 3) 떨어지는 시간 대기
        yield return new WaitForSeconds(dropTime);

        Instantiate(explodeEffect, spawnPos, Quaternion.identity);

        // 4) 착지 폭발 데미지
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            new Vector2(player.transform.position.x, boss.transform.position.y),
            explodeRadius,
            LayerMask.GetMask("Player")
        );
        foreach (var h in hits)
            h.GetComponent<PlayerHealth>()?.Damaged(damage);

        Destroy(go);
        yield return new WaitForSeconds(0.2f);
    }
}