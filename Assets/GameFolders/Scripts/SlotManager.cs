using UnityEngine;
using System.Collections.Generic;

public class SlotManager : MonoBehaviour
{
    public static SlotManager Instance;

    public List<Slot> slots = new List<Slot>(); // Slot s�ras�n� burada tut

    void Awake()
    {
        Instance = this;
    }

    // Bo� slot d�nd�r�r (e�le�me yoksa)
    public Slot GetFirstEmptySlot()
    {
        foreach (var slot in slots)
        {
            if (!slot.IsOccupied)
                return slot;
        }
        return null;
    }

    // E�le�en slot varsa onu d�nd�r
    public Slot GetMatchingSlot(Sprite itemSprite)
    {
        foreach (var slot in slots)
        {
            if (slot.IsOccupied && slot.iconImage.sprite == itemSprite)
                return slot;
        }
        return null;
    }

    // Sa�a kayd�rma i�lemi (e�le�enSlot�tan itibaren)
    public void ShiftRightFrom(Slot startSlot)
    {
        int index = slots.IndexOf(startSlot);
        for (int i = slots.Count - 2; i >= index; i--)
        {
            var current = slots[i];
            var next = slots[i + 1];

            if (current.IsOccupied)
            {
                next.FillSlotWithSprite(current.iconImage.sprite);
                current.ClearSlot();
            }
        }
    }
}
