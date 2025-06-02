using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HomingProjectile : MonoBehaviour
{
    private Transform target;
    private Rigidbody2D rb;
    private float speed;
    private float turnSpeed;
    public int damage = 10;
    /// <summary>
    /// 초기화: 발사 패턴에서 호출합니다.
    /// </summary>
    public void Init(Transform target, float speed, float turnSpeed)
    {
        this.target = target;
        this.speed = speed;
        this.turnSpeed = turnSpeed;
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // 목표 방향 벡터
        Vector2 dir = ((Vector2)target.position - rb.position).normalized;
        // 현재 회전벡터
        float rotateAmount = Vector3.Cross(dir, transform.up).z;

        // 회전 처리
        rb.angularVelocity = -rotateAmount * turnSpeed;
        // 진행 처리
        rb.linearVelocity = transform.up * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.Damaged(damage); // 맞추고 싶은 대미지
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
