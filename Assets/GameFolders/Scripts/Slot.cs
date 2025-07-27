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

    private void Awake()
    {
        // Slot'un kendi ölçeğini ayarla
        transform.localScale = defaultSlotScale;
    }

    public void ClearDataOnly()
    {
        StoredUniqueID = null;
        IsOccupied = false;
        IsReserved = false;
        Occupant = null;
    }

    public void ClearSlot()
    {
        if (Occupant != null)
        {
            Occupant.SetActive(false);
            // parent değiştirme kaldırıldı
        }
        ClearDataOnly();
    }

    public void AssignOccupant(GameObject obj, string id)
    {
        ClearSlot();

        Occupant = obj;
        StoredUniqueID = id;
        IsOccupied = true;

        // Sadece pozisyonu güncelle, parent'a dokunma
        obj.transform.position = transform.position;
        obj.transform.localScale = defaultOccupantScale;
        obj.SetActive(true);
    }

    public void SetReserved(bool value)
    {
        IsReserved = value;
    }

    public void ResizeSlot(Vector3 newScale)
    {
        transform.localScale = newScale;
    }
}
