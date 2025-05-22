using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("몬스터 체력")]
    public int hp;
    public int max_hp = 10;

    [Header("몬스터 공격력")]
    public int damage = 1;

    [Header("정찰 속도")]
    public float patrolSpeed = 2f;

    [Header("공격 범위, 공격 쿨타임")]
    public float attackRange = 2.0f;
    public float attackCooldown = 1f;

    [Header("추격 범위, 속도")]
    public float chaseRange = 10f;
    public float chaseSpeed = 5f;   

    [Header("공격 판정 위치")]
    public Transform attackPoint;
    public float attackRadius = 0.5f;
    public LayerMask playerLayer;

    private float lastAttackTime;

    private enum State { Patrol, Chase, Attack }
    private State state = State.Patrol;
    private Vector2 patrolDir = Vector2.right;
    public PlayerHealth playerHealth;
    public Transform playerTransform;
    private Rigidbody2D rb;

    private void Awake()
    {
        hp = max_hp;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (playerHealth.isDead == true) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        // 상태 결정
        if (dist <= attackRange)
            state = State.Attack;
        else if (dist >= attackRange && dist <= chaseRange)
            state = State.Chase;
        else
            state = State.Patrol;

        // 상태별 로직
        switch (state)
        {
            case State.Patrol:
                Patrol();
                break;
            case State.Chase:
                Chase();
                break;
            case State.Attack:
                Attack();
                break;
        }

        // 바라보는 방향
        if (playerTransform.position.x > transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }

    private void Patrol()
    {
        rb.linearVelocity = new Vector2(patrolDir.x * patrolSpeed, rb.linearVelocity.y);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, patrolDir, 2f, LayerMask.GetMask("Ground"));
        if (hit.collider != null)
            patrolDir *= -1;
    }

    private void Chase()
    {
        Vector2 dir = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * chaseSpeed, rb.linearVelocity.y);
    }


    private void Attack()
    {
        //rb.linearVelocity = Vector2.zero;
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            // 데미지 판정
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
            foreach (var hit in hits)
            {
                hit.GetComponent<PlayerHealth>()?.Damaged(damage);
            }
            // TODO: 이곳에 애니메이터 트리거(attack)도 연결
        }
    }

    public void Damaged(int amount)
    {
        hp -= amount;
        StartCoroutine(RedFlash());

        Debug.Log("Enemy Damaged");

        if (hp <= 0)
        {
            Die();
        }
    }
    IEnumerator RedFlash()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(0.2f);
        GetComponent<SpriteRenderer>().color = Color.white;
        yield return new WaitForSeconds(0.2f);
        GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(0.2f);
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    private void Die()
    {
        Destroy(gameObject, 0.2f);
    }
}
