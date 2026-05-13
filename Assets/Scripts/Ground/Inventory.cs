using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public DialougeSO dialogue;
    public bool FoundFirstScrap = false;

    public List<Item> items = new List<Item>();
    public static event Action<Item> OnItemAdded;
    public static event Action OnInventoryChanged;

    public void AddItem(Item item)
    {
        item.OnItemPickUp();
        OnItemAdded?.Invoke(item);
        
        if(!item.isStorable)
        return;

        items.Add(item);
        OnInventoryChanged?.Invoke();
        
    }

    public int GetScrapCount()
    {
        int scrapCount = 0;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] is Scrap)
            {
                scrapCount++;
            }
        }

        return scrapCount;
    }

    public void ClearScrap()
    {
        int removedCount = items.RemoveAll(item => item is Scrap);
        if (removedCount > 0)
        {
            OnInventoryChanged?.Invoke();
        }
    }

    public void StartFirstScrapFoundEvent()
    {
        DialogueTextManager.Instance.currentDialouge = dialogue;
        DialogueTextManager.Instance.StartDialouge();
    }
}
