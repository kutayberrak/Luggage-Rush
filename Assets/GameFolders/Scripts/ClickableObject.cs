// ClickableObject.cs
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class ClickableObject : MonoBehaviour
{
    public float speed = 5f;
    public float arrivalThreshold = 0.05f;

    // Atanacak slot indeksi, SlotManager tarafından güncellenecek
    [HideInInspector] public int reservedSlotIndex = -1;
    private bool isMoving = false;

    // Benzersiz ID’n varsa ekle
    public string UniqueID;

    private void OnMouseDown()
    {
        // Eğer zaten hareket ediyorsa veya bir slota rezerveli ise tıklamayı yoksay
        if (reservedSlotIndex >= 0) return;

        SlotManager.Instance.TryPlaceObject3D(gameObject, UniqueID);
    }

    private void Update()
    {
        if (!isMoving || reservedSlotIndex < 0) return;

        Vector3 targetPos = SlotManager.Instance.slots[reservedSlotIndex].transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) <= arrivalThreshold)
        {
            isMoving = false;
            SlotManager.Instance.OnMovableArrived(this);
        }
    }

    public void BeginMove(int slotIndex)
    {
        reservedSlotIndex = slotIndex;
        isMoving = true;
    }
}
