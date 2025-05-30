using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [Header("스탯")]
    public int maxHp = 100;
    public int maxMp = 5;
    public int currentHp { get;  set; }
    public int currentMp { get;  set; }

    public bool isDead { get;  set; }

    Collider2D col;
    Rigidbody2D rb;
    Animator animator;

    [Header("무적/깜박임")]
    public float invincibleDuration = 1f;
    public float deathFlashCount = 5;
    public float deathFlashInterval = 0.3f;

    // 피격 깜박임 공통
    IEnumerator FlashCoroutine(float flashAlpha, int flashes, float interval)
    {
        var sr = GetComponent<SpriteRenderer>();
        Color orig = sr.color;
        Color flash = new Color(1, 1, 1, flashAlpha);
        for (int i = 0; i < flashes; i++)
        {
            sr.color = flash;
            yield return new WaitForSeconds(interval);
            sr.color = orig;
            yield return new WaitForSeconds(interval);
        }
    }

    public void Respawn(Vector3 position, int hp, int mp)
    {
        // 1) 위치 복원
        transform.position = position;

        // 2) 회전/스케일 초기화 (바닥에 똑바로 세우기)
        transform.rotation = Quaternion.identity;
        var ls = transform.localScale;
        ls.y = Mathf.Abs(ls.y);
        transform.localScale = ls;

        // 3) 물리·충돌 복원
        var col = GetComponent<Collider2D>();
        var rb = GetComponent<Rigidbody2D>();
        col.enabled = true;
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;  // 이전 관성 제거

        // 4) 이동 스크립트 재활성화
        var pm = GetComponent<PlayerMovement>();
        if (pm != null) pm.enabled = true;

        // 5) 체력·마나 복원
        currentHp = maxHp;
        currentMp = maxMp;
        isDead = false;

        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);

        // 6) 애니메이터 상태 리셋
        animator.ResetTrigger("isDead");
        animator.Play("Base Layer.Locomotion.WalkRun");  // 또는 기본 아이들 애니메이션

        Debug.Log($"Player Respawned @({position.x:0.0},{position.y:0.0}) HP={hp} MP={mp}");
    }

    void Awake()
    {
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        currentHp = maxHp;
        currentMp = maxMp;
    }

    public void Damaged(int amount)
    {
        if (isDead) return;
        StartCoroutine(InvincibleRoutine());
        StartCoroutine(FlashCoroutine(0.3f, 2, 0.1f));

        currentHp = Mathf.Max(currentHp - amount, 0);
        if (currentHp <= 0) Die();
    }

    IEnumerator InvincibleRoutine()
    {
        bool prev = col.enabled;
        col.enabled = false;
        yield return new WaitForSeconds(invincibleDuration);
        col.enabled = prev;
    }

    void Die()
    {
        isDead = true;
        animator.SetTrigger("isDead");

        // 입력/이동 막기
        GetComponent<PlayerMovement>().enabled = false;

        // 물리·충돌 비활성화
        col.enabled = false;
        rb.simulated = false;

        // 깜박임 후 복귀 로드
        StartCoroutine(DeathSequence());

    }

    IEnumerator DeathSequence()
    {
        // 1) 느린 깜박임
        yield return FlashCoroutine(0.3f, (int)deathFlashCount, deathFlashInterval);

        // 2) 잠깐 대기(파티클, 연출 여지)
        yield return new WaitForSeconds(0.2f);

        if (!string.IsNullOrEmpty(SceneLoader.LastCheckpointZone))
        {
            SceneLoader.NextZone = SceneLoader.LastCheckpointZone;
            SceneLoader.NextSpawnPoint = SceneLoader.LastCheckpointSpawn;
        }
        else
        {
            // 체크포인트가 없으면 기본 리스폰
            SceneLoader.NextZone = "StartScene";
            SceneLoader.NextSpawnPoint = "StartSpawn";
        }

        SceneManager.LoadScene("Bootstrap", LoadSceneMode.Single);
        // 3) 씬 & 상태 복원
        //SceneManager.LoadScene("Bootstrap", LoadSceneMode.Single);
        // 여기서 GameManager가 비동기로 씬을 불러오고
        // 로드 완료 후 데이터를 복원합니다.

        // 4) 현재 오브젝트는 곧 사라질 테니 더 이상 작업 없음
    }
}
