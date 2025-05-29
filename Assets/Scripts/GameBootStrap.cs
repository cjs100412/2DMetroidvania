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
        bool isFirstBoot = string.IsNullOrEmpty(SceneLoader.CurrentZone);

        // 1) Persistent �� �ε�
        if (isFirstBoot)
            yield return SceneManager.LoadSceneAsync(persistentSceneName, LoadSceneMode.Additive);

        // 2) Zone ��ȯ
        if (isFirstBoot)
        {
            // ���� ����
            yield return LoadNewZone(initialZoneName, initialSpawnPointName);
            SceneLoader.CurrentZone = initialZoneName;
        }
        else
        {
            // (A) ���� Zone ��ε�
            string prev = SceneLoader.CurrentZone;
            if (!string.IsNullOrEmpty(prev))
            {
                var sc = SceneManager.GetSceneByName(prev);
                if (sc.isLoaded)
                    yield return SceneManager.UnloadSceneAsync(prev);
            }

            // (B) ���� Zone ���� (üũ����Ʈ ������ prevZone �� StartScene)
            string nextZone = SceneLoader.NextZone;
            if (string.IsNullOrEmpty(nextZone))
            {
                Debug.LogWarning($"NextZone ������� �� CurrentZone({prev}) ����");
                nextZone = prev;
            }

            // (C) ���� SpawnPoint ���� (������ StartSpawn)
            string nextSpawn = SceneLoader.NextSpawnPoint;
            if (string.IsNullOrEmpty(nextSpawn))
            {
                Debug.LogWarning($"NextSpawnPoint ������� �� initialSpawnPointName({initialSpawnPointName}) ���");
                nextSpawn = initialSpawnPointName;
            }

            // (D) ���� �ε�
            yield return LoadNewZone(nextZone, nextSpawn);

            SceneLoader.CurrentZone = nextZone;
            SceneLoader.NextZone = null;
            SceneLoader.NextSpawnPoint = null;
        }

        // 3) Bootstrap ��ε�
        yield return SceneManager.UnloadSceneAsync(gameObject.scene.name);
    }

    private IEnumerator LoadNewZone(string zoneName, string spawnPointName)
    {
        if (string.IsNullOrEmpty(zoneName))
        {
            Debug.LogError($"LoadNewZone: zoneName�� ��� �ֽ��ϴ�!");
            yield break;
        }

        // Additive �ε�
        yield return SceneManager.LoadSceneAsync(zoneName, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(zoneName));

        // �÷��̾� ��ġ �̵�
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var spawn = GameObject.Find(spawnPointName);
            if (spawn != null)
                player.transform.position = spawn.transform.position;
            else
                Debug.LogWarning($"SpawnPoint[{spawnPointName}] �� ã��. ��ġ ���� ��ŵ.");
        }

    }
}