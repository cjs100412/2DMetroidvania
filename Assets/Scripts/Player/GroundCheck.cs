using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public PlayerMovement playerMovement;

    public LayerMask groundLayer;


    void OnCollisionEnter2D(Collision2D col)
    {
        if (((1 << col.gameObject.layer) & groundLayer) != 0)
        {
            playerMovement.isGrounded = true;
            playerMovement.jumpCount = 0;  // 지면에 닿으면 점프 카운트 리셋
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (((1 << col.gameObject.layer) & groundLayer) != 0)
        {
            playerMovement.isGrounded = false;
        }
    }
}
