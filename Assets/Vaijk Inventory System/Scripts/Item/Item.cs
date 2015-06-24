using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]

public class Item : MonoBehaviour
{
    public int itemID;
    public int maxStackAmount;

    public float interactTime;

    public bool usable;

    [HideInInspector]
    public bool wasPlaced;

    public Texture2D slotIcon;

    private Renderer thisRenderer = null;
    private Collider thisCollider = null;

    private Inventory playerInventory = null;

    private void OnValidate ()
    {
        if (itemID < 1)
            itemID = 1;

        if (maxStackAmount < 1)
            maxStackAmount = 1;

        if (interactTime < 1)
            interactTime = 1;
    }

    private void Awake ()
    {
        thisRenderer = GetComponent<Renderer>();
        thisCollider = GetComponent<Collider>();

        playerInventory = GameObject.FindObjectOfType<Inventory>();
    }

    public void PickupItem (Transform player)
    {
        // Set our parent as the Player's Camera.
        transform.parent = player.Find("Camera").Find("Inventory Items");

        if (thisRenderer == null)
        {
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).GetComponent<Renderer>().enabled = false;
        }
        else
        {
            thisRenderer.enabled = false;
        }

        thisCollider.isTrigger = true;
    }

    public void DropItem ()
    {
        transform.position = transform.parent.position;
        transform.rotation = transform.parent.rotation;
        transform.parent = null;

        if (thisRenderer == null)
        {
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).GetComponent<Renderer>().enabled = true;
        }
        else
        {
            thisRenderer.enabled = true;
        }

        thisCollider.isTrigger = false;
        GetComponent<Rigidbody>().AddForce(transform.forward * 40, ForceMode.Force);
    }

    private void Interacting ()
    {
        if (!playerInventory.InventoryHasSpace())
            return;

        GameObject playerCamera = playerInventory.transform.FindChild("Camera").gameObject;

        PlayerInteraction interaction = playerCamera.GetComponent<PlayerInteraction>();

        interaction.interactTime = interactTime;
        interaction.interactLabel = "Picking up...";
    }

    private void Interacted ()
    {
        playerInventory.AddItem(this);
    }
}