using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public int verticalSlots;
    public int horizontalSlots;

    public float slotSizeX;
    public float slotSizeY;

    [HideInInspector]
    public bool showInventory;

    [HideInInspector]
    public bool userHasOption = false;

    public Texture2D emptySlotTexture;

    public KeyCode inventoryInput;

    public GUISkin guiSkin;

    public List<Slot> inventory;

    private bool dragging = false;

    private GameObject equipedObject = null;

    private Vector2 windowPosition = Vector2.zero;
    private Vector2 windowSize = Vector2.zero;
    private Vector2 clickMousePosition = Vector2.zero;

    private Slot dragSlot = new Slot();
    private Slot receiver = new Slot();

    private PlayerInteraction playerInteraction = null;

    // Represents an Inventory Slot.
    public struct Slot
    {
        public int id; // Identifier used to find this Slot.
        public int itemID;

        // Returns the actual amount of Items inside this Slot.
        public int stacks
        {
            get { return slotObjects.Count; }
        }

        // Returns if we have more then one Item inside this Slot.
        public bool stackableSlot
        {
            get { return slotObjects.Count > 1; }
        }

        public List<GameObject> slotObjects;

        public Texture2D slotTexture;

        public Rect originalRect;
        public Rect currentRect;

        // Returns a bool indicating if this slots original Rect contains <cursorPosition>.
        public bool Contains (Vector2 cursorPosition)
        {
            return this.originalRect.Contains(cursorPosition);
        }

        // Returns a bool indicating if this slot is outside the GUI Windows Rect.
        public bool OutsideWindow (Vector2 windowPosition, Vector2 windowSize)
        {
            Rect windowRect = new Rect(windowPosition.x, windowPosition.y, windowSize.x, windowSize.y);

            return !windowRect.Contains(Event.current.mousePosition);
        }
    }

    private void OnValidate ()
    {
        if (verticalSlots < 1)
            verticalSlots = 1;

        if (horizontalSlots < 1)
            horizontalSlots = 1;

        if (slotSizeY < 1)
            slotSizeY = 1;

        if (slotSizeX < 1)
            slotSizeX = 1;
    }

    private void Start ()
    {
        // Find our PlayerInteraction MonoBehaviour on our Player Camera GameObject.
        playerInteraction = transform.Find("Camera").GetComponent<PlayerInteraction>();

        // Inicialize our <inventory> List with a set <inventorySize> capacity.
        inventory = new List<Slot>(horizontalSlots * verticalSlots);

        // Calculate the <windowSize> based on the <slotSize> and the amount of slots.
        windowSize = new Vector2(horizontalSlots * slotSizeX, verticalSlots * slotSizeY);

        // Calculate the <windowPosition> based on <windowSize>.
        windowPosition = new Vector2((Screen.width - windowSize.x) / 2, (Screen.height - windowSize.y) / 2);

        // Our current (X,Y) positions inside our GUI Window.
        float currentX = windowPosition.x;
        float currentY = windowPosition.y;

        // Add an empty slot for each available slot in the <inventory>.
        for (int i = 0; i < inventory.Capacity; i++)
        {
            Slot emptySlot = new Slot();

            emptySlot.id = i;
            emptySlot.itemID = 0;
            emptySlot.slotObjects = new List<GameObject>();
            emptySlot.slotTexture = emptySlotTexture;
            emptySlot.originalRect = new Rect(currentX, currentY, slotSizeX, slotSizeY);
            emptySlot.currentRect = emptySlot.originalRect;

            // Move horizontally by <slotSize>.
            currentX += slotSizeX;

            // Check if we can fit another Slot horizontally.
            if (currentX + slotSizeX > windowPosition.x + windowSize.x)
            {
                // If we cannot fit another slot horizontally then move vertically and
                // reset our horizontal position.
                currentX = windowPosition.x;
                currentY += slotSizeY;
            }

            inventory.Add(emptySlot);
        }
    }

    private void Update ()
    {
        // Show or hide this inventory.
        if (Input.GetKeyDown(inventoryInput))
        {
            showInventory = !showInventory;

            Cursor.visible = showInventory;

            GetComponent<MouseRotation>().enabled = !showInventory;
            transform.Find("Camera").GetComponent<MouseRotation>().enabled = !showInventory;
        }
    }

    private void OnGUI ()
    {
        GUI.skin = guiSkin;

        if (showInventory)
        {
            // The Inventory's Header.
            GUI.Box(new Rect(windowPosition.x, windowPosition.y - 25, windowSize.x, 30), "Inventory");

            // The Inventory's Window.
            GUI.Box(new Rect(windowPosition.x, windowPosition.y, windowSize.x, windowSize.y), "");

            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i].stackableSlot)
                {
                    // If this slot is stackable then draw a label counting the amount of
                    // stacks and disable the ability to equip this stackable item.

                    string stackLabel = string.Format("<b><size=20>{0}</size></b>", inventory[i].stacks);

                    // Stackable items are not equipable.
                    GUI.Button(inventory[i].currentRect, inventory[i].slotTexture);

                    // Only show the stack amount label is we have more then 1 item stacked.
                    if (inventory[i].stacks > 0)
                        GUI.Label(inventory[i].currentRect, stackLabel);
                }
                else
                {
                    // If we click this Slot Button then equip the GameObject it holds.
                    if (GUI.Button(inventory[i].currentRect, inventory[i].slotTexture) && !userHasOption)
                    {
                        Item slotItem = inventory[i].slotObjects[0].GetComponent<Item>();

                        if (inventory[i].slotObjects.Count > 0 && slotItem.usable)
                        {
                            GameObject slotObject = inventory[i].slotObjects[0];

                            slotObject.SendMessage("InventoryUse", SendMessageOptions.DontRequireReceiver);

                            Destroy(inventory[i].slotObjects[inventory[i].slotObjects.Count - 1]);
                            inventory[i].slotObjects.RemoveAt(inventory[i].slotObjects.Count - 1);

                            if (inventory[i].slotObjects.Count < 1)
                                inventory[i] = ResetSlot(inventory[i]);
                        }
                    }
                }

                // If the mouse is inside this Slot then watch for dragging.
                if (inventory[i].Contains(Event.current.mousePosition) && !dragging)
                {
                    // If we click with the left mouse button set its click screen position;
                    if (Input.GetMouseButtonDown(0))
                    {
                        clickMousePosition = Event.current.mousePosition;
                    }

                    // If we clicked then moved our mouse that means we are dragging the slot.
                    if (Input.GetMouseButton(0) && Event.current.mousePosition != clickMousePosition)
                    {
                        // Prevent the Player from dragging empty slots.
                        if (inventory[i].slotObjects.Count > 0)
                        {
                            dragSlot = inventory[i];
                            dragging = true;
                        }
                    }
                }

                if (dragging)
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        // If we released our left mouse button and the Slots Rect is
                        // outside the Window, then drop the item it contained.
                        if (dragSlot.OutsideWindow(windowPosition, windowSize))
                        {
                            if (dragSlot.slotObjects.Count > 0)
                            {
                                Item item = null;

                                if (dragSlot.stackableSlot)
                                {
                                    // If we are a stackable Slot then we drop 1 of the stacked items.

                                    if (dragSlot.slotObjects.Count == 1)
                                    {
                                        // If we only have 1 GameObject left then drop it and restore this
                                        // slot to a normal empty slot.

                                        item = dragSlot.slotObjects[0].GetComponent<Item>();

                                        dragSlot = ResetSlot(dragSlot);
                                    }
                                    else
                                    {
                                        int lastItemIndex = dragSlot.slotObjects.Count - 1;

                                        item = dragSlot.slotObjects[lastItemIndex].GetComponent<Item>();

                                        dragSlot.slotObjects.RemoveAt(lastItemIndex);
                                    }
                                }
                                else
                                {
                                    // If the Slot we dragged out of the Window is not stackable
                                    // then just drop the item it contained.

                                    item = dragSlot.slotObjects[0].GetComponent<Item>();

                                    dragSlot = ResetSlot(dragSlot);

                                    if (equipedObject != null)
                                    {
                                        equipedObject.SetActive(false);
                                        equipedObject = null;
                                    }
                                }

                                item.DropItem();

                                // Since we are modifying this Slot inside the main foreach loop
                                // calling UpdateSlotData() would be extremelly slow due to a
                                // loop inside another loop.
                                inventory[inventory[i].id] = inventory[i];
                            }
                        }
                        else
                        {
                            // If we dropped the Item inside the Window, check what Slot we dropped
                            // it upon.

                            for (int y = 0; y < inventory.Count; y++)
                            {
                                // If this instance is the <dragSlot> itself then skip it.
                                if (inventory[y].id == dragSlot.id)
                                    continue;

                                // If we found a Slot that contains the mouse then exchange
                                // Slot information from the <dragSlot> to the <receiverSlot>
                                // and the <receiverSlot> to the <dragSlot>.
                                if (inventory[y].Contains(Event.current.mousePosition))
                                {
                                    receiver = inventory[y];
                                    userHasOption = true;
                                    break;
                                }
                            }
                        }

                        // If we got our left mouse button up return the <dragSlot>
                        // to its original position.
                        dragSlot.currentRect = dragSlot.originalRect;

                        // Update the <dragSlot>'s data.
                        UpdateSlotData(dragSlot);

                        // And since we released the left mouse button we are no longer dragging.
                        dragging = false;
                        break;
                    }

                    // If we are dragging then set the <dragSlot>'s position to be the current
                    // mouse cursor's position (Compensating for the <slotSize> offset).
                    dragSlot.currentRect.x = Event.current.mousePosition.x - 35;
                    dragSlot.currentRect.y = Event.current.mousePosition.y - 35;

                    // If the current slot being iterated is the <dragSlot> then update its info.
                    if (inventory[i].id == dragSlot.id)
                        inventory[inventory.IndexOf(inventory[i])] = dragSlot;
                }
            }

            if (userHasOption)
            {
                // Prompt the Player if he wants to create whatever the combination
                // of both items can create or just exchange Slots.

                if (receiver.slotObjects.Count == 0)
                {
                    if (dragSlot.stackableSlot && dragSlot.stacks > 1)
                    {
                        Rect button1Rect = new Rect(Screen.width - windowPosition.x,
                                                    Screen.height - windowPosition.y - 80, 150, 40);

                        Rect button2Rect = new Rect(Screen.width - windowPosition.x,
                                                    Screen.height - windowPosition.y - 40, 150, 40);

                        if (GUI.Button(button1Rect, "Move Stack"))
                        {
                            receiver.itemID = dragSlot.itemID;
                            receiver.slotObjects = dragSlot.slotObjects;
                            receiver.slotTexture = dragSlot.slotTexture;

                            dragSlot.itemID = 0;
                            dragSlot.slotObjects = new List<GameObject>();
                            dragSlot.slotTexture = emptySlotTexture;

                            UpdateSlotData(receiver);
                            UpdateSlotData(dragSlot);

                            userHasOption = false;
                        }

                        if (GUI.Button(button2Rect, "Seperate Stack"))
                        {
                            receiver.itemID = dragSlot.itemID;
                            receiver.slotObjects.Add(dragSlot.slotObjects[dragSlot.slotObjects.Count - 1]);
                            receiver.slotTexture = dragSlot.slotTexture;

                            dragSlot.slotObjects.RemoveAt(dragSlot.slotObjects.Count - 1);

                            UpdateSlotData(receiver);
                            UpdateSlotData(dragSlot);

                            userHasOption = false;
                        }
                    }
                    else
                    {
                        ExchangeSlots(dragSlot, receiver);

                        userHasOption = false;
                    }
                }
                else
                {
                    Rect button1Rect = new Rect(Screen.width - windowPosition.x,
                                                Screen.height - windowPosition.y - 120, 150, 40);

                    Rect button2Rect = new Rect(Screen.width - windowPosition.x,
                                                Screen.height - windowPosition.y - 80, 150, 40);

                    Rect button3Rect = new Rect(Screen.width - windowPosition.x,
                                                Screen.height - windowPosition.y - 40, 150, 40);

                    // GUI Button for combining 2 Slot items.
                    CraftingCombinations crafting = this.GetComponent<CraftingCombinations>();
                    string craftableItem = "";
                    int newItemID = 0;

                    foreach (CraftingCombinations.ItemCombination combination in crafting.itemCombinations)
                    {
                        int combinationSum = (combination.itemID1 * 10) + combination.itemID2;
                        int inventorySum = (dragSlot.itemID * 10) + receiver.itemID;

                        if (combinationSum == inventorySum)
                        {
                            craftableItem = combination.name;
                            newItemID = combinationSum;
                        }
                    }

                    if (newItemID != 0)
                    {
                        // GUI Button for crafting items.
                        if (GUI.Button(button3Rect, "Craft " + craftableItem))
                        {
                            GameObject craftedObject = Resources.Load<GameObject>(craftableItem);
                            GameObject craftedInstance = (GameObject)Instantiate(craftedObject);

                            craftedInstance.name = craftableItem;

                            CraftItem(craftedInstance, newItemID);
                        }
                    }

                    // GUI Button for swaping Slots.
                    if (GUI.Button(button2Rect, "Swap Items"))
                    {
                        ExchangeSlots(dragSlot, receiver);
                        userHasOption = false;
                    }

                    // GUI Button for stacking Slots.
                    if (receiver.itemID == dragSlot.itemID)
                    {
                        if (GUI.Button(button1Rect, "Stack Items"))
                        {
                            receiver.slotObjects.Add(dragSlot.slotObjects[dragSlot.slotObjects.Count - 1]);
                            dragSlot.slotObjects.RemoveAt(dragSlot.slotObjects.Count - 1);

                            if (dragSlot.slotObjects.Count < 1)
                            {
                                for (int i = 0; i < inventory.Count; i++)
                                {
                                    if (inventory[i].id == dragSlot.id)
                                    {
                                        inventory[i] = ResetSlot(inventory[i]);
                                        break;
                                    }
                                }
                            }

                            userHasOption = false;
                        }
                    }
                }
            }
        }
    }

    public void CraftItem (GameObject craftedObject, int newItemID)
    {
        Item craftedItem = craftedObject.GetComponent<Item>();

        // Remove the item from the <dragSlot>.
        if (dragSlot.slotObjects.Count > 0)
        {
            Destroy(dragSlot.slotObjects[dragSlot.slotObjects.Count - 1]);

            if (dragSlot.stacks > 1)
            {
                dragSlot.slotObjects.RemoveAt(dragSlot.slotObjects.Count - 1);
            }
            else
            {
                dragSlot = ResetSlot(dragSlot);
            }
        }

        // Remove the item from the <receiver> Slot.
        if (receiver.slotObjects.Count > 0)
        {
            Destroy(receiver.slotObjects[receiver.slotObjects.Count - 1]);

            if (receiver.stacks > 1)
            {
                receiver.slotObjects.RemoveAt(receiver.slotObjects.Count - 1);
            }
            else
            {
                receiver = ResetSlot(receiver);
            }

            craftedItem.PickupItem(this.transform);

            if (InventoryHasSpace())
            {
                Slot slot = new Slot();

                for (int i = 0; i < inventory.Count; i++)
                {
                    slot = inventory[i];

                    if (slot.itemID == 0)
                    {
                        slot.itemID = newItemID;
                        slot.slotTexture = craftedItem.slotIcon;
                        slot.slotObjects.Add(craftedObject);
                        break;
                    }
                }

                UpdateSlotData(slot);
            }
            else
            {
                craftedItem.DropItem();
            }
        }

        UpdateSlotData(receiver);
        UpdateSlotData(dragSlot);

        userHasOption = false;
    }

    public bool InventoryHasSpace()
    {
        foreach (Slot slot in inventory)
        {
            if (slot.slotObjects.Count == 0)
                return true;
        }

        return false;
    }

    public void AddItem (Item item)
    {
        if (!InventoryHasSpace())
        {
            Debug.Log("No available slots in inventory for " + item.name + " to be added.");
            return;
        }

        Slot editSlot = new Slot();

        // If this item is stackable then find a Slot with its similar
        // GameObject type and stack it, if there is none, just add it.
        if (item.maxStackAmount > 1)
        {
            // Search for any stackable Slots with the same GameObject type as 
            // the one we want to add.
            List<Slot> stackableSlots = FindSlot(item.itemID);

            if (stackableSlots.Count > 0)
            {
                foreach (Slot stackableSlot in stackableSlots)
                {
                    if (stackableSlot.stacks < item.maxStackAmount)
                    {
                        // If we found a stackable Slot with the same GameObject type as
                        // the one we want to add, then add another stack to that slot.

                        // If this is the Slots first stack set it to stackable.
                        stackableSlot.slotObjects.Add(playerInteraction.currentObject);

                        editSlot = stackableSlot;
                        break;
                    }
                }

                if (editSlot.itemID == 0)
                    editSlot = AddToEmptySlot(item);
            }
            else
            {
                // If we do not already contain this GameObject type then just add it.
                editSlot = AddToEmptySlot(item);
            }
        }
        else
        {
            // If this item is not stackable then just add it to an empty Slot.
            editSlot = AddToEmptySlot(item);
        }

        // Pickup the item we were interacting with.
        item.PickupItem(this.transform);

        // Update the actual inventory Slot.
        UpdateSlotData(editSlot);
    }

    private Slot ResetSlot (Slot slot)
    {
        slot.itemID = 0;
        slot.slotObjects = new List<GameObject>();
        slot.slotTexture = emptySlotTexture;

        return slot;
    }

    // Exchanges information between the <sender> and <receiver> Slots.
    private void ExchangeSlots(Slot sender, Slot receiver)
    {
        // Check if the <sender> has any information to exchange, otherwise
        // there is no reason to exchange Nothing for Nothing.
        if (sender.slotObjects.Count > 0)
        {
            Slot oldSender = sender;

            // Change the <dragSlot>'s information to the ones the <receiver> Slot had.
            sender.itemID = receiver.itemID;
            sender.slotObjects = receiver.slotObjects;
            sender.slotTexture = receiver.slotTexture;

            // Change the <receiver>'s information to the ones the <dragSlot> had.
            receiver.itemID = oldSender.itemID;
            receiver.slotObjects = oldSender.slotObjects;
            receiver.slotTexture = oldSender.slotTexture;

            UpdateSlotData(sender);
            UpdateSlotData(receiver);
        }
    }

    // Finds the next available empty Slot.
    private Slot FindSlot()
    {
        foreach (Slot slot in inventory)
        {
            if (slot.slotObjects.Count == 0)
                return slot;
        }

        return new Slot();
    }

    // Finds the slot that contains a GameObject with the same
    // type as the <objectName>.
    private List<Slot> FindSlot(int objectID)
    {
        List<Slot> foundSlots = new List<Slot>();

        foreach (Slot slot in inventory)
        {
            if (slot.itemID == objectID)
                foundSlots.Add(slot);
        }

        return foundSlots;
    }

    // Finds the <target> Slot inside the Inventory by using its id.
    private void UpdateSlotData (Slot target)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].id == target.id)
                inventory[i] = target;
        }
    }

    private Slot AddToEmptySlot (Item item)
    {
        Slot editSlot = FindSlot();

        if (editSlot.slotObjects.Count == 0)
        {
            // Add the GameObject we were interacting with to the slot.
            editSlot.slotObjects.Add(playerInteraction.currentObject);
            editSlot.slotTexture = item.slotIcon;
            editSlot.itemID = item.itemID;
        }

        return editSlot;
    }
}