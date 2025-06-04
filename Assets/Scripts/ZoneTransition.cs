using UnityEngine;
using UnityEngine.SceneManagement;

public class ZoneTransition : MonoBehaviour
{
    [Tooltip("이 포털을 타면 이동할 Zone 씬 이름")]
    public string targetZoneScene;
    [Tooltip("이 Zone 씬 내부에서 플레이어가 나타날 SpawnPoint 오브젝트 이름")]
    public string spawnPointName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFX.Portal);
        }
        // 다음에 로드할 씬/스폰위치 지정
        SceneLoader.NextZone = targetZoneScene;
        SceneLoader.NextSpawnPoint = spawnPointName;
        // Bootstrap 씬을 Single 모드로 로드 -> 기존 맵/Bootstrap 모두 언로드
        SceneLoader.IsRespawn = false;
        SceneManager.LoadScene("Bootstrap", LoadSceneMode.Additive);
    }
}