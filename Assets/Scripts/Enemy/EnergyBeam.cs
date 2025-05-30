using UnityEngine;

public class EnergyBeam : MonoBehaviour
{
    public int damage = 10;            // ���ϴ� ������
    public LayerMask hitLayer;        // ���� LayerMask.GetMask("Player") ����

    private void OnTriggerEnter2D(Collider2D other)
    {
        // �÷��̾� ���̾ ���͸�
        if (((1 << other.gameObject.layer) & hitLayer) == 0) return;

        var ph = other.GetComponent<PlayerHealth>();
        if (ph != null)
            ph.Damaged(damage);
    }
}
