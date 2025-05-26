using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameBootstrap : MonoBehaviour
{
    [Header("항상 켜둘 Persistent 씬")]
    public string persistentSceneName = "Persistent";

    [Header("최초 로드할 맵(Zone) 씬")]
    public string initialZoneName = "StartScene";
    [Header("최초 로드할 스폰 포인트 이름")]
    public string initialSpawnPointName = "StartSpawn";

    private IEnumerator Start()
    {
        bool isFirstBoot = string.IsNullOrEmpty(SceneLoader.CurrentZone);

        // 1) Persistent 씬 로드 (Additive)
        if (isFirstBoot)
            yield return SceneManager.LoadSceneAsync(persistentSceneName, LoadSceneMode.Additive);

        // 2) Zone 전환
        if (isFirstBoot)
        {
            // 최초 로드: 인스펙터에 설정한 initialZoneName 사용
            yield return LoadNewZone(initialZoneName, initialSpawnPointName);
            SceneLoader.CurrentZone = initialZoneName;
        }
        else
        {
            // 포털 진입 후 로드: SceneLoader.NextZone 사용
            yield return SceneManager.UnloadSceneAsync(SceneLoader.CurrentZone);

            string nextZone = SceneLoader.NextZone;
            string nextSpawn = SceneLoader.NextSpawnPoint;

            yield return LoadNewZone(nextZone, nextSpawn);
            SceneLoader.CurrentZone = nextZone;

            SceneLoader.NextZone = null;
            SceneLoader.NextSpawnPoint = null;
        }

        // 3) Bootstrap 씬 언로드
        yield return SceneManager.UnloadSceneAsync("Bootstrap");
    }

    private IEnumerator LoadNewZone(string zoneName, string spawnPointName)
    {
        if (string.IsNullOrEmpty(zoneName))
        {
            Debug.LogError($"LoadNewZone: zoneName이 비어 있습니다!");
            yield break;
        }

        // Additive 로드
        yield return SceneManager.LoadSceneAsync(zoneName, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(zoneName));

        // 플레이어 위치 이동
        var player = GameObject.FindWithTag("Player");
        var spawn = GameObject.Find(spawnPointName);
        if (player != null && spawn != null)
            player.transform.position = spawn.transform.position;
        else
            Debug.LogWarning($"플레이어({player}) 또는 SpawnPoint({spawnPointName})를 찾지 못했습니다.");

    }
}