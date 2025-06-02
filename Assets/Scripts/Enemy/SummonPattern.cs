// SummonPattern.cs
using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "SummonPattern", menuName = "BossPatterns/Summon", order = 15)]
public class SummonPattern : ScriptableObject, ISpawnPattern
{
    [Header("Summon Settings")]
    public GameObject summonPrefab;      // ��ȯ�� ���� ������
    public float cooldown = 5f;          // ���� ��ٿ�
    public int summonCount = 1;          // �� ���� ��ȯ�� �� �ִ� ����
    public float spawnInterval = 0.2f;   // ���� ���� ��ȯ�� �� ����

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
        // Time �������� ��ٿ��� ������ ���� ����
        return Time.time >= lastUsedTime + cooldown;
    }

    public IEnumerator Execute(BossController boss, Transform player)
    {
        lastUsedTime = Time.time;

        // 1) ��ȯ �ִϸ��̼� Ʈ����(�ʿ� ��)
        boss.Animator.SetTrigger("Summon");
        yield return new WaitForSeconds(0.5f);

        // 2) summonCount��ŭ ���������� ��ȯ
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
