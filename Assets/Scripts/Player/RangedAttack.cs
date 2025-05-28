using UnityEngine;

public class RangedAttack : MonoBehaviour
{
    public LayerMask enemyLayer;  // 인스펙터에서 Enemy 레이어만 체크

    public int damage = 10;

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1) 충돌한 오브젝트가 Enemy 레이어인지 체크
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            // 2) Enemy 컴포넌트가 있으면 Damaged 호출
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

        // 3) 투사체 파괴
        Destroy(gameObject);
    }
}