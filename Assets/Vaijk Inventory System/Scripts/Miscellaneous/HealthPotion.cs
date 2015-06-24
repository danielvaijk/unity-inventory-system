using UnityEngine;
using System.Collections;

public class HealthPotion : MonoBehaviour
{
    public float healAmount;

    private void InventoryUse ()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        player.GetComponent<PlayerHealth>().health += healAmount;
    }
}