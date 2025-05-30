using UnityEngine;

public class EnergyBeam : MonoBehaviour
{
    public int damage = 10;            // 원하는 데미지
    public LayerMask hitLayer;        // 보통 LayerMask.GetMask("Player") 세팅

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어 레이어만 필터링
        if (((1 << other.gameObject.layer) & hitLayer) == 0) return;

        var ph = other.GetComponent<PlayerHealth>();
        if (ph != null)
            ph.Damaged(damage);
    }
}
