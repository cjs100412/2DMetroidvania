using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Tooltip("�� üũ����Ʈ�� ���� ��������Ʈ �̸�")]
    public string spawnPointName = "FirstCheckPoint";

    void Reset()
    {
        // �ڵ����� Trigger ����
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;


        // 1) SceneLoader�� ���� ��Ȱ ��/���� ����
        string zone = SceneManager.GetActiveScene().name;
        SceneLoader.NextZone = zone;
        SceneLoader.NextSpawnPoint = spawnPointName;

        SceneLoader.LastCheckpointZone = zone;
        SceneLoader.LastCheckpointSpawn = spawnPointName;


        // 2) ���� SaveGame ������ ȣ��
        var health = other.GetComponent<PlayerHealth>();
        health.currentHp = health.maxHp;
        health.currentMp = health.maxMp;
        var inv = other.GetComponent<PlayerInventory>();
        GameManager.I.SaveGame(
            other.transform.position,
            health.currentHp,
            health.currentMp,
            inv.CoinCount
        );

        Debug.Log($"[Checkpoint] Saved zone={zone}, spawn={spawnPointName}");
    }
}
