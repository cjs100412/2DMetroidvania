using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
public class PlayerHealth : MonoBehaviour
{
    [Header("스탯")]
    [Tooltip("플레이어의 최대 체력")]
    [SerializeField] private int maxHp = 100;  // 최대 체력
    private int currentHp;                      // 현재 체력

    // 외부에서 읽기 전용으로, 내부에서만 변경
    public bool isDead { get; private set; } = false;

    // 캐싱할 컴포넌트들
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private Rigidbody2D rb;

    public float invincibleDuration = 1f;    // 무적 지속 시간
    private bool isInvincible = false;       // 무적 중인지 여부

    // 애니메이터 파라미터 해시
    //private static readonly int HashDamaged = Animator.StringToHash("isDamaged");  // 피격 플래그
    private static readonly int HashDead = Animator.StringToHash("isDead");     // 사망 플래그

    void Awake()
    {
        // 컴포넌트 캐싱
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        // 체력 초기화
        currentHp = maxHp;
    }

    /// <summary>
    /// 플레이어가 데미지를 입었을 때 호출
    /// </summary>
    public void Damaged(int amount)
    {
        // 이미 사망한 상태라면 무시
        if (isDead)
            return;

        // 무적 상태면 무시
        if (isInvincible) return;

        // 체력 차감, 음수 방지
        currentHp = Mathf.Max(currentHp - amount, 0);
        StartCoroutine(InvincibleCoroutine());
        StartCoroutine(DamagedFlash());   // 피격 깜박임 효과
        Debug.Log("플레이어 데미지: " + amount);

        // 체력이 0 이하이면 사망 처리
        if (currentHp <= 0)
            Die();
    }

    /// <summary>
    /// 피격 시 깜박임 코루틴
    /// </summary>
    private IEnumerator DamagedFlash()
    {
        //animator.SetBool(HashDamaged, true);  // 애니메이터에 피격 상태 설정
        // 깜박임 효과 (반투명 ↔ 원래 색) 반복
        animator.SetTrigger("isDamaged");
        yield return FlashCoroutine(0.3f, 3, 0.2f);
        //animator.SetBool(HashDamaged, false); // 피격 상태 해제
    }

    /// <summary>
    /// 피격 무적 코루틴
    /// </summary>
    private IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;
        Debug.Log("무적");
        yield return new WaitForSeconds(invincibleDuration);
        isInvincible = false;
    }

    /// <summary>
    /// 깜박임 공통 코루틴 (투명도, 반복 횟수, 간격)
    /// </summary>
    private IEnumerator FlashCoroutine(float flashAlpha, int flashes, float interval)
    {
        //Color original = spriteRenderer.color;
        Color original = new Color(1f, 1f, 1f, 1f);
        Color flashColor = new Color(1f, 1f, 1f, flashAlpha);

        for (int i = 0; i < flashes; i++)
        {
            spriteRenderer.color = flashColor;      // 반투명 설정
            yield return new WaitForSeconds(interval);
            spriteRenderer.color = original;        // 원래 색 복원
            yield return new WaitForSeconds(interval);
        }
    }

    /// <summary>
    /// 플레이어 사망 처리
    /// </summary>
    private void Die()
    {
        isDead = true;
        animator.SetTrigger(HashDead);  // 사망 애니메이션 트리거
        Debug.Log("플레이어 사망");

        // 피격 중이던 코루틴 정지
        StopAllCoroutines();
        // 스크립트 비활성화로 추가 로직 방지
        enabled = false;

        // 콜라이더 및 물리 비활성화
        col.enabled = false;
        rb.simulated = false;

        // 사망 깜박임 후 오브젝트 제거
        StartCoroutine(DeathSequence());
    }

    /// <summary>
    /// 사망 깜박임 및 제거 코루틴
    /// </summary>
    private IEnumerator DeathSequence()
    {
        // 느린 깜박임 5회 반복
        yield return FlashCoroutine(0.3f, 5, 0.3f);

        // 잠시 대기 후 파괴
        Destroy(gameObject, 0.5f);
    }
}
