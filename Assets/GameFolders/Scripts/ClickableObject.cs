using GameFolders.Scripts.Enums;
using UnityEngine;

[System.Serializable]
public class ClickableID
{
    public ObjectType category;

    public LuggageType luggageType;
    public JunkPieceType garbageType;
    public CollectiblePieceType collectionType;

    public string GetID()
    {
        return category switch
        {
            ObjectType.Luggage => luggageType.ToString(),
            ObjectType.Garbage => garbageType.ToString(),
            ObjectType.Collection => collectionType.ToString(),
            _ => "Unknown"
        };
    }
}
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

    // **YENİ**: Tıklama kilidi
    private bool isClickProcessed = false;
    private float lastClickTime = 0f;
    private const float CLICK_COOLDOWN = 0.05f; // 50ms tıklama bekleme süresi (daha hızlı)

    public ClickableID id;
    public string UniqueID => id.GetID();

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
        isClickProcessed = false; // Hareket başladığında tıklama kilidini kaldır
    }

    private void Update()
    {
        if (!isMoving || reservedSlotIndex < 0)
            return;

        // ————— havada bekle —————
        if (Time.time - moveStartTime < moveDelay)
            return;

        // **YENİ**: Slot'un hala geçerli olduğunu kontrol et
        if (reservedSlotIndex >= SlotManager.Instance.slots.Count)
        {
            isMoving = false;
            return;
        }

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
        Debug.Log("Tıklandı");
        // **YENİ**: Tıklama cooldown kontrolü
        if (Time.time - lastClickTime < CLICK_COOLDOWN)
        {
            return;
        }
        lastClickTime = Time.time;

        // Eğer zaten bir slot'a rezerveliysen veya hâlihazırda hareket ediyorsa
        if (reservedSlotIndex >= 0 || isMoving)
        {
            Debug.Log($"[ClickableObject] Object already reserved or moving. Slot: {reservedSlotIndex}, Moving: {isMoving}");
            return;
        }

        // **YENİ**: Tıklama işlemi zaten başlatıldıysa tekrar başlatma
        if (isClickProcessed)
        {
            Debug.Log($"[ClickableObject] Click already being processed for {UniqueID}");
            return;
        }

        // **YENİ**: eğer o anda hiç boş slot kalmadıysa, tıklamayı yoksay
        if (!SlotManager.Instance.HasFreeSlot())
        {
            Debug.Log("No free slots right now — click ignored.");
            return;
        }

        // **YENİ**: Tıklama işlemini başlat
        isClickProcessed = true;
        Debug.Log($"[ClickableObject] Processing click for {UniqueID}");

        // Aksi halde yerleştirme akışını başlat
        SlotManager.Instance.TryPlaceObject3D(gameObject, UniqueID);
    }

    // **YENİ**: Obje yok edildiğinde temizlik
    private void OnDestroy()
    {
        if (SlotManager.Instance != null)
        {
            // Eğer bu obje hala hareket ediyorsa, SlotManager'dan kaldır
            if (isMoving && reservedSlotIndex >= 0)
            {
                // SlotManager'da bu objeyi temizle
                // Bu işlem SlotManager'da yapılacak
            }
        }
    }
}
