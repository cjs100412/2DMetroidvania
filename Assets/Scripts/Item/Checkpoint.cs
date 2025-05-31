using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Tooltip("이 체크포인트가 가진 스폰포인트 이름")]
    public string spawnPointName = "FirstCheckPoint";

    [Header("–– 체크포인트 도달 시 깜빡일 텍스트(CanvasGroup)")]
    [Tooltip("체크포인트를 찍었을 때 잠깐 보여줄 텍스트 오브젝트의 CanvasGroup")]
    public CanvasGroup checkpointTextCanvasGroup;

    [Tooltip("페이드 인/아웃에 걸리는 시간(초)")]
    public float fadeDuration = 0.5f;

    [Tooltip("페이드 인 후 텍스트가 완전히 보인 상태로 유지되는 시간(초)")]
    public float holdDuration = 1.0f;

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

        // 3) 텍스트 페이드 인/아웃 코루틴 실행
        if (checkpointTextCanvasGroup != null)
        {
            // 혹시 이미 코루틴이 실행 중이면 중지 후 다시 시작
            StopAllCoroutines();
            StartCoroutine(FadeTextRoutine());
        }
    }

    private IEnumerator FadeTextRoutine()
    {
        Debug.Log("FadeTextRoutine: 시작, alpha=0 세팅");
        checkpointTextCanvasGroup.alpha = 0f;
        checkpointTextCanvasGroup.blocksRaycasts = false;

        // 페이드 인
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            checkpointTextCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        checkpointTextCanvasGroup.alpha = 1f;
        Debug.Log("FadeTextRoutine: 페이드 인 완료, alpha=1");

        // 고정 대기
        yield return new WaitForSeconds(holdDuration);
        Debug.Log("FadeTextRoutine: holdDuration 완료");

        // 페이드 아웃
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            checkpointTextCanvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / fadeDuration));
            yield return null;
        }
        checkpointTextCanvasGroup.alpha = 0f;
        Debug.Log("FadeTextRoutine: 페이드 아웃 완료, alpha=0");
    }
}
