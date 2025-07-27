using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Slot : MonoBehaviour
{
    public Image iconImage; // Reference to the Image component that displays the item's icon.

    private bool isOccupied = false;
    public bool IsOccupied => isOccupied; // Read-only property for occupancy status.

    private bool isReserved = false;
    public bool IsReserved => isReserved; // Read-only property for reservation status.

    public string StoredUniqueID; // Stores the unique identifier of the item in this slot.

    /// <summary>
    /// Ensures the slot starts at its default scale (Vector3.one).
    /// </summary>
    private void Awake()
    {
        transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Sets the reservation status of the slot. A reserved slot cannot be used as an empty slot.
    /// </summary>
    /// <param name="value">True to reserve the slot, false otherwise.</param>
    public void SetReserved(bool value)
    {
        isReserved = value;
    }

    /// <summary>
    /// Sets the occupied status of the slot.
    /// </summary>
    /// <param name="value">True if the slot is occupied, false otherwise.</param>
    public void SetOccupied(bool value)
    {
        isOccupied = value;
    }

    /// <summary>
    /// Sets the unique ID of the item stored in this slot.
    /// </summary>
    /// <param name="id">The unique ID string.</param>
    public void SetStoredID(string id)
    {
        StoredUniqueID = id;
    }

    /// <summary>
    /// Fills the slot with a new item's sprite and unique ID.
    /// Also triggers a small "punch" animation.
    /// </summary>
    /// <param name="sprite">The sprite to display.</param>
    /// <param name="id">The unique ID of the item.</param>
    public void FillSlot(Sprite sprite, string id)
    {
        iconImage.sprite = sprite;
        iconImage.enabled = true; // Make sure the image is visible.
        StoredUniqueID = id;
        isOccupied = true;
        isReserved = false; // Ensure a newly filled slot is not reserved.
        //StartCoroutine(PunchScale()); // Play a visual feedback animation.
    }

    /// <summary>
    /// Clears the slot, removing its item and resetting its state.
    /// </summary>
    public void ClearSlot()
    {
        iconImage.sprite = null;
        iconImage.enabled = false; // Hide the image.
        StoredUniqueID = null; // Clear the ID.
        isOccupied = false; // Mark as unoccupied.
        isReserved = false; // Mark as unreserved.
        transform.localScale = Vector3.one; // Reset scale to default.
    }

    /// <summary>
    /// Copies the content (sprite and ID) from another slot to this slot.
    /// </summary>
    /// <param name="other">The Slot object to copy from.</param>
    public void CopyFrom(Slot other)
    {
        // Ensure the slot is cleared and reset to default scale before copying new content.
        ClearSlot();
        // Fill the slot with the copied content.
        FillSlot(other.iconImage.sprite, other.StoredUniqueID);
    }

    /// <summary>
    /// Coroutine for a quick punch-scale animation when the slot is filled.
    /// Ensures the slot always returns to its default scale (Vector3.one).
    /// </summary>
    private IEnumerator PunchScale()
    {
        // Animasyonun her zaman temel boyutu (Vector3.one) referans almasını sağla.
        Vector3 originalScale = Vector3.one;
        Vector3 punchScale = originalScale * 1.2f; // Punch animesi için büyütme miktarı

        transform.localScale = punchScale; // Anlık olarak büyüt
        yield return new WaitForSeconds(0.1f); // Kısa bir süre bekle
        transform.localScale = originalScale; // Temel boyuta geri dön
    }
}