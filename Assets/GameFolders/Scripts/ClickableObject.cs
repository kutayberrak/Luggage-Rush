using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening; // DOTween kütüphanesini dahil et!

public class ClickableObject : MonoBehaviour
{
    public string uniqueId; // The unique ID of this clickable object.
    public Sprite objectSprite; // The sprite representation of this object.
    public Canvas mainCanvas; // Reference to the main UI Canvas.

    private bool isFlying = false; // True if the object is currently moving towards a slot.
    private Slot currentTargetSlot; // The slot this object is currently flying towards.

    public float moveSpeed = 5f; // Speed at which the object moves towards a slot (now more like duration multiplier).
    public float stopDistance = 0.05f; // Initial distance to target before it's considered "placed".

    private float realStopDistance; // The calculated stop distance, potentially adjusted by rapid clicks.

    // Static variables to manage click timing for dynamic stop distance.
    public static float lastClickTime = -100f; // Stores the Time.time of the last click.
    public static float clickDelayThreshold = 0.3f; // If clicks are faster than this, add to stop distance.
    public static float addedStopPerClick = 0.2f; // Amount added to stop distance for rapid clicks.
    public static float baseStopDistance = 0.05f; // Minimum stop distance.
    public static float maxStopDistance = 0.6f; // Maximum stop distance to prevent excessive "hang time".

    private Tween currentMoveTween; // DOTween hareket tween'ini tutmak için

    void Update()
    {
        // Sadece uçuş modundayken güncellemeleri yap.
        if (!isFlying) return;

        // Nesnenin hareket etmesini beklediğimiz durumdayken hedefi sürekli kontrol et.
        // Bu, SlotManager'daki diğer işlemler nedeniyle hedefin değişebilmesi için önemli.
        Slot newTarget = SlotManager.Instance.GetCurrentValidInsertSlot(uniqueId);

        if (newTarget != null)
        {
            // Eğer hedef değiştiyse veya yeni bir hedef belirlendiyse
            if (newTarget != currentTargetSlot || currentMoveTween == null || !currentMoveTween.IsPlaying())
            {
                currentTargetSlot = newTarget; // Hedefi güncelle

                Vector3 targetPos = SlotManager.Instance.GetSlotWorldPosition(currentTargetSlot, mainCanvas);
                float distance = Vector3.Distance(transform.position, targetPos);

                // Eğer zaten bir tween oynuyorsa, onu durdur ve yenisini başlat
                if (currentMoveTween != null && currentMoveTween.IsPlaying())
                {
                    currentMoveTween.Kill(); // Mevcut tween'i öldür
                }

                // Hareket süresini mesafeye ve hıza göre ayarla
                // moveSpeed, artık saniyede katedilecek mesafe değil, toplam süreyi etkileyen bir çarpan gibi davranacak
                float duration = distance / (moveSpeed * 5f); // 5f çarpanı deneysel, daha yumuşak bir süre için ayarlanabilir

                // DOTween ile hareket ettir
                // UI elemanları için genellikle RectTransform.DOAnchorPos kullanılır, ancak objeniz bir dünya objesiyse DOMove daha uygun olabilir.
                // Eğer bu ClickableObject bir UI elemanı ise (RectTransform'a sahipse), DOAnchorPos kullanmak daha doğrudur.
                // Eğer bir 3D oyun objesiyse, DOMove kullanmak daha uygundur.
                // Varsayılan olarak DOMove kullanıyorum, ihtiyaca göre DOAnchorPos olarak değiştirebilirsin.
                currentMoveTween = transform.DOMove(targetPos, duration)
                    .SetEase(Ease.OutQuad) // Hareketin sonuna doğru yavaşlama efekti
                    .OnComplete(() => OnMoveComplete()); // Hareket tamamlandığında çağrılacak fonksiyon

                // Ekstra: Eğer bu bir UI elementi ise, DOAnchorPos kullanın:
                // if (GetComponent<RectTransform>() != null) {
                //    currentMoveTween = GetComponent<RectTransform>().DOAnchorPos(targetPos, duration, false)
                //        .SetEase(Ease.OutQuad)
                //        .OnComplete(() => OnMoveComplete());
                // } else {
                //    currentMoveTween = transform.DOMove(targetPos, duration)
                //        .SetEase(Ease.OutQuad)
                //        .OnComplete(() => OnMoveComplete());
                // }
            }
        }
        else
        {
            // Eğer geçerli bir hedef slot bulunamazsa, objeyi yok et.
            Debug.LogWarning($"ClickableObject '{uniqueId}' could not find a target slot and will be destroyed.");
            if (currentMoveTween != null && currentMoveTween.IsPlaying())
            {
                currentMoveTween.Kill(); // Tween'i öldür
            }
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Hareket tamamlandığında çağrılan metod.
    /// </summary>
    private void OnMoveComplete()
    {
        // Hedefe ulaşıldığında nesneyi yerleştir ve yok et.
        SlotManager.Instance.TryPlaceObject(uniqueId, objectSprite, mainCanvas);
        Destroy(gameObject);
    }

    /// <summary>
    /// Called when the mouse button is pressed over this collider/object.
    /// Initiates the object's movement towards a slot.
    /// </summary>
    private void OnMouseDown()
    {
        if (isFlying) return; // Zaten uçuyorsa tekrar tıklamayı engelle.

        float timeSinceLastClick = Time.time - lastClickTime;
        lastClickTime = Time.time;

        float addedStop = (timeSinceLastClick < clickDelayThreshold) ? addedStopPerClick : 0f;
        realStopDistance = Mathf.Clamp(baseStopDistance + addedStop, baseStopDistance, maxStopDistance);

        isFlying = true; // Objenin hareketini başlat.

        // Eğer manuel olarak hareket başlatılmazsa, Update döngüsü otomatik olarak DOTween'i tetikleyecektir.
        // İstersen burada küçük bir "punch" animasyonu veya başlangıç efekti ekleyebilirsin:
        // transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 10, 1f);
    }

    /// <summary>
    /// Nesne yok edilirken çalışan metod. Tween'i temizler.
    /// </summary>
    private void OnDestroy()
    {
        if (currentMoveTween != null && currentMoveTween.IsActive())
        {
            currentMoveTween.Kill(); // Objeler yok edilirken aktif tween'leri temizle
        }
    }
}
