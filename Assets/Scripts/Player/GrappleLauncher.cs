using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GrappleLauncher : MonoBehaviour
{
    [Header("Settings")]
    public float attachRange = 10f;    // �ִ� �� �� �ִ� �Ÿ�
    public LayerMask grappleLayer;                 // �� �� �ִ� ������Ʈ ���̾�
    public float launchForce = 200f;   // ��� ������Ʈ ƨ�ܳ� ��
    public float playerLaunchForce = 100f;   // �÷��̾� ƨ�ܳ� ��
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

        // ��Ÿ�ӿ� Joint �߰�
        joint = gameObject.AddComponent<DistanceJoint2D>();
        joint.enabled = false;
        joint.autoConfigureDistance = false;
        joint.maxDistanceOnly = true;    // Ropeó�� �����ϰ�
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
        // ���� transform.localScale.x ��� spriteRenderer.flipX ���
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

        // 1) �÷��̾ �������� ����Ű �������� (8����)
        Vector2 inputDir = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        Vector2 launchDir;

        // 2) �Է��� ������ �� ����, ������ ���� ���� ����(pullDir) ���
        if (inputDir.sqrMagnitude > 0.01f)
        {
            launchDir = inputDir.normalized;
        }
        else
        {
            // pullDir = (��� �� �÷��̾�) �ݴ�
            Vector2 pullDir = (grabbedRb.position - rbPlayer.position).normalized;
            launchDir = pullDir;
        }

        // 3) ��󿡵� ���� ����(launchDir)���� Impulse
        grabbedRb.AddForce(launchDir * launchForce,
                            ForceMode2D.Impulse);

        // 4) �÷��̾�� �ݴ� ����(-launchDir)���� Impulse
        rbPlayer.AddForce(-launchDir * playerLaunchForce,
                           ForceMode2D.Impulse);

        // ���� �Է� ���
        var pm = GetComponent<PlayerMovement>();
        pm?.LockHorizontal();

        // 5) Joint ����
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
