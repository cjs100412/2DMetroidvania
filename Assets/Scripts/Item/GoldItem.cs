using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider2D))]
public class GoldItem : MonoBehaviour
{
    [Header("»πµÊ ¿Ã∆Â∆Æ")]
    public ParticleSystem pickupEffect;

    private PlayerInventory playerInventory;


    private void Awake()
    {
        var player = GameObject.FindWithTag("Player");
        playerInventory = player.GetComponent<PlayerInventory>();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            playerInventory.AddCoins(50);


            // ¿Ã∆Â∆Æ ¿Áª˝
            if (pickupEffect != null)
                Instantiate(pickupEffect, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }

}
