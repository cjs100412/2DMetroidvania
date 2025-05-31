using UnityEngine;

public class BossBarrier : MonoBehaviour
{
    [Tooltip("����� ������ bossID�� ������ ���ξ�� ���� (��: Level1_BossA_Wall)")]
    public string wallID = "Level1_BossA_Wall";

    void Start()
    {
        // ���� �ε�� ��, �̹� �ı��� ���̶�� ������Ʈ�� �ı�
        if (GameManager.I.IsWallDestroyed(wallID))
        {
            Destroy(this.gameObject);
            return;
        }
        // �ƴϸ� ���ó�� ���� ���� �ְ� ��
    }
}