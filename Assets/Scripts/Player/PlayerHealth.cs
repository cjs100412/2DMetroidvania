using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
public class PlayerHealth : MonoBehaviour
{
    [Header("����")]
    [Tooltip("�÷��̾��� �ִ� ü��")]
    [SerializeField] private int maxHp = 100;  // �ִ� ü��
    private int currentHp;                      // ���� ü��

    // �ܺο��� �б� ��������, ���ο����� ����
    public bool isDead { get; private set; } = false;

    // ĳ���� ������Ʈ��
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private Rigidbody2D rb;

    public float invincibleDuration = 1f;    // ���� ���� �ð�
    private bool isInvincible = false;       // ���� ������ ����

    // �ִϸ����� �Ķ���� �ؽ�
    //private static readonly int HashDamaged = Animator.StringToHash("isDamaged");  // �ǰ� �÷���
    private static readonly int HashDead = Animator.StringToHash("isDead");     // ��� �÷���

    void Awake()
    {
        // ������Ʈ ĳ��
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        // ü�� �ʱ�ȭ
        currentHp = maxHp;
    }

    /// <summary>
    /// �÷��̾ �������� �Ծ��� �� ȣ��
    /// </summary>
    public void Damaged(int amount)
    {
        // �̹� ����� ���¶�� ����
        if (isDead)
            return;

        // ���� ���¸� ����
        if (isInvincible) return;

        // ü�� ����, ���� ����
        currentHp = Mathf.Max(currentHp - amount, 0);
        StartCoroutine(InvincibleCoroutine());
        StartCoroutine(DamagedFlash());   // �ǰ� ������ ȿ��
        Debug.Log("�÷��̾� ������: " + amount);

        // ü���� 0 �����̸� ��� ó��
        if (currentHp <= 0)
            Die();
    }

    /// <summary>
    /// �ǰ� �� ������ �ڷ�ƾ
    /// </summary>
    private IEnumerator DamagedFlash()
    {
        //animator.SetBool(HashDamaged, true);  // �ִϸ����Ϳ� �ǰ� ���� ����
        // ������ ȿ�� (������ �� ���� ��) �ݺ�
        animator.SetTrigger("isDamaged");
        yield return FlashCoroutine(0.3f, 3, 0.2f);
        //animator.SetBool(HashDamaged, false); // �ǰ� ���� ����
    }

    /// <summary>
    /// �ǰ� ���� �ڷ�ƾ
    /// </summary>
    private IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;
        Debug.Log("����");
        yield return new WaitForSeconds(invincibleDuration);
        isInvincible = false;
    }

    /// <summary>
    /// ������ ���� �ڷ�ƾ (����, �ݺ� Ƚ��, ����)
    /// </summary>
    private IEnumerator FlashCoroutine(float flashAlpha, int flashes, float interval)
    {
        //Color original = spriteRenderer.color;
        Color original = new Color(1f, 1f, 1f, 1f);
        Color flashColor = new Color(1f, 1f, 1f, flashAlpha);

        for (int i = 0; i < flashes; i++)
        {
            spriteRenderer.color = flashColor;      // ������ ����
            yield return new WaitForSeconds(interval);
            spriteRenderer.color = original;        // ���� �� ����
            yield return new WaitForSeconds(interval);
        }
    }

    /// <summary>
    /// �÷��̾� ��� ó��
    /// </summary>
    private void Die()
    {
        isDead = true;
        animator.SetTrigger(HashDead);  // ��� �ִϸ��̼� Ʈ����
        Debug.Log("�÷��̾� ���");

        // �ǰ� ���̴� �ڷ�ƾ ����
        StopAllCoroutines();
        // ��ũ��Ʈ ��Ȱ��ȭ�� �߰� ���� ����
        enabled = false;

        // �ݶ��̴� �� ���� ��Ȱ��ȭ
        col.enabled = false;
        rb.simulated = false;

        // ��� ������ �� ������Ʈ ����
        StartCoroutine(DeathSequence());
    }

    /// <summary>
    /// ��� ������ �� ���� �ڷ�ƾ
    /// </summary>
    private IEnumerator DeathSequence()
    {
        // ���� ������ 5ȸ �ݺ�
        yield return FlashCoroutine(0.3f, 5, 0.3f);

        // ��� ��� �� �ı�
        Destroy(gameObject, 0.5f);
    }
}
