using UnityEngine;

public class ClickableObject : MonoBehaviour
{
    public string uniqueId; // The unique ID of this clickable object.
    public Sprite objectSprite; // The sprite representation of this object.
    public Canvas mainCanvas; // Reference to the main UI Canvas.

    private bool isFlying = false; // True if the object is currently moving towards a slot.
    private Slot currentTargetSlot; // The slot this object is currently flying towards.

    public float moveSpeed = 5f; // Speed at which the object moves towards a slot.
    public float stopDistance = 0.05f; // Initial distance to target before it's considered "placed".

    private float realStopDistance; // The calculated stop distance, potentially adjusted by rapid clicks.

    // Static variables to manage click timing for dynamic stop distance.
    public static float lastClickTime = -100f; // Stores the Time.time of the last click.
    public static float clickDelayThreshold = 0.3f; // If clicks are faster than this, add to stop distance.
    public static float addedStopPerClick = 0.2f; // Amount added to stop distance for rapid clicks.
    public static float baseStopDistance = 0.05f; // Minimum stop distance.
    public static float maxStopDistance = 0.6f; // Maximum stop distance to prevent excessive "hang time".

    void Update()
    {
        // Only proceed if the object is in "flying" mode.
        if (!isFlying) return;

        // Continuously get the most up-to-date valid target slot.
        // This is crucial because the slot the object targets might change
        // due to other items being placed or matches occurring.
        Slot newTarget = SlotManager.Instance.GetCurrentValidInsertSlot(uniqueId);

        if (newTarget != null)
        {
            currentTargetSlot = newTarget; // Update the target slot.

            // Calculate the world position of the target slot.
            Vector3 targetPos = SlotManager.Instance.GetSlotWorldPosition(currentTargetSlot, mainCanvas);
            // Calculate the direction vector from current position to target.
            Vector3 dir = (targetPos - transform.position).normalized;
            // Calculate the remaining distance to the target.
            float distance = Vector3.Distance(transform.position, targetPos);

            // Move the object towards the target position.
            transform.position += dir * moveSpeed * Time.deltaTime;

            // Check if the object is close enough to be considered "placed".
            if (distance < realStopDistance)
            {
                // Attempt to place the object into the slot system.
                // mainCanvas'ı TryPlaceObject metoduna parametre olarak geçiyoruz.
                SlotManager.Instance.TryPlaceObject(uniqueId, objectSprite, mainCanvas);
                // Destroy this flying object as it has been placed.
                Destroy(gameObject);
            }
        }
        else
        {
            // If there's no valid target slot (e.g., all slots full and no place for an item),
            // the object might despawn or return to a pool. For now, it just destroys itself.
            Debug.LogWarning($"ClickableObject '{uniqueId}' could not find a target slot and will be destroyed.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Called when the mouse button is pressed over this collider/object.
    /// Initiates the object's movement towards a slot.
    /// </summary>
    private void OnMouseDown()
    {
        if (isFlying) return; // Prevent multiple clicks from affecting an already flying object.

        // Calculate time since last click for dynamic stop distance.
        float timeSinceLastClick = Time.time - lastClickTime;
        lastClickTime = Time.time; // Update last click time.

        // If clicks are rapid, add to the stop distance. This gives a feeling of "snappiness"
        // or a slight delay for quick successive placements.
        float addedStop = (timeSinceLastClick < clickDelayThreshold) ? addedStopPerClick : 0f;

        // Clamp the 'realStopDistance' to keep it within sensible bounds.
        realStopDistance = Mathf.Clamp(baseStopDistance + addedStop, baseStopDistance, maxStopDistance);

        isFlying = true; // Start the object's movement.

        // You could add a small visual animation here (e.g., a slight scale change)
        // before the object starts flying, similar to what Slot.cs does.
    }
}
