using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int hp;
    public int max_hp = 10;
    public int damage = 1;
    public float patrolSpeed = 2f;
    public float attackRange = 2.0f;
    public float attackCooldown = 1f;
    public float chaseRange = 10f;
    public float chaseSpeed = 5f;   

    private float lastAttackTime;


    [Header("공격 판정 위치")]
    public Transform attackPoint;
    public float attackRadius = 0.5f;
    public LayerMask playerLayer;

    private enum State { Patrol, Chase, Attack }
    private State state = State.Patrol;
    private Vector2 patrolDir = Vector2.right;
    public Transform playerTransform;
    private Rigidbody2D rb;

    private void Awake()
    {
        hp = max_hp;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        float dist = Vector2.Distance(transform.position, playerTransform.position);

        // 상태 결정
        if (dist <= attackRange)
            state = State.Attack;
        else if (dist <= chaseRange)
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
        rb.linearVelocity = Vector2.zero;
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
        if (hp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject, 0.2f);
    }
}
