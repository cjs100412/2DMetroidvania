using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GrappleLauncher : MonoBehaviour
{
    [Header("Settings")]
    public float attachRange = 10f;    // 최대 걸 수 있는 거리
    public LayerMask grappleLayer;                 // 걸 수 있는 오브젝트 레이어
    public float launchForce = 200f;   // 대상 오브젝트 튕겨낼 힘
    public float playerLaunchForce = 100f;   // 플레이어 튕겨낼 힘
    public KeyCode grappleKey = KeyCode.C;

    // runtime
    Rigidbody2D rbPlayer;
    DistanceJoint2D joint;
    Rigidbody2D grabbedRb;
    public bool isAttached;
    public bool Cangrap = false;

    void Awake()
    {
        rbPlayer = GetComponent<Rigidbody2D>();

        // 런타임에 Joint 추가
        joint = gameObject.AddComponent<DistanceJoint2D>();
        joint.enabled = false;
        joint.autoConfigureDistance = false;
        joint.maxDistanceOnly = true;    // Rope처럼 동작하게
    }

    void Update()
    {
        if (Input.GetKeyDown(grappleKey) && Cangrap)
        {
            if (!isAttached) TryAttach();
            else LaunchOff();
        }
    }

    void TryAttach()
    {
        // 기존 transform.localScale.x 대신 spriteRenderer.flipX 사용
        var sr = GetComponent<SpriteRenderer>();
        Vector2 dir = sr != null && sr.flipX
            ? Vector2.left
            : Vector2.right;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, dir, attachRange, grappleLayer
        );
        if (hit.collider != null && hit.rigidbody != null)
        {
            grabbedRb = hit.rigidbody;
            float d = Vector2.Distance(transform.position, hit.point);
            joint.connectedBody = grabbedRb;
            joint.distance = d;
            joint.enabled = true;
            isAttached = true;
        }
    }

    void LaunchOff()
    {
        if (!isAttached) return;

        // 1) 플레이어가 눌러놓은 방향키 가져오기 (8방향)
        Vector2 inputDir = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        Vector2 launchDir;

        // 2) 입력이 있으면 그 방향, 없으면 원래 로프 방향(pullDir) 사용
        if (inputDir.sqrMagnitude > 0.01f)
        {
            launchDir = inputDir.normalized;
        }
        else
        {
            // pullDir = (대상 → 플레이어) 반대
            Vector2 pullDir = (grabbedRb.position - rbPlayer.position).normalized;
            launchDir = pullDir;
        }

        // 3) 대상에도 같은 방향(launchDir)으로 Impulse
        grabbedRb.AddForce(launchDir * launchForce,
                            ForceMode2D.Impulse);

        // 4) 플레이어는 반대 방향(-launchDir)으로 Impulse
        rbPlayer.AddForce(-launchDir * playerLaunchForce,
                           ForceMode2D.Impulse);

        // 수평 입력 잠금
        var pm = GetComponent<PlayerMovement>();
        pm?.LockHorizontal();

        // 5) Joint 해제
        joint.enabled = false;
        joint.connectedBody = null;
        grabbedRb = null;
        isAttached = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 dir = transform.localScale.x >= 0
            ? Vector3.right
            : Vector3.left;
        Gizmos.DrawLine(transform.position,
                        transform.position + dir * attachRange);
    }
}
