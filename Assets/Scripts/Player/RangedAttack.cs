using UnityEngine;

public class RangedAttack : MonoBehaviour
{
    public LayerMask enemyLayer;  // �ν����Ϳ��� Enemy ���̾ üũ

    public int damage = 10;

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1) �浹�� ������Ʈ�� Enemy ���̾����� üũ
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            // 2) Enemy ������Ʈ�� ������ Damaged ȣ��
            var enemy = other.GetComponent<Enemy>();
            if (enemy != null)
                enemy.Damaged(damage);

            var dashBoss = other.GetComponent<DashBoss>();
            if (dashBoss != null)
                dashBoss.Damaged(damage);

            var doubleJumpBoss = other.GetComponent<DoubleJumpBoss>();
            if (doubleJumpBoss != null)
                doubleJumpBoss.Damaged(damage);
        }

        // 3) ����ü �ı�
        Destroy(gameObject);
    }
}