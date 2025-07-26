using UnityEngine;
using System.Collections.Generic;
using System.Linq; // LINQ kullanmak için gerekli

public class SlotManager : MonoBehaviour
{
    public static SlotManager Instance;

    public List<Slot> slots = new List<Slot>(); // Slot sırasını burada tut

    void Awake()
    {
        Instance = this;
    }

    // Boş slot döndürür (eşleşme yoksa)
    public Slot GetFirstEmptySlot()
    {
        foreach (var slot in slots)
        {
            if (!slot.IsOccupied)
                return slot;
        }
        return null; // Tüm slotlar doluysa null döner
    }

    // Eşleşen slot varsa onu döndür
    public Slot GetMatchingSlot(Sprite itemSprite)
    {
        foreach (var slot in slots)
        {
            if (slot.IsOccupied && slot.iconImage.sprite == itemSprite)
                return slot;
        }
        return null;
    }

    // Sağa kaydırma işlemi (belirli bir başlangıç slotundan itibaren)
    public void ShiftRightFrom(Slot startSlot)
    {
        // Liste boşsa veya sadece bir eleman varsa kaydırma yapmaya gerek yok
        if (slots.Count <= 1)
        {
            Debug.LogWarning("Slot listesi kaydırma için yeterli değil.");
            return;
        }

        int index = slots.IndexOf(startSlot);

        // Eğer başlangıç slotu listede yoksa veya son slot ise kaydırma yapmaya gerek yok
        if (index == -1 || index == slots.Count - 1)
        {
            // Debug.LogWarning("Başlangıç slotu bulunamadı veya son slot. Sağa kaydırma yapılamadı.");
            return; // Hata yerine sadece uyarı verip çıkabiliriz
        }

        // Sondan başlayarak index'e kadar olan slotları sağa kaydır
        for (int i = slots.Count - 2; i >= index; i--)
        {
            var current = slots[i];
            var next = slots[i + 1];

            if (current.IsOccupied)
            {
                // Slot içeriğini kopyala (sprite ve uniqueId)
                next.FillSlotWithSprite(current.iconImage.sprite);
                next.StoredUniqueID = current.StoredUniqueID; // Unique ID'yi de kopyala!
                current.ClearSlot(); // Eski slotu temizle
            }
            else
            {
                // Mevcut slot boşsa, yanındaki dolu slotu çekmek yerine sadece temizlemek yeterli
                current.ClearSlot();
            }
        }
    }

    // Slotları tamamen temizle (örneğin oyun bittiğinde veya yeniden başlatıldığında)
    public void ClearAllSlots()
    {
        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }
    }

    // YENİ METOT: Eşleşmeleri kontrol et ve temizle
    public void CheckForMatches(Slot changedSlot)
    {
        // Yereleşen objenin ID'sini al
        string changedSlotID = changedSlot.StoredUniqueID;

        // Eğer yerleşen slot boşsa veya ID'si yoksa kontrol etmeye gerek yok
        if (!changedSlot.IsOccupied || string.IsNullOrEmpty(changedSlotID))
        {
            return;
        }

        // Aynı ID'ye sahip tüm dolu slotları bul
        List<Slot> matchingSlots = new List<Slot>();
        foreach (var slot in slots)
        {
            if (slot.IsOccupied && slot.StoredUniqueID == changedSlotID)
            {
                matchingSlots.Add(slot);
            }
        }

        // Eğer 3 veya daha fazla eşleşen slot varsa
        if (matchingSlots.Count >= 3)
        {
            Debug.Log($"{matchingSlots.Count} adet {changedSlotID} eşleşmesi bulundu! Temizleniyor...");
            foreach (var slotToClear in matchingSlots)
            {
                slotToClear.ClearSlot(); // Eşleşen slotu temizle
            }
            // Slotlar temizlendikten sonra, boşlukları doldurmak için sola kaydırma gerekebilir
            // Buraya yeni bir "ShiftLeft" veya "CompactSlots" metodu ekleyebilirsiniz.
            CompactSlots(); // Slotları sola doğru sıkıştır

            // Not: Eğer birden fazla 3'lü eşleşme aynı anda temizleniyorsa,
            // (örneğin 6 tane aynı anda geldiyse ve 2 tane 3'lü oluştuysa)
            // bu mevcut mantık doğru çalışacaktır.
            // Puanlama veya efekt gibi ek mantıkları buraya ekleyebilirsiniz.
        }
    }

    // YENİ METOT: Boşlukları doldurmak için slotları sola sıkıştır
    // SlotManager.cs içindeki CompactSlots metodu
    public void CompactSlots()
    {
        // Geçici bir liste oluşturacağız. Bu listeye dolu slotların verilerini kopyalayacağız.
        // Boş slotların verileriyle uğraşmaya gerek yok, onlar sonra temizlenecek.
        List<(Sprite sprite, string uniqueId)> tempOccupiedData = new List<(Sprite, string)>();

        // Dolu slotların verilerini (sprite ve uniqueId) geçici listeye kopyala
        foreach (var slot in slots)
        {
            if (slot.IsOccupied)
            {
                tempOccupiedData.Add((slot.iconImage.sprite, slot.StoredUniqueID));
            }
        }

        // Şimdi orijinal 'slots' listesini baştan sona düzenleyeceğiz.
        // Önce tüm dolu slotları yerleştir, sonra kalanları temizle.

        // Dolu slot verilerini başa yerleştir
        for (int i = 0; i < tempOccupiedData.Count; i++)
        {
            slots[i].FillSlotWithSprite(tempOccupiedData[i].sprite);
            slots[i].StoredUniqueID = tempOccupiedData[i].uniqueId;
        }

        // Kalan slotları (yani önceden dolu olup şimdi boş olan yerler veya zaten boş olan yerler) temizle
        for (int i = tempOccupiedData.Count; i < slots.Count; i++)
        {
            slots[i].ClearSlot();
        }
    }
}