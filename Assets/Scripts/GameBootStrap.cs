using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameBootstrap : MonoBehaviour
{
    [Header("�׻� �ѵ� Persistent ��")]
    public string persistentSceneName = "Persistent";

    [Header("���� �ε��� ��(Zone) ��")]
    public string initialZoneName = "StartScene";
    [Header("���� �ε��� ���� ����Ʈ �̸�")]
    public string initialSpawnPointName = "StartSpawn";

    private IEnumerator Start()
    {
        // 1) Persistent ���� �� ���� �ҷ�����
        if(GameManager.I == null)
        {
            yield return SceneManager.LoadSceneAsync(persistentSceneName, LoadSceneMode.Additive);
        }

        // 2) ���� Zone ��ε� (�� �� �ƴ� ��)
        if (!string.IsNullOrEmpty(SceneLoader.CurrentZone))
        {
            Scene prev = SceneManager.GetSceneByName(SceneLoader.CurrentZone);
            if (prev.IsValid() && prev.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(prev);
            }
        }

        // 3) ���� Zone/Spawn ���� (üũ����Ʈ ������ �ʱⰪ ���)
        string nextZone = string.IsNullOrEmpty(SceneLoader.NextZone)
            ? initialZoneName
            : SceneLoader.NextZone;
        string nextSpawn = string.IsNullOrEmpty(SceneLoader.NextSpawnPoint)
            ? initialSpawnPointName
            : SceneLoader.NextSpawnPoint;

        // 4) �� Zone �ε� & ����
        yield return LoadNewZone(nextZone, nextSpawn);

        // 5) ���� Ŭ����
        SceneLoader.CurrentZone = nextZone;
        SceneLoader.NextZone = null;
        SceneLoader.NextSpawnPoint = null;

        // 6) Bootstrap ��ε�
        yield return SceneManager.UnloadSceneAsync(gameObject.scene.name);
    }


    private IEnumerator LoadNewZone(string zoneName, string spawnPointName)
    {
        if (string.IsNullOrEmpty(zoneName))
        {
            Debug.LogError($"LoadNewZone: zoneName�� ��� �ֽ��ϴ�!");
            yield break;
        }

        // 1) ��� Zone�� Additive ���� �ε�
        yield return SceneManager.LoadSceneAsync(zoneName, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(zoneName));

        // 2) �÷��̾� ��ġ �̵�
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var spawn = GameObject.Find(spawnPointName);
            if (spawn != null)
            {
                // 2-1) ��ġ ����
                player.transform.position = spawn.transform.position;

                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.linearVelocity = Vector2.zero;

                //var ph = player.GetComponent<PlayerHealth>();
                //if (ph != null)
                //    ph.Respawn(spawn.transform.position, ph.maxHp, ph.maxMp);

                var inv = player.GetComponent<PlayerInventory>();
                if (inv != null)
                {

                    inv.CoinCount = 0;
                    inv.OnCoinChanged?.Invoke(0);

                    // ����� �ֽ� ���� ����ŭ �ٽ� �߰�
                    inv.AddCoins(GameManager.I.SavedCoins);
                }
            }
            else
            {
                Debug.LogWarning($"SpawnPoint[{spawnPointName}] �� ã��. ��ġ ���� ��ŵ.");
            }
        }
    }

}