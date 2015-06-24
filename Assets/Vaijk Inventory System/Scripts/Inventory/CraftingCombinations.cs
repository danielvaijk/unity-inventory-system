using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

public class CraftingCombinations : MonoBehaviour
{
    public List<ItemCombination> itemCombinations;

    [Serializable]
    public struct ItemCombination
    {
        public string name;

        public int itemID1;
        public int itemID2;
    }
}