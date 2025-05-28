using UnityEngine;

public class BossBullet : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.Damaged(5);
        }
        Destroy(gameObject);
    }
}
