using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    Animator animator;

    public int Max_hp = 100;
    public int hp;



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
        hp -= amount;
        animator.SetTrigger("isDamaged");
        Debug.Log("Player Damaged");
        if(hp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        animator.SetTrigger("isDead");
        Debug.Log("Player Die");

    }
}
