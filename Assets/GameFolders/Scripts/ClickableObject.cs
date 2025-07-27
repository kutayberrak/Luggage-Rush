using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening; // DOTween kütüphanesini dahil et!

public class ClickableObject : MonoBehaviour
{
    public string uniqueId; // Tıklanabilir objenin benzersiz kimliği.
    public Sprite objectSprite; // Objeyi temsil eden sprite.
    public Canvas mainCanvas; // Ana UI Canvas referansı.

    private bool isFlying = false; // Obje şu anda hareket ediyorsa (başlangıç veya slota doğru) true.
    private Slot currentTargetSlot; // Objeyi şu anda hedefleyen slot.

    public float moveSpeed = 5f; // Objelerin slota hareket hızı (artık süre çarpanı gibi).
    public float stopDistance = 0.05f; // Objeyi "yerleştirilmiş" kabul etmek için hedef mesafesi.

    private float realStopDistance; // Hesaplanmış durma mesafesi, hızlı tıklamalarla değişebilir.

    // Dinamik durma mesafesi için tıklama zamanlamasını yöneten statik değişkenler.
    public static float lastClickTime = -100f; // Son tıklamanın Time.time değeri.
    public static float clickDelayThreshold = 0.3f; // Tıklamalar bu süreden hızlıysa durma mesafesine ekle.
    public static float addedStopPerClick = 0.2f; // Hızlı tıklamalar için durma mesafesine eklenecek miktar.
    public static float baseStopDistance = 0.05f; // Minimum durma mesafesi.
    public static float maxStopDistance = 0.6f; // Aşırı "takılmayı" önlemek için maksimum durma mesafesi.

    private Tween currentMoveTween; // DOTween hareket tween'ini tutmak için
    private Sequence initialAnimationSequence; // Başlangıç animasyon sekansını tutmak için

    // Yeni: Başlangıç animasyonu ayarları
    public float initialLiftHeight = 0.5f; // Obje tıklanınca ne kadar yükselecek (Unity birimi).
    public float initialAnimationDuration = 0.2f; // Yükselme ve rotasyon animasyonunun süresi (saniye).

    // Yeni: Slota girerken küçülme ayarları
    public float shrinkScaleFactor = 0.8f; // Slota girerken objenin küçüleceği oran (örn: 0.8f = %80'ine küçül).
    public Ease shrinkEaseType = Ease.OutQuad; // Küçülme animasyonunun yumuşama tipi.

    /// <summary>
    /// Her karede çağrılır. Objelerin hedef slota hareketini yönetir.
    /// </summary>
    void Update()
    {
        // Yalnızca "uçuş" durumundaysa ve ana hareket tween'i aktifse hedefi kontrol et.
        // Başlangıç animasyonu sırasında bu blok çalışmayacak.
        if (!isFlying || currentMoveTween == null || !currentMoveTween.IsPlaying()) return;

        Slot newTarget = SlotManager.Instance.GetCurrentValidInsertSlot(uniqueId);

        // Eğer hedef slot değiştiyse, mevcut hareketi durdur ve yeni hedefe doğru yeniden başlat.
        // Bu, FindInsertIndex'in dinamik olarak farklı bir yere yönlendirmesi durumunda önemlidir.
        if (newTarget != null && newTarget != currentTargetSlot)
        {
            currentTargetSlot = newTarget; // Hedefi güncelle
            if (currentMoveTween != null && currentMoveTween.IsPlaying())
            {
                currentMoveTween.Kill(); // Mevcut tween'i öldür
            }
            InitiateSlotMovement(); // Yeni hedefe doğru hareketi başlat
        }
        else if (newTarget == null && currentTargetSlot != null) // Hedef null olduysa ama daha önce bir hedef vardıysa
        {
            Debug.LogWarning($"ClickableObject '{uniqueId}' could not find a target slot and will be destroyed.");
            if (currentMoveTween != null && currentMoveTween.IsPlaying())
            {
                currentMoveTween.Kill();
            }
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Obje hedef slota ulaştığında ve hareket animasyonu tamamlandığında çağrılır.
    /// </summary>
    private void OnMoveComplete()
    {
        // Hedefe ulaşıldığında nesneyi yerleştirme mantığını SlotManager'a devret.
        // SlotManager, tüm işlemler bittikten sonra bu ClickableObject'i yok edecek.
        
        SlotManager.Instance.TryPlaceObject(uniqueId, objectSprite, mainCanvas, this); // 'this' keyword'ü ile ClickableObject referansını gönderdik.
        Destroy(gameObject); // ARTIK ClickableObject kendi kendini burada yok etmeyecek.
    }

    /// <summary>
    /// Fare düğmesi bu objenin üzerinde basıldığında çağrılır.
    /// Objeye başlangıç animasyonunu ve ardından slota doğru hareketi başlatır.
    /// </summary>
    private void OnMouseDown()
    {
        if (isFlying) return;
        isFlying = true;

        float timeSinceLastClick = Time.time - lastClickTime;
        lastClickTime = Time.time;

        float delay = Mathf.Clamp(0.15f - timeSinceLastClick, 0f, 0.15f); // 💡 hızlı tıklama varsa beklet
        float addedStop = (timeSinceLastClick < clickDelayThreshold) ? addedStopPerClick : 0f;
        realStopDistance = Mathf.Clamp(baseStopDistance + addedStop, baseStopDistance, maxStopDistance);

        initialAnimationSequence = DOTween.Sequence();

        // ⏳ Gecikme varsa en başa bekleme ekle
        if (delay > 0)
            initialAnimationSequence.AppendInterval(delay);

        Debug.Log("delay: " + delay);

        // Yükselme animasyonu
        if (GetComponent<RectTransform>() != null)
        {
            initialAnimationSequence.Append(
                GetComponent<RectTransform>().DOAnchorPos3DY(GetComponent<RectTransform>().anchoredPosition3D.y + initialLiftHeight, initialAnimationDuration)
                .SetEase(Ease.OutQuad)
            );
        }
        else
        {
            initialAnimationSequence.Append(
               transform.DOMoveY(transform.position.y + initialLiftHeight, initialAnimationDuration)
               .SetEase(Ease.OutQuad)
           );
        }

        initialAnimationSequence.Append(
            transform.DORotate(Vector3.zero, initialAnimationDuration).SetEase(Ease.OutQuad)
        );

        initialAnimationSequence.OnComplete(() => InitiateSlotMovement());
    }

    /// <summary>
    /// Başlangıç animasyonları bittikten sonra objenin slota doğru hareketini başlatan metod.
    /// </summary>
    private void InitiateSlotMovement()
    {
        // Hedef slotu bul
        currentTargetSlot = SlotManager.Instance.GetCurrentValidInsertSlot(uniqueId);

        if (currentTargetSlot != null)
        {
            Vector3 targetPos = SlotManager.Instance.GetSlotWorldPosition(currentTargetSlot, mainCanvas);
            float distance = Vector3.Distance(transform.position, targetPos);
            float duration = distance / (moveSpeed * 5f); // moveSpeed ayarlanabilir

            // Hareket ve küçülme animasyonlarını birleştirmek için bir Sequence oluştur.
            Sequence moveAndShrinkSequence = DOTween.Sequence();

            // UI elementi mi yoksa 3D obje mi kontrol et ve ona göre tween başlat
            if (GetComponent<RectTransform>() != null)
            {
                moveAndShrinkSequence.Append(
                    GetComponent<RectTransform>().DOAnchorPos(targetPos, duration, false)
                        .SetEase(Ease.OutQuad)
                );
            }
            else
            {
                moveAndShrinkSequence.Append(
                    transform.DOMove(targetPos, duration)
                        .SetEase(Ease.OutQuad)
                );
            }

            // Objeyi hedef slota doğru hareket ederken küçült.
            // Bu animasyonu, hareket animasyonu ile eş zamanlı olarak '.Join()' ile ekliyoruz.
            moveAndShrinkSequence.Join(
                transform.DOScale(Vector3.one * shrinkScaleFactor, duration)
                    .SetEase(shrinkEaseType) // Küçülme animasyonu için seçilen yumuşama tipi
            );

            currentMoveTween = moveAndShrinkSequence.OnComplete(() => OnMoveComplete());
        }
        else
        {
            // Eğer başlangıç animasyonlarından sonra hedef slot bulunamazsa objeyi yok et.
            Debug.LogWarning($"ClickableObject '{uniqueId}' could not find a target slot after initial animation and will be destroyed.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Nesne yok edilirken çağrılır. Aktif DOTween tween'lerini temizler.
    /// </summary>
    private void OnDestroy()
    {
        // Nesne yok edildiğinde çalışan herhangi bir DOTween tween'ini durdur ve temizle.
        if (currentMoveTween != null && currentMoveTween.IsActive())
        {
            currentMoveTween.Kill();
        }
        if (initialAnimationSequence != null && initialAnimationSequence.IsActive())
        {
            initialAnimationSequence.Kill();
        }
    }
}
