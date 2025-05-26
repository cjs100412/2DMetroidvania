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

        // 1) Persistent �� �ε� (Additive)
        if (isFirstBoot)
            yield return SceneManager.LoadSceneAsync(persistentSceneName, LoadSceneMode.Additive);

        // 2) Zone ��ȯ
        if (isFirstBoot)
        {
            // ���� �ε�: �ν����Ϳ� ������ initialZoneName ���
            yield return LoadNewZone(initialZoneName, initialSpawnPointName);
            SceneLoader.CurrentZone = initialZoneName;
        }
        else
        {
            // ���� ���� �� �ε�: SceneLoader.NextZone ���
            yield return SceneManager.UnloadSceneAsync(SceneLoader.CurrentZone);

            string nextZone = SceneLoader.NextZone;
            string nextSpawn = SceneLoader.NextSpawnPoint;

            yield return LoadNewZone(nextZone, nextSpawn);
            SceneLoader.CurrentZone = nextZone;

            SceneLoader.NextZone = null;
            SceneLoader.NextSpawnPoint = null;
        }

        // 3) Bootstrap �� ��ε�
        yield return SceneManager.UnloadSceneAsync("Bootstrap");
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
        var spawn = GameObject.Find(spawnPointName);
        if (player != null && spawn != null)
            player.transform.position = spawn.transform.position;
        else
            Debug.LogWarning($"�÷��̾�({player}) �Ǵ� SpawnPoint({spawnPointName})�� ã�� ���߽��ϴ�.");

    }
}