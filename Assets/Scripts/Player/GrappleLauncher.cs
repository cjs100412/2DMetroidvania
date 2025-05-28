using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GrappleLauncher : MonoBehaviour
{
    [Header("Settings")]
    public float attachRange = 5f;    // 최대 걸 수 있는 거리
    public LayerMask grappleLayer;                 // 걸 수 있는 오브젝트 레이어
    public float launchForce = 20f;   // 대상 오브젝트 튕겨낼 힘
    public float playerLaunchForce = 10f;   // 플레이어 튕겨낼 힘
    public KeyCode grappleKey = KeyCode.C;

    // runtime
    Rigidbody2D rbPlayer;
    DistanceJoint2D joint;
    Rigidbody2D grabbedRb;
    bool isAttached;

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
        if (Input.GetKeyDown(grappleKey))
        {
            if (!isAttached) TryAttach();
            else LaunchOff();
        }
    }

    void TryAttach()
    {
        // 바라보는 방향으로 Raycast
        Vector2 dir = transform.localScale.x >= 0
            ? Vector2.right
            : Vector2.left;

        var hit = Physics2D.Raycast(transform.position,
                                    dir, attachRange, grappleLayer);
        if (hit.collider != null && hit.rigidbody != null)
        {
            grabbedRb = hit.rigidbody;

            // 현재 거리만큼만 묶기
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

        // 당긴 방향 (플레이어→대상)
        Vector2 pullDir = (grabbedRb.position - rbPlayer.position).normalized;

        // (1) 대상 튕기기
        grabbedRb.AddForce(pullDir * launchForce,
                           ForceMode2D.Impulse);

        // (2) 플레이어 튕기기
        rbPlayer.AddForce(-pullDir * playerLaunchForce,
                          ForceMode2D.Impulse);

        // (3) Joint 해제
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
