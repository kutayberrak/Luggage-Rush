using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ClickableObject : MonoBehaviour
{
    public float speed = 5f;
    public float arrivalThreshold = 0.05f;

    [HideInInspector] public int reservedSlotIndex = -1;
    private bool isMoving = false;

    // **Yeni** alanlar:
    private float moveDelay = 0f;
    private float moveStartTime = 0f;

    public string UniqueID;

    /// <summary>
    /// SlotManager tarafından atanacak gecikme süresi.
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
        if (!isMoving || reservedSlotIndex < 0)
            return;

        // ————— havada bekle —————
        if (Time.time - moveStartTime < moveDelay)
            return;

        // (eğer retargeting kodu varsa buraya gelmeden önce çalışır)

        // ————— asıl hareket —————
        Vector3 targetPos = SlotManager.Instance.slots[reservedSlotIndex].transform.position;
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            speed * Time.deltaTime
        );

        // varış kontrolü
        if (Vector3.Distance(transform.position, targetPos) <= arrivalThreshold)
        {
            isMoving = false;
            SlotManager.Instance.OnMovableArrived(this);
        }
    }

    private void OnMouseDown()
    {
        // Eğer zaten bir slot’a rezerveliysen veya hâlihazırda hareket ediyorsa
        if (reservedSlotIndex >= 0)
            return;

        // **YENİ**: eğer o anda hiç boş slot kalmadıysa, tıklamayı yoksay
        if (!SlotManager.Instance.HasFreeSlot())
        {
            Debug.Log("No free slots right now — click ignored.");
            return;
        }

        // Aksi halde yerleştirme akışını başlat
        SlotManager.Instance.TryPlaceObject3D(gameObject, UniqueID);
    }
}
