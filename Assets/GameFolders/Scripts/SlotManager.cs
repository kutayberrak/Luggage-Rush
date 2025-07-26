using UnityEngine;
using System.Collections.Generic;

public class SlotManager : MonoBehaviour
{
    public static SlotManager Instance;

    public List<Slot> slots = new List<Slot>();

    private void Awake()
    {
        Instance = this;
    }

    public Slot GetFirstEmptySlot()
    {
        foreach (var slot in slots)
        {
            if (!slot.IsOccupied && !slot.IsReserved)
                return slot;
        }
        return null;
    }

    public bool ShiftRightFrom(int startIndex)
    {
        if (slots.Count <= 1 || startIndex < 0 || startIndex >= slots.Count)
            return false;

        if (slots[slots.Count - 1].IsOccupied || slots[slots.Count - 1].IsReserved)
            return false;

        for (int i = slots.Count - 2; i >= startIndex; i--)
        {
            Slot current = slots[i];
            Slot next = slots[i + 1];

            if (current.IsOccupied && !current.IsReserved)
            {
                next.CopyFrom(current);
                next.SetOccupied(true);
                next.SetReserved(false);
                current.ClearSlot();
            }
        }

        return true;
    }

    public void CheckForMatches(Slot changedSlot)
    {
        string id = changedSlot.StoredUniqueID;

        if (!changedSlot.IsOccupied || string.IsNullOrEmpty(id))
            return;

        List<Slot> matchGroup = new List<Slot>();

        for (int i = 0; i < slots.Count; i++)
        {
            Slot slot = slots[i];
            if (slot.IsOccupied && !slot.IsReserved && slot.StoredUniqueID == id)
            {
                matchGroup.Add(slot);
                if (matchGroup.Count >= 3)
                {
                    foreach (var m in matchGroup)
                        m.ClearSlot();

                    CompactSlots();
                    return;
                }
            }
            else
            {
                matchGroup.Clear();
            }
        }
    }

    public void CompactSlots()
    {
        List<(Sprite sprite, string id)> temp = new List<(Sprite, string)>();

        foreach (var slot in slots)
        {
            if (slot.IsOccupied)
                temp.Add((slot.iconImage.sprite, slot.StoredUniqueID));
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < temp.Count)
            {
                slots[i].FillSlot(temp[i].sprite, temp[i].id);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }

    public void ClearAllSlots()
    {
        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }
    }
}
