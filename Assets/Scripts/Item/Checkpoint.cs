using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Tooltip("이 체크포인트가 가진 스폰포인트 이름")]
    public string spawnPointName = "FirstCheckPoint";

    void Reset()
    {
        // 자동으로 Trigger 설정
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;


        // 1) SceneLoader에 다음 부활 씬/스폰 저장
        string zone = SceneManager.GetActiveScene().name;
        SceneLoader.NextZone = zone;
        SceneLoader.NextSpawnPoint = spawnPointName;

        SceneLoader.LastCheckpointZone = zone;
        SceneLoader.LastCheckpointSpawn = spawnPointName;


        // 2) 기존 SaveGame 로직도 호출
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
