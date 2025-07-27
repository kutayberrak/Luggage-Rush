// Slot.cs
using UnityEngine;
using DG.Tweening;

public class Slot : MonoBehaviour
{
    [Header("Slot & Occupant Scale")]
    [Tooltip("Slot GameObject'inin başlangıç ölçeği")]
    public Vector3 defaultSlotScale = Vector3.one;

    [Tooltip("Yeni Assign edilirken Occupant'ın ölçeği")]
    public Vector3 defaultOccupantScale = Vector3.one;

    public bool IsOccupied { get; private set; }
    public bool IsReserved { get; private set; }
    public string StoredUniqueID { get; private set; }
    public GameObject Occupant { get; private set; }

    // **YENİ**: Slot durumu takibi
    private bool isProcessing = false;

    private void Awake()
    {
        // Slot'un kendi ölçeğini ayarla
        transform.localScale = defaultSlotScale;
    }

    public void ClearDataOnly()
    {
        // **YENİ**: İşlem sırasında temizlik yapma - shift işlemlerinde bu kontrolü kaldır
        // if (isProcessing) return;

        StoredUniqueID = null;
        IsOccupied = false;
        // **YENİ**: ClearDataOnly'de rezervasyonu silme, sadece data'yı temizle
        // IsReserved = false;
        Occupant = null;
    }

    public void ClearSlot()
    {
        // **YENİ**: İşlem sırasında temizlik yapma - shift işlemlerinde bu kontrolü kaldır
        // if (isProcessing) return;

        if (Occupant != null)
        {
            Occupant.SetActive(false);
            // parent değiştirme kaldırıldı
        }
        ClearDataOnly();
        // **YENİ**: ClearSlot'da rezervasyonu da temizle
        IsReserved = false;
    }

    public void AssignOccupant(GameObject obj, string id)
    {
        // **YENİ**: İşlem kilidi - shift sırasında bu kontrolü kaldır
        // if (isProcessing) return;

        // isProcessing = true;

        // **YENİ**: Slot'un gerçekten boş olduğunu kontrol et - shift sırasında bu kontrolü kaldır
        // if (IsOccupied || IsReserved)
        // {
        //     Debug.LogWarning($"[Slot] Attempting to assign occupant to occupied/reserved slot: {name}");
        //     isProcessing = false;
        //     return;
        // }

        ClearSlot();

        Occupant = obj;
        StoredUniqueID = id;
        IsOccupied = true;

        // Sadece pozisyonu güncelle, parent'a dokunma
        if (obj != null)
        {
            obj.transform.position = transform.position;
            obj.transform.localScale = defaultOccupantScale;
            obj.SetActive(true);
        }

        // isProcessing = false;
    }

    public void SetReserved(bool value)
    {
        // **YENİ**: İşlem sırasında rezervasyon değiştirme - shift sırasında bu kontrolü kaldır
        // if (isProcessing) return;

        // **YENİ**: Eğer slot zaten doluysa rezervasyon yapma - shift sırasında bu kontrolü kaldır
        // if (value && IsOccupied)
        // {
        //     Debug.LogWarning($"[Slot] Attempting to reserve occupied slot: {name}");
        //     return;
        // }

        IsReserved = value;
    }

    public void ResizeSlot(Vector3 newScale)
    {
        transform.localScale = newScale;
    }

    // **YENİ**: Slot durumunu kontrol et
    public bool IsAvailable()
    {
        return !IsOccupied && !IsReserved; // isProcessing kontrolünü kaldır
    }

    // **YENİ**: Slot'u tamamen temizle (emergency)
    public void ForceClear()
    {
        isProcessing = false;
        IsOccupied = false;
        IsReserved = false;
        StoredUniqueID = null;
        if (Occupant != null)
        {
            Occupant.SetActive(false);
        }
        Occupant = null;
    }
}
