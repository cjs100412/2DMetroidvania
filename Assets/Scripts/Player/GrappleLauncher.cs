using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GrappleLauncher : MonoBehaviour
{
    [Header("Settings")]
    public float attachRange = 5f;    // �ִ� �� �� �ִ� �Ÿ�
    public LayerMask grappleLayer;                 // �� �� �ִ� ������Ʈ ���̾�
    public float launchForce = 20f;   // ��� ������Ʈ ƨ�ܳ� ��
    public float playerLaunchForce = 10f;   // �÷��̾� ƨ�ܳ� ��
    public KeyCode grappleKey = KeyCode.C;

    // runtime
    Rigidbody2D rbPlayer;
    DistanceJoint2D joint;
    Rigidbody2D grabbedRb;
    bool isAttached;

    void Awake()
    {
        rbPlayer = GetComponent<Rigidbody2D>();

        // ��Ÿ�ӿ� Joint �߰�
        joint = gameObject.AddComponent<DistanceJoint2D>();
        joint.enabled = false;
        joint.autoConfigureDistance = false;
        joint.maxDistanceOnly = true;    // Ropeó�� �����ϰ�
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
        // �ٶ󺸴� �������� Raycast
        Vector2 dir = transform.localScale.x >= 0
            ? Vector2.right
            : Vector2.left;

        var hit = Physics2D.Raycast(transform.position,
                                    dir, attachRange, grappleLayer);
        if (hit.collider != null && hit.rigidbody != null)
        {
            grabbedRb = hit.rigidbody;

            // ���� �Ÿ���ŭ�� ����
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

        // ��� ���� (�÷��̾����)
        Vector2 pullDir = (grabbedRb.position - rbPlayer.position).normalized;

        // (1) ��� ƨ���
        grabbedRb.AddForce(pullDir * launchForce,
                           ForceMode2D.Impulse);

        // (2) �÷��̾� ƨ���
        rbPlayer.AddForce(-pullDir * playerLaunchForce,
                          ForceMode2D.Impulse);

        // (3) Joint ����
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
