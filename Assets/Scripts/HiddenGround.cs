using UnityEngine;
using UnityEngine.Tilemaps;

public class HiddenGround : MonoBehaviour
{
    private Tilemap tileMap;

    private void Awake()
    {
        tileMap = GetComponent<Tilemap>();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            tileMap.color = new Color(1f, 1f, 1f, 0.5f);

        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            tileMap.color = new Color(1f, 1f, 1f, 1f);

        }
    }

}
