using UnityEngine;
using System.Collections.Generic;
using System.Collections; // Coroutine'ler için gerekli
using UnityEngine.UI;

public class SlotManager : MonoBehaviour
{
    public static SlotManager Instance;

    public List<Slot> slots = new List<Slot>();

    private void Awake()
    {
        // Ensure only one instance of SlotManager exists.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances.
        }
    }

    /// <summary>
    /// Finds and returns the first unoccupied and unreserved slot.
    /// </summary>
    /// <returns>The first empty slot, or null if none are found.</returns>
    public Slot GetFirstEmptySlot()
    {
        foreach (var slot in slots)
        {
            if (!slot.IsOccupied && !slot.IsReserved)
                return slot;
        }
        return null;
    }

    /// <summary>
    /// Shifts all occupied elements from startIndex one position to the right.
    /// The element at the last slot will be pushed out if no empty slot exists further right.
    /// The slot at startIndex will be cleared.
    /// </summary>
    /// <param name="startIndex">The index from which to start shifting elements.</param>
    /// <returns>True if the shift operation was performed, false if startIndex is invalid.</returns>
    public bool ShiftRightFrom(int startIndex)
    {
        // Basic validation for startIndex.
        // If startIndex is out of bounds, or if there are no slots, return false.
        if (startIndex < 0 || startIndex >= slots.Count)
        {
            Debug.LogError($"ShiftRightFrom called with invalid startIndex: {startIndex}. Slots count: {slots.Count}");
            return false;
        }

        // We iterate from the second-to-last slot down to the startIndex.
        // Each slot's content is copied to the slot immediately to its right.
        // This effectively moves everything from startIndex onwards, one step to the right.
        // The item in the last slot will be overwritten or "pushed out" if it's occupied.
        for (int i = slots.Count - 2; i >= startIndex; i--)
        {
            Slot current = slots[i];
            Slot next = slots[i + 1];

            // If the current slot is occupied, copy its content to the next slot.
            // This includes the sprite, ID, and occupied status.
            if (current.IsOccupied)
            {
                next.CopyFrom(current);
            }
            else
            {
                // If the current slot is empty, ensure the 'next' slot (which received content
                // from a slot further left during this shift operation, or was empty)
                // is also cleared, maintaining the empty space if it was propagated.
                next.ClearSlot();
            }
        }

        // After the loop, the slot at 'startIndex' will have had its content moved
        // to 'startIndex + 1'. We now clear 'startIndex' to make it ready for the new item.
        slots[startIndex].ClearSlot();

        return true;
    }

    /// <summary>
    /// Rearranges slots to remove gaps, moving all occupied slots to the left.
    /// </summary>
    public void CompactSlots()
    {
        // Temporarily store occupied slot data.
        List<(Sprite sprite, string id)> occupiedItems = new List<(Sprite, string)>();

        foreach (var slot in slots)
        {
            if (slot.IsOccupied)
                occupiedItems.Add((slot.iconImage.sprite, slot.StoredUniqueID));
        }

        // Fill slots from the beginning with the compacted items.
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < occupiedItems.Count)
            {
                slots[i].FillSlot(occupiedItems[i].sprite, occupiedItems[i].id);
            }
            else
            {
                // Clear any remaining slots after all occupied items have been moved.
                slots[i].ClearSlot();
            }
        }
    }

    /// <summary>
    /// Determines the best valid slot to insert a new object based on new logic.
    /// If an item with the same ID exists, it returns the index of the first occurrence
    /// (implying insertion *before* that block). If no match, returns the first empty slot.
    /// </summary>
    /// <param name="id">The unique ID of the object to be inserted.</param>
    /// <returns>The calculated insertion index, or -1 if no suitable slot is found.</returns>
    public Slot GetCurrentValidInsertSlot(string id)
    {
        int index = FindInsertIndex(id);
        if (index >= 0 && index < slots.Count)
            return slots[index];

        return null;
    }

    /// <summary>
    /// Converts a UI RectTransform position to a world position for moving objects.
    /// </summary>
    /// <param name="slot">The target slot.</param>
    /// <param name="canvas">The main Canvas the UI element belongs to.</param>
    /// <returns>The world position of the slot.</returns>
    public Vector3 GetSlotWorldPosition(Slot slot, Canvas canvas)
    {
        RectTransform rect = slot.GetComponent<RectTransform>();
        // Determine the camera based on Canvas Render Mode.
        // ScreenSpaceOverlay canvases don't use a camera for UI positioning.
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        // Convert the UI element's screen position to world position.
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, rect.position);
        // We need a Z-coordinate for ScreenToWorldPoint. Use a fixed distance from the main camera.
        // Bu z değeri, UI elementini 3D uzayda doğru bir şekilde konumlandırmak içindir.
        float z = Mathf.Abs(Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, z));
    }

    /// <summary>
    /// Attempts to place a new object into the slot system based on insertion rules.
    /// This method now handles shifting existing items to make space if needed.
    /// It also orchestrates match checking, animation, and compaction.
    /// </summary>
    /// <param name="id">The unique ID of the object to place.</param>
    /// <param name="sprite">The sprite of the object to place.</param>
    /// <param name="sourceCanvas">The Canvas used for UI position conversions.</param>
    public void TryPlaceObject(string id, Sprite sprite, Canvas sourceCanvas)
    {
        // Tüm yerleştirme ve eşleşme sürecini bir coroutine üzerinden yönet.
        StartCoroutine(ProcessPlacementAndMatches(id, sprite, sourceCanvas));
    }

    /// <summary>
    /// Determines the index where a new object should be inserted.
    /// Priority:
    /// 1. The first index where an item with the same ID already exists (inserting BEFORE that block).
    /// 2. The first completely empty slot.
    /// </summary>
    /// <param name="id">The unique ID of the object to insert.</param>
    /// <returns>The calculated insertion index, or -1 if no suitable slot is found.</returns>
    private int FindInsertIndex(string id)
    {
        // First, iterate to find if an item with the same ID already exists.
        // If it does, we want to insert the new item right before the first occurrence of that ID.
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsOccupied && slots[i].StoredUniqueID == id)
            {
                // Found a matching ID. This is where we want to insert, shifting subsequent items right.
                return i;
            }
        }

        // If no matching ID was found in the slots, then find the first empty slot available.
        // This will be the insertion point.
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsOccupied) // We don't care about 'IsReserved' here for finding a general empty spot.
            {
                return i;
            }
        }

        // If no matching ID was found and no empty slot is available, then we cannot place the object.
        return -1;
    }

    /// <summary>
    /// Clears all slots in the manager.
    /// </summary>
    public void ClearAllSlots()
    {
        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }
    }

    /// <summary>
    /// Manages the full process of placing an object, checking for matches, animating, and compacting.
    /// This is a coroutine to allow for delays and animations.
    /// </summary>
    /// <param name="id">The unique ID of the object to place.</param>
    /// <param name="sprite">The sprite of the object to place.</param>
    /// <param name="sourceCanvas">The Canvas used for UI position conversions.</param>
    private IEnumerator ProcessPlacementAndMatches(string id, Sprite sprite, Canvas sourceCanvas)
    {
        int insertIndex = FindInsertIndex(id);
        if (insertIndex == -1)
        {
            Debug.Log($"Cannot place object '{id}': No suitable slot found.");
            yield break; // Exit coroutine
        }

        // If the calculated insertIndex is already occupied, it means we need to shift
        // existing elements to the right to make space for the new item at this position.
        if (slots[insertIndex].IsOccupied)
        {
            bool shifted = ShiftRightFrom(insertIndex);
            if (!shifted)
            {
                Debug.LogWarning($"Failed to shift elements for object '{id}' at index {insertIndex}.");
                yield break; // Exit coroutine
            }
        }

        // Now that the 'insertIndex' slot is guaranteed to be empty, fill it with the new object.
        slots[insertIndex].FillSlot(sprite, id);

        // Initial match check and animation (if any match is found)
        yield return StartCoroutine(CheckForMatchesCoroutine(sourceCanvas));

        // After potential matches are cleared (and animated), compact the slots.
        CompactSlots();

        // Check for new matches after compaction (this handles cascades)
        yield return StartCoroutine(CheckForMatchesCoroutine(sourceCanvas));
    }

    /// <summary>
    /// Checks for and clears groups of 3 or more identical, consecutive items.
    /// This is a coroutine to allow for animation and delays during match clearance.
    /// </summary>
    /// <param name="sourceCanvas">The Canvas used for UI position conversions.</param>
    private IEnumerator CheckForMatchesCoroutine(Canvas sourceCanvas)
    {
        if (slots.Count < 3)
            yield break; // Nothing to check

        int count = 1; // Tracks consecutive identical items
        for (int i = 1; i < slots.Count; i++)
        {
            // Check if current slot and previous slot are occupied and have the same unique ID.
            if (slots[i].IsOccupied && slots[i - 1].IsOccupied &&
                slots[i].StoredUniqueID == slots[i - 1].StoredUniqueID)
            {
                count++;
            }
            else // Mismatch or empty slot, so evaluate the current sequence.
            {
                if (count >= 3)
                {
                    // Found a match, initiate animation and clear.
                    // This will yield until AnimateMatchClearance is done.
                    yield return StartCoroutine(AnimateMatchClearance(i - count, count, sourceCanvas));
                    // After clearing a match, we can stop and let ProcessPlacementAndMatches
                    // handle the next steps (compacting and re-checking for cascades).
                    yield break;
                }
                count = 1; // Reset count for the new sequence.
            }
        }

        // After the loop, check if there's a match at the very end of the slots list.
        if (count >= 3)
        {
            yield return StartCoroutine(AnimateMatchClearance(slots.Count - count, count, sourceCanvas));
        }
    }

    /// <summary>
    /// Animates the matched items consolidating to the first slot of the match, then clears them.
    /// </summary>
    /// <param name="startIndex">The starting index of the matched sequence.</param>
    /// <param name="count">The number of items in the matched sequence.</param>
    /// <param name="sourceCanvas">The Canvas used for UI position conversions.</param>
    private IEnumerator AnimateMatchClearance(int startIndex, int count, Canvas sourceCanvas)
    {
        /* float animationDuration = 0.25f; // Animasyon süresi (saniye)
         float delayAfterAnimation = 0.1f; // Animasyon bittikten sonraki ek bekleme süresi

         // Eşleşen tüm ikonları canlandırmak için topla
         List<Image> imagesToAnimate = new List<Image>();
         List<Vector3> initialPositions = new List<Vector3>(); // İkonların başlangıç dünya pozisyonları

         // Hedef slotun dünya pozisyonunu al (eşleşen grubun ilk slotu)
         // Tüm eşleşen ikonlar bu slota doğru kayacak.
         Vector3 targetWorldPos = GetSlotWorldPosition(slots[startIndex], sourceCanvas);

         for (int i = startIndex; i < startIndex + count; i++)
         {
             if (slots[i].IsOccupied)
             {
                 imagesToAnimate.Add(slots[i].iconImage);
                 initialPositions.Add(slots[i].iconImage.transform.position); // Mevcut dünya pozisyonunu kaydet
             }
         }

         float timer = 0f;
         while (timer < animationDuration)
         {
             timer += Time.deltaTime;
             float t = timer / animationDuration; // Normalleştirilmiş zaman (0'dan 1'e)

             for (int i = 0; i < imagesToAnimate.Count; i++)
             {
                 if (imagesToAnimate[i] != null)
                 {
                     // Her ikonu başlangıç pozisyonundan hedef pozisyona doğru Lerp ile hareket ettir.
                     // Bu, "kaydırma" (swipe) efektini oluşturur.
                     imagesToAnimate[i].transform.position = Vector3.Lerp(initialPositions[i], targetWorldPos, t);
                 }
             }
             yield return null; // Bir sonraki frame'e kadar bekle
         }

         // Animasyon sonunda ikonların tam olarak hedef pozisyonda olduğundan emin ol
         for (int i = 0; i < imagesToAnimate.Count; i++)
         {
             if (imagesToAnimate[i] != null)
             {
                 imagesToAnimate[i].transform.position = targetWorldPos;
             }
         }*/

        //yield return new WaitForSeconds(delayAfterAnimation); // Animasyon sonrası kısa bir bekleme
        yield return new WaitForSeconds(.2f);
        // Şimdi slotları temizle
        for (int j = startIndex; j < startIndex + count; j++)
        {
            slots[j].ClearSlot();
        }
    }
}