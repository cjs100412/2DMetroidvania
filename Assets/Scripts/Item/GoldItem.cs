using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider2D))]
public class GoldItem : MonoBehaviour
{
    [Header("������ ���� ID (��: Gold_001)")]
    public string itemID;

    [Header("ȹ�� ����Ʈ")]
    public ParticleSystem pickupEffect;

    private PlayerInventory playerInventory;


    private void Awake()
    {
        if (GameManager.I != null && GameManager.I.IsItemCollected(itemID))
        {
            Destroy(gameObject);
            return;
        }

        var player = GameObject.FindWithTag("Player");
        playerInventory = player.GetComponent<PlayerInventory>();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            GameManager.I.SetItemCollected(itemID);

            playerInventory.AddCoins(50);


            // ����Ʈ ���
            if (pickupEffect != null)
                Instantiate(pickupEffect, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }

}
