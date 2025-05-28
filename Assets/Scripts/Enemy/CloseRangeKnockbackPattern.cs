using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "CloseRangeKnockbackPattern",
                 menuName = "BossPatterns/Close Range Knockback",
                 order = 11)]
public class CloseRangeKnockbackPattern : ScriptableObject, IBossPattern
{
    [Header("Knockback Settings")]
    public float knockbackForce = 100f;
    public float closeDistance = 15f;
    public float cooldown = 4f;

    float lastUsedTime = -Mathf.Infinity;
    public float Cooldown => cooldown;

    private void OnEnable()
    {
        lastUsedTime = -Mathf.Infinity;
    }

    public bool CanExecute(BossController boss, Transform player)
    {
        float dist = Vector2.Distance(boss.transform.position, player.position);
        return Time.time >= lastUsedTime + cooldown && dist <= closeDistance;
    }



    public IEnumerator Execute(BossController boss, Transform player)
    {
        lastUsedTime = Time.time;
        // 1) �˹� �ִϸ��̼� Ʈ����
        //boss.Animator.SetTrigger("Knockback");
        yield return new WaitForSeconds(0.3f);

        // 2) �÷��̾� �˹�
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 dir = (player.position - boss.transform.position).normalized;
            rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            player.gameObject.GetComponent<PlayerMovement>().isKnockback = true;
            Debug.Log("�˹�");
        }

        // 3) ��ó�� ��
        yield return new WaitForSeconds(0.2f);
        player.gameObject.GetComponent<PlayerMovement>().isKnockback = false;

    }
}