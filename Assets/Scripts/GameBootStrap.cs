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
        // 1) Persistent 씬은 한 번만 불러오기
        if(GameManager.I == null)
        {
            yield return SceneManager.LoadSceneAsync(persistentSceneName, LoadSceneMode.Additive);
        }

        // 2) 이전 Zone 언로드 (빈 값 아닐 때)
        if (!string.IsNullOrEmpty(SceneLoader.CurrentZone))
        {
            Scene prev = SceneManager.GetSceneByName(SceneLoader.CurrentZone);
            if (prev.IsValid() && prev.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(prev);
            }
        }

        // 3) 다음 Zone/Spawn 결정 (체크포인트 없으면 초기값 사용)
        string nextZone = string.IsNullOrEmpty(SceneLoader.NextZone)
            ? initialZoneName
            : SceneLoader.NextZone;
        string nextSpawn = string.IsNullOrEmpty(SceneLoader.NextSpawnPoint)
            ? initialSpawnPointName
            : SceneLoader.NextSpawnPoint;

        // 4) 새 Zone 로드 & 스폰
        yield return LoadNewZone(nextZone, nextSpawn);

        // 5) 상태 클리어
        SceneLoader.CurrentZone = nextZone;
        SceneLoader.NextZone = null;
        SceneLoader.NextSpawnPoint = null;

        // 6) Bootstrap 언로드
        yield return SceneManager.UnloadSceneAsync(gameObject.scene.name);
    }


    private IEnumerator LoadNewZone(string zoneName, string spawnPointName)
    {
        if (string.IsNullOrEmpty(zoneName))
        {
            Debug.LogError($"LoadNewZone: zoneName이 비어 있습니다!");
            yield break;
        }

        // 1) 대상 Zone을 Additive 모드로 로드
        yield return SceneManager.LoadSceneAsync(zoneName, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(zoneName));

        // 2) 플레이어 위치 이동
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var spawn = GameObject.Find(spawnPointName);
            if (spawn != null)
            {
                // 2-1) 위치 복원
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

                    // 저장된 최신 코인 수만큼 다시 추가
                    inv.AddCoins(GameManager.I.SavedCoins);
                }
            }
            else
            {
                Debug.LogWarning($"SpawnPoint[{spawnPointName}] 못 찾음. 위치 복원 스킵.");
            }
        }
    }

}