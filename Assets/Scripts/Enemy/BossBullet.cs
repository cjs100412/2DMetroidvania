using UnityEngine;

public class BossBullet : MonoBehaviour
{
    public int damage = 10;

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.Damaged(damage);
        }
        Destroy(gameObject);
    }
}
