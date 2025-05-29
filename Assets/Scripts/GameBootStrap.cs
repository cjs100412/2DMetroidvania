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

        // 1) Persistent 씬 로드
        if (isFirstBoot)
            yield return SceneManager.LoadSceneAsync(persistentSceneName, LoadSceneMode.Additive);

        // 2) Zone 전환
        if (isFirstBoot)
        {
            // 최초 진입
            yield return LoadNewZone(initialZoneName, initialSpawnPointName);
            SceneLoader.CurrentZone = initialZoneName;
        }
        else
        {
            // (A) 이전 Zone 언로드
            string prev = SceneLoader.CurrentZone;
            if (!string.IsNullOrEmpty(prev))
            {
                var sc = SceneManager.GetSceneByName(prev);
                if (sc.isLoaded)
                    yield return SceneManager.UnloadSceneAsync(prev);
            }

            // (B) 다음 Zone 결정 (체크포인트 없으면 prevZone → StartScene)
            string nextZone = SceneLoader.NextZone;
            if (string.IsNullOrEmpty(nextZone))
            {
                Debug.LogWarning($"NextZone 비어있음 → CurrentZone({prev}) 재사용");
                nextZone = prev;
            }

            // (C) 다음 SpawnPoint 결정 (없으면 StartSpawn)
            string nextSpawn = SceneLoader.NextSpawnPoint;
            if (string.IsNullOrEmpty(nextSpawn))
            {
                Debug.LogWarning($"NextSpawnPoint 비어있음 → initialSpawnPointName({initialSpawnPointName}) 사용");
                nextSpawn = initialSpawnPointName;
            }

            // (D) 실제 로드
            yield return LoadNewZone(nextZone, nextSpawn);

            SceneLoader.CurrentZone = nextZone;
            SceneLoader.NextZone = null;
            SceneLoader.NextSpawnPoint = null;
        }

        // 3) Bootstrap 언로드
        yield return SceneManager.UnloadSceneAsync(gameObject.scene.name);
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
        if (player != null)
        {
            var spawn = GameObject.Find(spawnPointName);
            if (spawn != null)
                player.transform.position = spawn.transform.position;
            else
                Debug.LogWarning($"SpawnPoint[{spawnPointName}] 못 찾음. 위치 복원 스킵.");
        }

    }
}