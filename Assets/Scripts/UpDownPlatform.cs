using UnityEngine;

public class UpDownPlatform : MonoBehaviour
{
    private bool goDown = false;

    private float speed = 2.0f;
    private float max_y = -5;
    private float min_y = -21f;
    private void Update()
    {
        if (goDown)
        {
            transform.position += Vector3.down * Time.deltaTime * speed;
            if (transform.position.y <= min_y)
                goDown = false;
        }
        else
        {
            transform.position += Vector3.up * Time.deltaTime * speed;
            if (transform.position.y >= max_y)
                goDown = true;
        }
    }
}
