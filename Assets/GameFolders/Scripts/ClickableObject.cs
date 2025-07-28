using UnityEngine;
using DG.Tweening;
using GameFolders.Scripts.Enums;

[RequireComponent(typeof(Collider))]
public class ClickableObject : MonoBehaviour
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

    public float speed = 5f;
    public float arrivalThreshold = 0.05f;

    [Header("Click Animation Settings")]
    [Tooltip("Tıklama sonrası yükselme süresi")]
    public float riseDuration = 0.3f;
    [Tooltip("Tıklama sonrası yükselme yüksekliği")]
    public float riseHeight = 1f;
    [Tooltip("Tıklama animasyonu easing tipi")]
    public Ease riseEase = Ease.OutQuad;

    [HideInInspector] public int reservedSlotIndex = -1;
    private bool isMoving = false;
    private bool isInClickAnimation = false;

    // **Yeni** alanlar:
    private float moveDelay = 0f;
    private float moveStartTime = 0f;

    // **YENİ**: Tıklama kilidi
    private bool isClickProcessed = false;
    private float lastClickTime = 0f;
    private const float CLICK_COOLDOWN = 0.05f; // 50ms tıklama bekleme süresi (daha hızlı)

    // **YENİ**: Animasyon için orijinal pozisyon ve rotasyon
    private Vector3 originalPosition;
    private Vector3 originalRotation;

    private Rigidbody rigidBody;
    public string UniqueID => GetID();

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }
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

    // **YENİ**: Tıklama animasyonunu başlat
    private void StartClickAnimation()
    {
        if (isInClickAnimation) return;

        isInClickAnimation = true;
        
        // Orijinal pozisyon ve rotasyonu kaydet
        originalPosition = transform.position;
        originalRotation = transform.eulerAngles;

        // Yükselme animasyonu
        Vector3 risePosition = originalPosition + Vector3.up * riseHeight;
        
        // Pozisyon ve rotasyon animasyonunu aynı anda başlat
        Sequence clickSequence = DOTween.Sequence();
        
        clickSequence.Append(transform.DOMove(risePosition, riseDuration).SetEase(riseEase));
        clickSequence.Join(transform.DORotate(Vector3.zero, riseDuration).SetEase(riseEase));
        
        // Animasyon tamamlandığında slot hareketini başlat
        clickSequence.OnComplete(() => {
            isInClickAnimation = false;
            // SlotManager'dan hareketi başlat
            SlotManager.Instance.TryPlaceObject3D(gameObject, UniqueID);
        });

        Debug.Log($"[ClickableObject] Started click animation for {UniqueID}");
    }

    private void Update()
    {
        // **YENİ**: Animasyon sırasında normal hareket çalışmasın
        if (isInClickAnimation)
            return;

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

    public void OnClickedByPlayer()
    {
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

        // **YENİ**: Tıklama animasyonu sırasında tekrar tıklamayı engelle
        if (isInClickAnimation)
        {
            Debug.Log($"[ClickableObject] Click animation in progress for {UniqueID}");
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

        HandleRigidBody();

        // **YENİ**: Tıklama işlemini başlat
        isClickProcessed = true;
        Debug.Log($"[ClickableObject] Processing click for {UniqueID}");

        // **YENİ**: Tıklama animasyonunu başlat
        StartClickAnimation();
    }

    private void HandleRigidBody()
    {
        rigidBody.isKinematic = true;
        rigidBody.detectCollisions = false;
        rigidBody.useGravity = false;
        rigidBody.linearVelocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
    }

    // **YENİ**: Obje yok edildiğinde temizlik
    private void OnDestroy()
    {
        // **YENİ**: DOTween animasyonlarını temizle
        DOTween.Kill(transform);
        
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
