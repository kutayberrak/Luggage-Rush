using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class ClickableObject : MonoBehaviour
{
    public Sprite objectSprite;
    public Canvas mainCanvas;
    public float moveDuration = 0.5f;
    public float curveHeight = 1.5f; // Yükselme miktarı
    public float clickScale = 1.2f; // Tıklanınca büyüme oranı
    public float slotScale = 0.2f; // Slota girerken küçülme oranı
    public float scaleDuration = 0.15f; // Büyüme animasyon süresi
    public float rotateDuration = 0.25f; // Dönme animasyon süresi
    public float waitAfterScale = 0.15f; // Büyüme sonrası bekleme süresi

    private bool isMoving = false;
    private Slot reservedSlot = null;

    public string uniqueId; // Objenin benzersiz kimliği

    private void OnMouseDown()
    {
        if (isMoving) return;

        string currentObjectId = this.uniqueId; // Tıklanan objenin uniqueID'si

        // Aynı ID'ye sahip dolu slotları bul
        List<Slot> matchingSlots = new List<Slot>();
        foreach (var slot in SlotManager.Instance.slots)
        {
            if (slot.IsOccupied && slot.StoredUniqueID == currentObjectId)
            {
                matchingSlots.Add(slot);
            }
        }

        // Eğer 2 veya daha fazla eşleşen slot varsa (3. öğeyi eklemek için)
        if (matchingSlots.Count >= 1) // Eğer 2 tane varsa, bu 3. eşleşen olacak
        {
            // Bu durumda, yeni objeyi ilk eşleşen slotun yerine yerleştirmeli
            // ve oradan itibaren diğer slotları sağa kaydırmalıyız.
            int insertIndex = SlotManager.Instance.slots.IndexOf(matchingSlots[0]);

            // Slotları sağa kaydır. Bu, yeni objeye yer açacak.
            SlotManager.Instance.ShiftRightFrom(SlotManager.Instance.slots[insertIndex]);

            // Yeni objeyi belirlenen yere yerleştir
            reservedSlot = SlotManager.Instance.slots[insertIndex];
        }
        else // Eşleşen slot sayısı 0 veya 1 ise (yani yeni bir eşleşme başlatılıyor veya 2. eşleşen bulunuyor)
        {
            // Normal boş slot bul ve oraya yerleştir.
            reservedSlot = SlotManager.Instance.GetFirstEmptySlot();
        }

        // Eğer boş slot bulunamazsa (tüm slotlar doluysa) objeyi hareket ettirme
        if (reservedSlot != null)
        {
            isMoving = true;
            // Slotun içeriğini ve durumunu geçici olarak ayarla
            // Animasyon bittiğinde kesin dolumu yapacağız.
            reservedSlot.SetOccupied(true);
            reservedSlot.StoredUniqueID = this.uniqueId; // Henüz sprite atanmadı ama unique ID tutulur
            reservedSlot.iconImage.sprite = this.objectSprite;
            // Animasyon dizisi
            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOScale(clickScale, scaleDuration).SetEase(Ease.InCubic));
            seq.Join(transform.DORotate(Vector3.zero, rotateDuration).SetEase(Ease.InCubic));
            seq.AppendInterval(waitAfterScale);
            seq.OnComplete(() =>
            {
                MoveToSlotWithCurve(reservedSlot);
            });
        }
        else
        {
            Debug.LogWarning("Tüm slotlar dolu! Obje yerleştirilemedi.");
            // Burada kullanıcıya geri bildirim verebilirsiniz (örn. ses, hata mesajı)
        }
    }

    private void MoveToSlotWithCurve(Slot targetSlot)
    {
        RectTransform slotRect = targetSlot.GetComponent<RectTransform>();
        Vector3 worldTargetPos = Vector3.zero;

        if (slotRect != null && mainCanvas != null)
        {
            Camera cam = mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCanvas.worldCamera;
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, slotRect.position);
            // Z mesafesini koruyarak dünya pozisyonuna dönüştür
            float zDistance = Vector3.Distance(Camera.main.transform.position, transform.position);
            worldTargetPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDistance));
        }
        else
        {
            // Eğer RectTransform veya Canvas yoksa, doğrudan slotun dünya pozisyonunu kullan
            worldTargetPos = targetSlot.transform.position;
        }

        // Objelerin aynı Z düzleminde kalmasını sağla
        worldTargetPos.z = transform.position.z;

        // Kavisli yol için ara nokta (yukarıya doğru)
        Vector3 midPoint = (transform.position + worldTargetPos) / 2f;
        midPoint += Vector3.up * curveHeight;

        // Path noktalarını oluştur
        Vector3[] path = new Vector3[] { transform.position, midPoint, worldTargetPos };

        // Hareketle birlikte scale animasyonu başlat
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOPath(path, moveDuration, PathType.CatmullRom).SetEase(Ease.Linear));
        seq.Join(transform.DOScale(slotScale, moveDuration).SetEase(Ease.InCubic));
        seq.OnComplete(() =>
        {
            // Animasyon tamamlandığında slotu sprite ile doldur ve objeyi yok et
            targetSlot.FillSlotWithSprite(objectSprite); // Sprite'ı buraya ata
            Destroy(gameObject);
            isMoving = false; // Hareket bitti

            // YENİ ÇAĞRI: Eşleşmeleri kontrol et ve temizle
            SlotManager.Instance.CheckForMatches(targetSlot);
        });
    }
}