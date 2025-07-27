using UnityEngine;
using System.Collections.Generic;
using System.Collections; // Coroutine'ler için gerekli
using UnityEngine.UI;
using DG.Tweening; // DOTween kütüphanesini dahil et!
using UnityEngine.EventSystems; // LayoutRebuilder için gerekli

public class SlotManager : MonoBehaviour
{
    public static SlotManager Instance;

    public List<Slot> slots = new List<Slot>();

    // Unity Inspector'dan atayın: Slotların tamamını içeren GameObject'teki LayoutGroup bileşeni.
    public LayoutGroup slotsLayoutGroup;

    // Geçici ikon kopyaları prefab'ı artık kullanılmıyor, bu değişken silinebilir veya boş bırakılabilir.
    // public GameObject floatingIconPrefab; 

    // Görsel kaydırma (pre-shift) animasyonunun süresi
    public float visualShiftAnimationDuration = 0.2f;

    // Eşleşme temizleme animasyonunun süresi
    public float matchClearanceAnimationDuration = 0.25f;
    // Eşleşme animasyonu bittikten sonraki ek bekleme süresi
    public float delayAfterMatchAnimation = 0.1f;

    private void Awake()
    {
        // Yalnızca tek bir SlotManager örneğinin var olduğundan emin olun.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Tekrar eden örnekleri yok edin.
        }
    }

    /// <summary>
    /// İlk boş ve rezerve edilmemiş slotu bulur ve döndürür.
    /// </summary>
    /// <returns>İlk boş slot, bulunamazsa null.</returns>
    public Slot GetFirstEmptySlot()
    {
        foreach (var slot in slots)
        {
            if (!slot.IsOccupied && !slot.IsReserved)
                return slot;
        }
        return null;
    }

    /// <summary>
    /// Veri tarafında slotların içeriğini sağa kaydırır.
    /// Bu metod görsel animasyon yapmaz, sadece Slot objelerinin içindeki veriyi günceller.
    /// </summary>
    /// <param name="startIndex">Kaydırmanın başlayacağı indeks.</param>
    /// <returns>Kaydırma başarılıysa true, değilse false.</returns>
    public bool ShiftRightFromData(int startIndex)
    {
        Debug.Log($"[ShiftRightFromData] Called with startIndex = {startIndex}");
        if (startIndex < 0 || startIndex >= slots.Count)
        {
            Debug.LogError($"ShiftRightFromData called with invalid startIndex: {startIndex}. Slots count: {slots.Count}");
            return false;
        }

        // Sondan başlayarak hedef startIndex'e kadar olan veriyi sağa kaydır.
        // Son slotun içeriği dışarı itilecektir.
        for (int i = slots.Count - 2; i >= startIndex; i--)
        {
            Slot current = slots[i];
            Slot next = slots[i + 1];
            Debug.Log($"Slot {i} isOccupied = {current.IsOccupied}, StoredUniqueID = {current.StoredUniqueID}");
            if (current.IsOccupied)
            {
                // Veriyi kopyala
                next.CopyDataFrom(current);
            }
            else
            {
                // Mevcut slot boşsa, yanındaki slotu da temizle (boşluğu propagate et)
                next.ClearDataOnly();
            }
        }

        // Başlangıç indeksi artık boşaltılmış veri olarak ayarlanır.
        slots[startIndex].ClearDataOnly();
        return true;
    }

    /// <summary>
    /// Belirli bir indeksten başlayarak dolu slotların ikonlarını görsel olarak sağa kaydırır.
    /// Bu metod sadece görsel animasyon yapar, slotların içindeki veriyi değiştirmez.
    /// Orijinal ikonlar hareket ettirilir ve sonra gizlenir.
    /// </summary>
    /// <param name="startIndex">Kaydırmanın başlayacağı indeks.</param>
    /// <param name="duration">Animasyonun süresi.</param>
    public IEnumerator PerformVisualShift(int startIndex, float duration)
    {
         // Animasyon sırasında Layout Group'ın otomatik düzenlemesini devre dışı bırakın.
         if (slotsLayoutGroup != null) slotsLayoutGroup.enabled = false;

         Sequence shiftSequence = DOTween.Sequence();

         // Kaydırılacak ikonların listesi ve hedef pozisyonları
         List<Image> iconsToShift = new List<Image>();
         List<Vector3> targetPositions = new List<Vector3>();
        /*
         // Sondan başlayarak hedef startIndex'e kadar olan dolu slotları görsel olarak kaydırın.
         for (int i = slots.Count - 2; i >= startIndex; i--)
         {
             Slot currentSlot = slots[i];
             Slot nextSlot = slots[i + 1];

             if (currentSlot.IsOccupied && currentSlot.iconImage != null)
             {
                 // İkonu animasyon için görünür ve tam alfa yap (zaten gizlenmişse)
                 currentSlot.iconImage.enabled = true;
                 currentSlot.iconImage.color = new Color(currentSlot.iconImage.color.r, currentSlot.iconImage.color.g, currentSlot.iconImage.color.b, 1);

                 iconsToShift.Add(currentSlot.iconImage);
                 // Objenin UI mı yoksa 3D mi olduğuna göre hedef pozisyonu belirle
                 if (currentSlot.iconImage.rectTransform != null) // UI elemanı varsayımı
                 {
                     targetPositions.Add(nextSlot.GetComponent<RectTransform>().anchoredPosition);
                 }
                 else // 3D obje
                 {
                     targetPositions.Add(GetSlotWorldPosition(nextSlot, currentSlot.GetComponentInParent<Canvas>()));
                 }
             }
         }

         // Tüm ikonların hareket animasyonlarını başlatın
         for (int i = 0; i < iconsToShift.Count; i++)
         {
             Image icon = iconsToShift[i];
             Vector3 targetPos = targetPositions[i];

             if (icon != null)
             {
                 if (icon.rectTransform != null) // UI elemanı
                 {
                     shiftSequence.Join(icon.rectTransform.DOAnchorPos(targetPos, duration).SetEase(Ease.OutQuad));
                 }
                 else // 3D obje
                 {
                     shiftSequence.Join(icon.transform.DOMove(targetPos, duration).SetEase(Ease.OutQuad));
                 }

                 // Kayan ikonları yavaşça görünmez yapın
                 shiftSequence.Join(
                     icon.DOFade(0, duration)
                         .SetEase(Ease.InQuad) // Hızlı başlangıç, yavaş bitiş (görsel kaybolma için uygun)
                 );
             }
         }

         yield return shiftSequence.WaitForCompletion(); // Tüm animasyonların bitmesini */

        // Animasyon bittikten sonra orijinal ikonları gizleyin ve sprite'larını temizleyin.
        // Bu adım, PerformVisualShift bittikten sonra slot verisi ShiftRightFromData ile güncellenmeden önce yapılır.
        yield return new WaitForSeconds(0f);
        /*foreach (var icon in iconsToShift)
        {
            if (icon != null)
            {
                icon.enabled = false;
                icon.sprite = null;
                icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, 1); // Alfa değerini sıfırla
                icon.rectTransform.localScale = Vector3.one; // Ölçeği sıfırla
            }
        }*/

        // Boşalan startIndex slotunun ikonunu da temizleyin (eğer zaten gizlenmemişse)
        if (slots[startIndex].iconImage != null)
        {
            slots[startIndex].iconImage.enabled = false;
            // slots[startIndex].iconImage.sprite = null; // ❌ BUNU SİL!
            slots[startIndex].iconImage.rectTransform.localScale = Vector3.one;
            slots[startIndex].iconImage.color = new Color(
                slots[startIndex].iconImage.color.r,
                slots[startIndex].iconImage.color.g,
                slots[startIndex].iconImage.color.b,
                0); // sadece görünmez yap
        }

        // Animasyon bittikten sonra Layout Group'ı tekrar etkinleştirin ve yenileyin.
        if (slotsLayoutGroup != null) slotsLayoutGroup.enabled = true;
        RefreshLayout();
    }

    /// <summary>
    /// Bellekteki slot verilerini sıkıştırır, boşlukları kaldırır.
    /// Bu metod sadece veriyi manipüle eder, görsel animasyon yapmaz.
    /// </summary>
    public void CompactSlots()
    {
        List<(Sprite sprite, string id)> occupiedItems = new List<(Sprite, string)>();

        foreach (var slot in slots)
        {
            if (slot.IsOccupied)
                occupiedItems.Add((slot.iconImage.sprite, slot.StoredUniqueID));
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < occupiedItems.Count)
            {
                slots[i].FillSlotDataOnly(occupiedItems[i].sprite, occupiedItems[i].id);
                // Kompaktlaşma sırasında ikonları anlık olarak görünür yapın
                if (slots[i].iconImage != null)
                {
                    slots[i].iconImage.enabled = true;
                    slots[i].iconImage.color = new Color(slots[i].iconImage.color.r, slots[i].iconImage.color.g, slots[i].iconImage.color.b, 1);
                    slots[i].iconImage.rectTransform.localScale = Vector3.one; // Ölçeği sıfırla
                }
            }
            else
            {
                slots[i].ClearSlot(); // Hem veriyi hem görseli temizleyin
            }
        }
        RefreshLayout(); // Veri değiştiği için layout'u yenileyin
    }

    /// <summary>
    /// Mevcut bir objenin yerleşeceği en uygun slot indeksini belirler.
    /// </summary>
    /// <param name="id">Yerleştirilecek objenin benzersiz kimliği.</param>
    /// <returns>Uygun slot objesi, bulunamazsa null.</returns>
    public Slot GetCurrentValidInsertSlot(string id)
    {
        int index = FindInsertIndex(id);
        if (index >= 0 && index < slots.Count)
            return slots[index];

        return null;
    }

    /// <summary>
    /// Belirli bir slotun dünya pozisyonunu döndürür.
    /// </summary>
    public Vector3 GetSlotWorldPosition(Slot slot, Canvas canvas)
    {
        RectTransform rect = slot.GetComponent<RectTransform>();
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, rect.position);
        float z = Mathf.Abs(Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, z));
    }

    /// <summary>
    /// Yeni objeyi slot verisine yerleştirir. Bu metod, görsel ön-kaydırma animasyonu bittikten sonra çağrılmalıdır.
    /// </summary>
    /// <param name="id">Objenin kimliği.</param>
    /// <param name="sprite">Objenin sprite'ı.</param>
    /// <param name="targetIndex">Objenin yerleştirileceği nihai indeks.</param>
    public void PlaceObjectDataOnly(string id, Sprite sprite, int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= slots.Count)
        {
            Debug.LogError($"PlaceObjectDataOnly called with invalid targetIndex: {targetIndex}");
            return;
        }

        // Önce slotun verisini doldurun
        slots[targetIndex].FillSlotDataOnly(sprite, id);

        // İkonu görünür yapın ve alfa değerini 0'dan 1'e animasyonlu getirin
        if (slots[targetIndex].iconImage != null)
        {
            slots[targetIndex].iconImage.enabled = true; // İkonu etkinleştirin
            // Başlangıçta şeffaf yapıp sonra animasyonlu görünür hale getirin
            slots[targetIndex].iconImage.color = new Color(slots[targetIndex].iconImage.color.r, slots[targetIndex].iconImage.color.g, slots[targetIndex].iconImage.color.b, 0);
            slots[targetIndex].iconImage.DOFade(1, 0.1f); // Yavaşça görünür yapın

            // İsteğe bağlı olarak PunchScale animasyonunu da burada çağırabiliriz
            StartCoroutine(slots[targetIndex].PunchScaleCoroutine());
        }
    }

    /// <summary>
    /// Slot verisinde değişiklikler (yerleştirme, eşleşme, sıkıştırma) sonrası işlemleri yönetir.
    /// </summary>
    /// <param name="sourceCanvas">UI Canvas referansı.</param>
    /// <param name="originatingClickableObject">Bu süreci başlatan ClickableObject.</param>
    public IEnumerator ProcessSlotChanges(Canvas sourceCanvas, ClickableObject originatingClickableObject)
    {
        // Eşleşme kontrolü ve animasyon (varsa)
        yield return StartCoroutine(CheckForMatchesCoroutine(sourceCanvas));

        // Potansiyel eşleşmeler temizlendikten sonra slotları sıkıştırın.
        CompactSlots();

        // Sıkıştırma sonrası yeni eşleşmeleri kontrol edin (kaskadları ele alır)
        yield return StartCoroutine(CheckForMatchesCoroutine(sourceCanvas));

        // Tüm işlemler bittiğinde ClickableObject'i yok edin.
        if (originatingClickableObject != null) Destroy(originatingClickableObject.gameObject);
    }

    /// <summary>
    /// Yeni objeyi slot sistemine yerleştirmeye çalışır. Bu metod, kaydırma ve eşleşme mantığını yönetir.
    /// </summary>
    /// <param name="id">Yerleştirilecek objenin kimliği.</param>
    /// <param name="sprite">Yerleştirilecek objenin sprite'ı.</param>
    /// <param name="sourceCanvas">UI konum dönüşümleri için kullanılan Canvas.</param>
    /// <param name="originatingClickableObject">Bu işlemi başlatan ClickableObject.</param>
    public void TryPlaceObject(string id, Sprite sprite, Canvas sourceCanvas, ClickableObject originatingClickableObject)
    {
        int insertIndex = FindInsertIndex(id);
        if (insertIndex == -1)
        {
            Debug.Log($"Cannot place object '{id}': No suitable slot found.");
            // Uygun slot bulunamazsa ClickableObject'i yok edin.
            if (originatingClickableObject != null) Destroy(originatingClickableObject.gameObject);
            return; // Metottan çıkın
        }

        // Eğer hesaplanan insertIndex zaten doluysa, mevcut öğeleri sağa kaydırmamız gerekir.
        // Bu, FindInsertIndex'in belirlediği "bloğun başına kaydır" senaryosudur.
        if (slots[insertIndex].IsOccupied)
        {
            // Görsel kaydırma animasyonunu başlatın ve bitmesini bekleyin
            StartCoroutine(HandlePlacementWithVisualShift(id, sprite, sourceCanvas, insertIndex, originatingClickableObject));
        }
        else // Hedef slot zaten boşsa, doğrudan yerleştirme yapın
        {
            PlaceObjectDataOnly(id, sprite, insertIndex);
            Debug.Log($"Placing sprite for ID {id}, sprite name = {(sprite == null ? "NULL" : sprite.name)} at index {insertIndex}");
            // Görsel kaydırma olmadığı için, yerleştirme sonrası süreçleri doğrudan başlatın
            StartCoroutine(ProcessSlotChanges(sourceCanvas, originatingClickableObject));
        }
    }

    /// <summary>
    /// Görsel kaydırma (pre-shift) gerektiren yerleşim senaryolarını yöneten coroutine.
    /// </summary>
    private IEnumerator HandlePlacementWithVisualShift(string id, Sprite sprite, Canvas sourceCanvas, int insertIndex, ClickableObject originatingClickableObject)
    {
        // Görsel kaydırma animasyonunu başlatın ve bitmesini bekleyin
        yield return StartCoroutine(PerformVisualShift(insertIndex, visualShiftAnimationDuration));

        // Görsel kaydırma bittikten sonra, veri tarafında kaydırma işlemini gerçekleştirin.
        bool shifted = ShiftRightFromData(insertIndex);
        if (!shifted)
        {
            Debug.LogWarning($"Failed to shift elements (data only) for object '{id}' at index {insertIndex}.");
            // Kaydırma başarısız olursa ClickableObject'i yok edin.
            if (originatingClickableObject != null) Destroy(originatingClickableObject.gameObject);
            yield break; // Coroutine'den çıkın
        }

        // Artık 'insertIndex' slotunun boş olduğu garantilendi, yeni objeyle doldurun.
        PlaceObjectDataOnly(id, sprite, insertIndex);

        // Yerleştirme sonrası süreçleri başlatın
        yield return StartCoroutine(ProcessSlotChanges(sourceCanvas, originatingClickableObject));
    }

    /// <summary>
    /// Sıralı eşleşmeleri kontrol eder ve 3 veya daha fazla özdeş, ardışık öğe grubunu temizler.
    /// Bu bir coroutine'dir ve eşleşme temizleme sırasında animasyon ve gecikmelere izin verir.
    /// </summary>
    private IEnumerator CheckForMatchesCoroutine(Canvas sourceCanvas)
    {
        if (slots.Count < 3)
            yield break; // Hiçbir şey kontrol edilmeyecek

        int count = 1; // Ardışık özdeş öğelerin sayısını izler
        for (int i = 1; i < slots.Count; i++)
        {
            // Geçerli slot ve önceki slotun dolu olup olmadığını ve aynı benzersiz ID'ye sahip olup olmadığını kontrol edin.
            if (slots[i].IsOccupied && slots[i - 1].IsOccupied &&
                slots[i].StoredUniqueID == slots[i - 1].StoredUniqueID)
            {
                count++;
            }
            else // Uyumsuzluk veya boş slot, bu nedenle mevcut sırayı değerlendirin.
            {
                if (count >= 3)
                {
                    // Bir eşleşme bulundu, animasyonu başlatın ve temizleyin.
                    yield return StartCoroutine(AnimateMatchClearance(i - count, count, sourceCanvas));
                    // Bir eşleşme temizlendikten sonra durabiliriz ve ProcessSlotChanges
                    // sonraki adımları (sıkıştırma ve kaskatlar için tekrar kontrol etme) yönetmesine izin verebiliriz.
                    yield break;
                }
                count = 1; // Yeni sıra için sayacı sıfırlayın.
            }
        }

        // Döngüden sonra, slot listesinin en sonunda bir eşleşme olup olmadığını kontrol edin.
        if (count >= 3)
        {
            yield return StartCoroutine(AnimateMatchClearance(slots.Count - count, count, sourceCanvas));
        }
    }

    /// <summary>
    /// Eşleşen öğeleri eşleşmenin ilk slotuna doğru animasyonlu bir şekilde birleştirir, sonra onları temizler.
    /// </summary>
    /// <param name="startIndex">Eşleşen sıranın başlangıç indeksi.</param>
    /// <param name="count">Eşleşen sıradaki öğe sayısı.</param>
    /// <param name="sourceCanvas">UI konum dönüşümleri için kullanılan Canvas.</param>
    private IEnumerator AnimateMatchClearance(int startIndex, int count, Canvas sourceCanvas)
    {
        /*Sequence matchSequence = DOTween.Sequence();
        // Eşleşmenin ilk slotunun dünya pozisyonu (tüm eşleşen ikonlar buraya doğru kayacak)
        Vector3 targetWorldPos = GetSlotWorldPosition(slots[startIndex], sourceCanvas);

        for (int i = startIndex; i < startIndex + count; i++)
        {
            if (slots[i].IsOccupied && slots[i].iconImage != null)
            {
                // Mevcut slotun ikonunun görsel transformunu hareket ettirin.
                // Objenin UI mı yoksa 3D mi olduğuna göre DOMove veya DOAnchorPos kullanın.
                if (slots[i].iconImage.rectTransform != null) // UI elemanı varsayımı
                {
                    matchSequence.Join(
                        slots[i].iconImage.rectTransform.DOAnchorPos(slots[startIndex].GetComponent<RectTransform>().anchoredPosition, matchClearanceAnimationDuration)
                            .SetEase(Ease.OutQuad)
                    );
                }
                else // 3D obje
                {
                    matchSequence.Join(
                        slots[i].iconImage.transform.DOMove(targetWorldPos, matchClearanceAnimationDuration)
                            .SetEase(Ease.OutQuad)
                    );
                }
                // Eşleşen ikonları yavaşça görünmez yapın.
                matchSequence.Join(slots[i].iconImage.DOFade(0, matchClearanceAnimationDuration / 2));
            }
        }

        yield return matchSequence.WaitForCompletion(); // Animasyonun bitmesini bekleyin*/
        yield return new WaitForSeconds(delayAfterMatchAnimation); // Animasyon sonrası kısa bir bekleme

        // Şimdi slotların verilerini temizleyin
        for (int j = startIndex; j < startIndex + count; j++)
        {
            slots[j].ClearDataOnly(); // Sadece veriyi temizle
            // Görseli de anında sıfırlayın, animasyon bittiği için görünmez kalmalı
            if (slots[j].iconImage != null)
            {
                slots[j].iconImage.sprite = null;
                slots[j].iconImage.enabled = false;
                slots[j].iconImage.rectTransform.localScale = Vector3.one;
                slots[j].iconImage.color = new Color(slots[j].iconImage.color.r, slots[j].iconImage.color.g, slots[j].iconImage.color.b, 0); // Alfa değerini sıfırla
            }
        }
        RefreshLayout(); // Veri değiştiği için layout'u yenileyin
    }

    /// <summary>
    /// Belirli bir ID'ye sahip nesneyi yerleştirmek için uygun indeksi bulur.
    /// Öncelik:
    /// 1. Eğer aynı ID'ye sahip bir blok varsa ve hemen sağında boş slot varsa, oraya yerleştir.
    /// 2. Eğer aynı ID'ye sahip bir blok varsa ve hemen sağındaki slot doluysa, bloğun başına kaydır.
    /// 3. Hiç aynı ID'ye sahip öğe yoksa, ilk boş slota yerleştir.
    /// </summary>
    /// <param name="id">Yerleştirilecek objenin benzersiz ID'si.</param>
    /// <returns>Hesaplanan ekleme indeksi veya uygun slot bulunamazsa -1.</returns>
    private int FindInsertIndex(string id)
    {
        int firstBlockIndex = -1;
        int lastBlockIndex = -1;

        // 1. Aynı ID'ye sahip ardışık bir bloğun başlangıcını ve sonunu bulun.
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsOccupied && slots[i].StoredUniqueID == id)
            {
                if (firstBlockIndex == -1) // Bu ID'yi ilk kez görüyorsak
                    firstBlockIndex = i;
                lastBlockIndex = i; // En son görülen indeksi güncelleyin
            }
            else if (firstBlockIndex != -1) // Bir bloktan sonra farklı bir şey veya boşluk varsa
            {
                break; // Ardışık blok burada bitti
            }
        }

        // Eğer aynı ID'ye sahip bir blok bulunduysa
        if (firstBlockIndex != -1)
        {
            int nextSlotAfterBlock = lastBlockIndex + 1;

            // Senaryo 1: Bloğun hemen sağındaki slot boşsa -> Bloğun sonuna ekleyin
            if (nextSlotAfterBlock < slots.Count && !slots[nextSlotAfterBlock].IsOccupied)
            {
                return nextSlotAfterBlock; // Bloğun sonundaki boş slota yerleştirin
            }
            else // Senaryo 2: Bloğun hemen sağındaki slot doluysa veya dışındaysa -> Bloğun başına kaydırın
            {
                // Bu durumda, tüm bloğu sağa kaydırıp yeni öğeyi başına eklemek istiyoruz.
                // ShiftRightFromData metodu startIndex'teki slotu temizler ve sağa kaydırma yapar.
                return firstBlockIndex;
            }
        }
        // Eğer aynı ID'ye sahip bir blok bulunamadıysa (firstBlockIndex hala -1)
        else
        {
            // İlk boş slotu bulun
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsOccupied)
                {
                    return i;
                }
            }
            // Boş slot bulunamadı
            return -1;
        }
    }

    /// <summary>
    /// Tüm slotları temizler.
    /// </summary>
    public void ClearAllSlots()
    {
        foreach (var slot in slots)
        {
            slot.ClearSlot(); // Hem veriyi hem de görseli temizler
        }
        RefreshLayout(); // Layout'u yeniler
    }

    /// <summary>
    /// UI Layout Group'u manuel olarak yeniden oluşturulması için işaretler.
    /// Bu, slotların pozisyonlarının güncel veriye göre ayarlanmasını sağlar.
    /// </summary>
    private void RefreshLayout()
    {
        if (slotsLayoutGroup != null)
        {
            LayoutRebuilder.MarkLayoutForRebuild(slotsLayoutGroup.GetComponent<RectTransform>());
        }
    }
}
