using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ClickableObject : MonoBehaviour
{
    public float speed = 5f;
    public float arrivalThreshold = 0.05f;

    [HideInInspector] public int reservedSlotIndex = -1;
    private bool isMoving = false;

    // **Yeni alanlar**:
    private float moveDelay = 0f;
    private float moveStartTime = 0f;

    public string UniqueID;

    /// <summary>
    /// SlotManager tarafından set edilecek.
    /// </summary>
    public void SetMoveDelay(float d)
    {
        moveDelay = d;
    }

    public void BeginMove(int slotIndex)
    {
        reservedSlotIndex = slotIndex;
        isMoving = true;
        moveStartTime = Time.time;
    }

    private void Update()
    {
        if (!isMoving || reservedSlotIndex < 0) return;

        // 1) Hava gecikmesi: delay süresi dolana kadar bekle
        if (Time.time - moveStartTime < moveDelay)
            return;

        // 2) Dönüşümlü retargeting vs. varsa burada kalmalı (önceki kodunuzu koruyun)
        //    …

        // 3) Asıl hareket
        Vector3 targetPos = SlotManager.Instance.slots[reservedSlotIndex].transform.position;
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            speed * Time.deltaTime
        );

        // 4) Varış kontrolü
        if (Vector3.Distance(transform.position, targetPos) <= arrivalThreshold)
        {
            isMoving = false;
            SlotManager.Instance.OnMovableArrived(this);
        }
    }

    private void OnMouseDown()
    {
        if (reservedSlotIndex >= 0) return;
        SlotManager.Instance.TryPlaceObject3D(gameObject, UniqueID);
    }
}
