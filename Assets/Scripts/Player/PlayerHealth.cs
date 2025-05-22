using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    Animator animator;

    [Header("플레이어 체력")]
    public int Max_hp = 100;
    public int hp;

    public bool isDead = false;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        hp = Max_hp;
    }

    private void Update()
    {

    }

    public void Damaged(int amount)
    {
        if(isDead) return;
        hp -= amount;
        StartCoroutine(DamagedFlash());
        Debug.Log("Player Damaged");
        animator.SetTrigger("isDamaged");
        if(hp <= 0)
        {
            Die();
        }
    }

    IEnumerator DamagedFlash()
    {
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.3f);
        yield return new WaitForSeconds(0.2f);
        GetComponent<SpriteRenderer>().color = Color.white;
        yield return new WaitForSeconds(0.2f);
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.3f);
        yield return new WaitForSeconds(0.2f);
        GetComponent<SpriteRenderer>().color = Color.white;
    }
    IEnumerator DeadFlash()
    {
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.3f);
        yield return new WaitForSeconds(0.3f);
        GetComponent<SpriteRenderer>().color = Color.white;
        yield return new WaitForSeconds(0.3f);
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.3f);
        yield return new WaitForSeconds(0.3f);
        GetComponent<SpriteRenderer>().color = Color.white;
        yield return new WaitForSeconds(0.3f);
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.3f);
        yield return new WaitForSeconds(0.3f);
        GetComponent<SpriteRenderer>().color = Color.white;
        yield return new WaitForSeconds(0.3f);
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.3f);
        yield return new WaitForSeconds(0.3f);
        GetComponent<SpriteRenderer>().color = Color.white;
        yield return new WaitForSeconds(0.3f);
    }

    public void Die()
    {
        animator.SetTrigger("isDead");
        Debug.Log("Player Die");
        isDead = true;

        // 콜라이더 비활성화
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Rigidbody 비활성화
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        StartCoroutine(DeadFlash());
        Destroy(gameObject, 5f);
    }
}
