using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [Header("����")]
    public int maxHp = 100;
    public int maxMp = 5;
    public int currentHp { get;  set; }
    public int currentMp { get;  set; }

    public bool isDead { get;  set; }

    Collider2D col;
    Rigidbody2D rb;
    Animator animator;

    [Header("����/������")]
    public float invincibleDuration = 1f;
    public float deathFlashCount = 5;
    public float deathFlashInterval = 0.3f;

    // �ǰ� ������ ����
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
        // 1) ��ġ ����
        transform.position = position;

        // 2) ȸ��/������ �ʱ�ȭ (�ٴڿ� �ȹٷ� �����)
        transform.rotation = Quaternion.identity;
        var ls = transform.localScale;
        ls.y = Mathf.Abs(ls.y);
        transform.localScale = ls;

        // 3) �������浹 ����
        var col = GetComponent<Collider2D>();
        var rb = GetComponent<Rigidbody2D>();
        col.enabled = true;
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;  // ���� ���� ����

        // 4) �̵� ��ũ��Ʈ ��Ȱ��ȭ
        var pm = GetComponent<PlayerMovement>();
        if (pm != null) pm.enabled = true;

        // 5) ü�¡����� ����
        currentHp = maxHp;
        currentMp = maxMp;
        isDead = false;

        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);

        // 6) �ִϸ����� ���� ����
        animator.ResetTrigger("isDead");
        animator.Play("Base Layer.Locomotion.WalkRun");  // �Ǵ� �⺻ ���̵� �ִϸ��̼�

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

        // �Է�/�̵� ����
        GetComponent<PlayerMovement>().enabled = false;

        // �������浹 ��Ȱ��ȭ
        col.enabled = false;
        rb.simulated = false;

        // ������ �� ���� �ε�
        StartCoroutine(DeathSequence());

    }

    IEnumerator DeathSequence()
    {
        // 1) ���� ������
        yield return FlashCoroutine(0.3f, (int)deathFlashCount, deathFlashInterval);

        // 2) ��� ���(��ƼŬ, ���� ����)
        yield return new WaitForSeconds(0.2f);

        if (!string.IsNullOrEmpty(SceneLoader.LastCheckpointZone))
        {
            SceneLoader.NextZone = SceneLoader.LastCheckpointZone;
            SceneLoader.NextSpawnPoint = SceneLoader.LastCheckpointSpawn;
        }
        else
        {
            // üũ����Ʈ�� ������ �⺻ ������
            SceneLoader.NextZone = "StartScene";
            SceneLoader.NextSpawnPoint = "StartSpawn";
        }

        SceneManager.LoadScene("Bootstrap", LoadSceneMode.Single);
        // 3) �� & ���� ����
        //SceneManager.LoadScene("Bootstrap", LoadSceneMode.Single);
        // ���⼭ GameManager�� �񵿱�� ���� �ҷ�����
        // �ε� �Ϸ� �� �����͸� �����մϴ�.

        // 4) ���� ������Ʈ�� �� ����� �״� �� �̻� �۾� ����
    }
}
