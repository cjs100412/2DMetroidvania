using UnityEngine;

public class RangedAttack : MonoBehaviour
{
    public LayerMask enemyLayer;  // �ν����Ϳ��� Enemy ���̾ üũ

    public int damage = 10;

    void OnTriggerEnter2D(Collider2D other)
    {
        // �浹�� ������Ʈ�� Enemy ���̾����� üũ
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            // Enemy ������Ʈ�� ������ Damaged ȣ��
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
        // ����ü �ı�
        Destroy(gameObject);
    }
}