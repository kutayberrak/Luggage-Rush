using UnityEngine;
using DG.Tweening;
using GameFolders.Scripts.Enums;

public interface ISlottable
{
    void NotifySlotted();
    void NotifyUnslotted();
}


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

    [Header("Curve Movement Settings")]
    [Tooltip("Curve hareketi kullanılsın mı?")]
    public bool useCurveMovement = true;
    [Tooltip("Curve hareket süresi")]
    public float curveMoveDuration = 1.2f;
    [Tooltip("Curve hareket easing tipi")]
    public Ease curveEase = Ease.InOutSine;
    [Tooltip("Curve yüksekliği (0 = düz çizgi)")]
    [Range(0f, 5f)]
    public float curveHeight = 2f;
    [Tooltip("Curve hareket sırasında rotasyon animasyonu")]
    public bool useRotationAnimation = true;
    [Tooltip("Rotasyon animasyon süresi")]
    public float rotationDuration = 0.8f;

    [HideInInspector] public int reservedSlotIndex = -1;
    protected bool isMoving = false;
    private bool isInClickAnimation = false;
    protected bool isInCurveMovement = false;

    // **Yeni** alanlar:
    private float moveDelay = 0f;
    private float moveStartTime = 0f;

    // **YENİ**: Tıklama kilidi
    private bool isClickProcessed = false;
    private float lastClickTime = 0f;
    private const float CLICK_COOLDOWN = 0.05f; // 50ms tıklama bekleme süresi (daha hızlı)

    // **YENİ**: Animasyon için orijinal pozisyon ve rotasyon
    protected Vector3 originalPosition;
    protected Vector3 originalRotation;

    private Rigidbody rigidBody;
    private Vector3 moveTargetPos;
    private float startDistance;
    private Vector3 originalScale;
    public string UniqueID => GetID();

    // **YENİ**: Curve hareket için
    private Sequence curveMovementSequence;

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
        isClickProcessed = false;

        Slot slot = SlotManager.Instance.slots[reservedSlotIndex];

        // — Taşıma bilgilerini kaydet —
        moveTargetPos = slot.transform.position + slot.positionOfset;
        startDistance = Vector3.Distance(transform.position, moveTargetPos);
        originalScale = transform.localScale;
    }

    // **YENİ**: Curve hareketi başlat
    protected virtual void StartCurveMovement()
    {
        if (isInCurveMovement) return;

        isInCurveMovement = true;
        
        // Orijinal pozisyonu kaydet
        Vector3 startPos = transform.position;
        Vector3 endPos = moveTargetPos;

        // Mesafeye göre curve yüksekliğini ayarla
        float distance = Vector3.Distance(startPos, endPos);
        float adjustedCurveHeight = Mathf.Clamp(curveHeight * (distance / 5f), 0.5f, curveHeight);
        
        // Curve için orta nokta hesapla
        Vector3 midPoint = Vector3.Lerp(startPos, endPos, 0.5f);
        midPoint.y += adjustedCurveHeight; // Ayarlanmış curve yüksekliği
        
        // Daha yumuşak curve için 5 noktalı path oluştur
        Vector3[] path = new Vector3[] { 
            startPos, 
            Vector3.Lerp(startPos, midPoint, 0.4f), // Başlangıç eğrisi
            midPoint, 
            Vector3.Lerp(midPoint, endPos, 0.6f),   // Bitiş eğrisi
            endPos 
        };
        
        // DOTween sequence oluştur
        curveMovementSequence = DOTween.Sequence();
        
        // Pozisyon animasyonu - daha yumuşak path
        curveMovementSequence.Append(transform.DOPath(path, curveMoveDuration, PathType.CatmullRom)
            .SetEase(curveEase));
        
        // Rotasyon animasyonu (opsiyonel)
        if (useRotationAnimation)
        {
            Vector3 startRotation = transform.eulerAngles;
            Vector3 endRotation = Vector3.zero;
            
            curveMovementSequence.Join(transform.DORotate(endRotation, rotationDuration)
                .SetEase(curveEase));
        }

        // Mevcut sistemle aynı ölçek animasyonu - 1x'den 5x'e kadar büyü
        curveMovementSequence.Join(transform.DOScale(originalScale * 0.4f, curveMoveDuration)
            .SetEase(Ease.OutQuad));

        // Animasyon tamamlandığında
        curveMovementSequence.OnComplete(() => {
            isInCurveMovement = false;
            isMoving = false;
            
            // Orijinal boyuta küçültme - mevcut sistemde yok, kaldırıyoruz
            // transform.localScale = originalScale;
            SlotManager.Instance.OnMovableArrived(this);
        });
        
        Debug.Log($"[ClickableObject] Started curve movement for {UniqueID} to slot {reservedSlotIndex} with height {adjustedCurveHeight:F2}");
    }

    // **YENİ**: Tıklama animasyonunu başlat
    protected virtual void StartClickAnimation()
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
        SlotManager.Instance.TryPlaceObject3D(gameObject, UniqueID);

        clickSequence.Append(transform.DOMove(risePosition, riseDuration).SetEase(riseEase));
        clickSequence.Join(transform.DORotate(Vector3.zero, riseDuration).SetEase(riseEase));
        
        // Animasyon tamamlandığında slot hareketini başlat
        clickSequence.OnComplete(() => {
            isInClickAnimation = false;
            // SlotManager'dan hareketi başlat
            
        });

        Debug.Log($"[ClickableObject] Started click animation for {UniqueID}");
    }

    private void Update()
    {
        if (isInClickAnimation || isInCurveMovement)
            return;
        if (!isMoving || reservedSlotIndex < 0)
            return;
        if (Time.time - moveStartTime < moveDelay)
            return;
        if (reservedSlotIndex >= SlotManager.Instance.slots.Count)
        {
            isMoving = false;
            return;
        }

        // **YENİ**: Curve hareketi kullanılıyorsa başlat
        if (useCurveMovement)
        {
            StartCurveMovement();
            return;
        }

        // —— Asıl hareket (mevcut sistem) ——
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, moveTargetPos, step);

        // —— Ölçek animasyonu —— 
        float remaining = Vector3.Distance(transform.position, moveTargetPos);
        float progress = startDistance > 0f
            ? 1f - (remaining / startDistance)
            : 1f;

        // —— Varış kontrolü ——
        if (remaining <= arrivalThreshold)
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

        // **YENİ**: Tıklama işlemini başlat
        isClickProcessed = true;
        Debug.Log($"[ClickableObject] Processing click for {UniqueID}");

        // **YENİ**: Tıklama animasyonunu başlat
        StartClickAnimation();
    }

    public void HandleRigidBody()
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
        
        // **YENİ**: Curve hareket sequence'ini temizle
        if (curveMovementSequence != null)
        {
            curveMovementSequence.Kill();
        }
        
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
