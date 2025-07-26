using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SlotManager : MonoBehaviour
{
    public static SlotManager Instance;

    public List<Slot> slots = new List<Slot>();

    void Awake()
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

    public Slot GetMatchingSlot(Sprite itemSprite)
    {
        foreach (var slot in slots)
        {
            if (slot.IsOccupied && slot.iconImage.sprite == itemSprite)
                return slot;
        }
        return null;
    }

    public bool ShiftRightFrom(int startIndex)
    {
        if (slots.Count <= 1 || startIndex < 0 || startIndex >= slots.Count)
        {
            Debug.LogWarning("Geçersiz slot indeksi veya slot listesi küçük.");
            return false;
        }

        // Eğer en sağ slot doluysa → kaydırma yapılamaz
        if (slots[slots.Count - 1].IsOccupied)
        {
            return false;
        }

        // Sağdan sola kaydır: i = slots.Count - 2 → startIndex
        for (int i = slots.Count - 2; i >= startIndex; i--)
        {
            Slot current = slots[i];
            Slot next = slots[i + 1];

            if (current.IsOccupied && !current.IsReserved)
            {
                next.CopyFrom(current);
                current.ClearSlot();
            }
        }

        return true;
    }

    public void ClearAllSlots()
    {
        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }
    }
    public void CheckForMatches(Slot changedSlot)
    {
        string targetID = changedSlot.StoredUniqueID;

        if (!changedSlot.IsOccupied || string.IsNullOrEmpty(targetID))
            return;

        List<Slot> consecutiveMatches = new List<Slot>();

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsOccupied && !slots[i].IsReserved && slots[i].StoredUniqueID == targetID)
            {
                consecutiveMatches.Add(slots[i]);

                if (consecutiveMatches.Count >= 3)
                {
                    // Eşleşme bulundu
                    Debug.Log($"{consecutiveMatches.Count} adet {targetID} eşleşmesi bulundu! Temizleniyor...");
                    foreach (var matchSlot in consecutiveMatches)
                    {
                        matchSlot.ClearSlot();
                    }
                    CompactSlots();
                    return; // sadece ilk eşleşme temizleniyor
                }
            }
            else
            {
                consecutiveMatches.Clear(); // zincir bozuldu
            }
        }
    }


    public void CompactSlots()
    {
        List<(Sprite sprite, string uniqueId)> tempOccupiedData = new List<(Sprite, string)>();

        foreach (var slot in slots)
        {
            if (slot.IsOccupied)
            {
                tempOccupiedData.Add((slot.iconImage.sprite, slot.StoredUniqueID));
            }
        }
        for (int i = 0; i < tempOccupiedData.Count; i++)
        {
            slots[i].FillSlotWithSprite(tempOccupiedData[i].sprite);
            slots[i].StoredUniqueID = tempOccupiedData[i].uniqueId;
        }

        for (int i = tempOccupiedData.Count; i < slots.Count; i++)
        {
            slots[i].ClearSlot();
        }
    }

    public Slot GetLatestTargetSlot(string uniqueId)
    {
        // Aynı ID'ye sahip ilk slotu bul
        foreach (var slot in slots)
        {
            if (slot.IsOccupied && slot.StoredUniqueID == uniqueId)
            {
                return slot;
            }
        }

        // Yoksa en sağda boş slot bul
        return GetFirstEmptySlot();
    }
}