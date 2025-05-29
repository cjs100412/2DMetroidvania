using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public PlayerMovement playerMovement;

    public LayerMask groundLayer;
    public LayerMask platformLayer;


    void OnCollisionEnter2D(Collision2D col)
    {
        if (((1 << col.gameObject.layer) & groundLayer) != 0 || ((1 << col.gameObject.layer) & platformLayer) != 0)
        {
            playerMovement.isGrounded = true;
            playerMovement.jumpCount = 0;  // ���鿡 ������ ���� ī��Ʈ ����
            playerMovement.UnlockHorizontal();
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (((1 << col.gameObject.layer) & groundLayer) != 0 || ((1 << col.gameObject.layer) & platformLayer) != 0)
        {
            playerMovement.isGrounded = false;
            //playerMovement.GetComponent<Animator>().SetBool("isGround", true);
        }
    }
}
