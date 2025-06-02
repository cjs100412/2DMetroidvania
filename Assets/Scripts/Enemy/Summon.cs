// Summon.cs
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class Summon : MonoBehaviour
{
    [Header("추적 속도")]
    public float chaseSpeed = 3f;

    [Header("아이템 충돌 데미지")]
    public int damage = 2;

    [Header("폭발 이펙트")]
    public GameObject explodeEffect;

    [Header("자폭 타이머 (초)")]
    public float lifetime = 3f;

    private Animator animator;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private bool isDestroyed = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        // Collider2D는 isTrigger = true로 설정되어 있어야 함
        var col = GetComponent<Collider2D>();
        if (!col.isTrigger) col.isTrigger = true;

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;       // 중력 영향 없음
        rb.freezeRotation = true;   // 회전 고정

        // 플레이어 참조
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
            playerTransform = playerGO.transform;
    }

    private void Start()
    {
        // lifetime 후에 자폭
        StartCoroutine(SelfDestructAfterDelay());
    }

    private IEnumerator SelfDestructAfterDelay()
    {
        yield return new WaitForSeconds(lifetime);
        Explode();
    }

    private void Update()
    {
        if (isDestroyed || playerTransform == null)
            return;

        // 플레이어 방향으로 이동
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity = direction * chaseSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroyed)
            return;

        // PlayerMovement 컴포넌트를 가진 오브젝트와 충돌 시 폭발
        var player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            // 플레이어 데미지
            var ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.Damaged(damage);

            Explode();
        }
    }

    private void Explode()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;

        // 폭발 이펙트 재생
        if (explodeEffect != null)
            Instantiate(explodeEffect, transform.position, Quaternion.identity);
        animator.SetTrigger("isDead");
        Destroy(gameObject);
    }
}
