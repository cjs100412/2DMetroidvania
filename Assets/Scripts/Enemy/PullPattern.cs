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
    public float duration = 2f;   // ������� �ð�
    public float pullForce = 5f;   // ���� �� (ForceMode2D.Force)

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

        // (1) ������� �ִϸ��̼� Ʈ����
        boss.Animator.SetTrigger("Pull");

        // (2) �÷��̾� Rigidbody ����
        var prb = player.GetComponent<Rigidbody2D>();
        if (prb == null)
            yield break;

        // (3) ������ duration ���� �� ������ X������ ���� ����
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // ���� �� �÷��̾� ���� (normalized)
            Vector2 dir = (boss.transform.position - player.position).normalized;
            // X�����θ� ���ϴ� ��
            prb.AddForce(new Vector2(dir.x * pullForce, 0f),
                         ForceMode2D.Force);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // (4) ��� ��� �� ����
        yield return new WaitForSeconds(0.2f);
    }
}
