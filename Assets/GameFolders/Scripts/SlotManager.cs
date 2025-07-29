using UnityEngine;
using System.Collections.Generic;
using System.Collections; // Coroutine'ler için gerekli
using UnityEngine.UI;
using DG.Tweening; // DOTween kütüphanesini dahil et!
using UnityEngine.EventSystems;
using System.Linq; // LayoutRebuilder için gerekli

public class SlotManager : MonoBehaviour
{
    public static SlotManager Instance;
    public List<Slot> slots = new List<Slot>();

    [Header("Movement Timings")]
    public float directMoveDuration = 0.1f;
    public float visualShiftDuration = 0.2f;
    public float matchClearanceDuration = 0.25f;
    public float postMatchDelay = 0.1f;

    [Header("Easing Settings")]
    [Tooltip("Slot'a yürüyüşte kullanılacak Ease tipi")]
    public Ease moveEase = Ease.InOutSine;

    [Tooltip("Pre‑shift animasyonunda kullanılacak Ease tipi")]
    public Ease shiftEase = Ease.InOutQuad;

    [Tooltip("Match ölçek animasyonunda kullanılacak Ease tipi")]
    public Ease matchEase = Ease.InQuad;

    [Header("Movement")]
    public float directMoveSpeed = 5f;

    [Header("Click Timing")]
    [Tooltip("Bu süreden kısa aralıklarla gelen tıklamalar rapidClickCount'i artırır")]
    public float rapidClickThreshold = 0.25f;
    [Tooltip("Rapid tıklama başına eklenecek temel gecikme")]
    public float baseMoveDelay = 0.1f;
    [Tooltip("Maksimum gecikme sınırı")]
    public float maxMoveDelay = 0.5f;

    private float lastClickTime = -Mathf.Infinity;
    private int rapidClickCount = 0;

    // --- bu ikisi eklendi ---
    // hareket halindeki nesneleri takip edeceğiz
    private Dictionary<ClickableObject, string> movingItems = new Dictionary<ClickableObject, string>();
    private Dictionary<ClickableObject, int> reservedIndices = new Dictionary<ClickableObject, int>();
    private Dictionary<int, float> slotNextAvailableTime = new Dictionary<int, float>();

    // **YENİ**: İşlem kilidi ve kuyruk sistemi
    private bool isProcessingPlacement = false;
    private Queue<(GameObject item, string id)> placementQueue = new Queue<(GameObject item, string id)>();
    private HashSet<string> processingIds = new HashSet<string>();
    
    // **YENİ**: Aynı anda tıklanan objeleri handle etmek için
    private Dictionary<string, List<GameObject>> pendingObjects = new Dictionary<string, List<GameObject>>();
    private Dictionary<string, float> lastProcessTime = new Dictionary<string, float>();
    private const float SAME_ID_PROCESS_DELAY = 0.1f; // Aynı ID'li objeler arası minimum süre
    
    // **YENİ**: Match işlemi sırasında gelen objeleri handle etmek için
    private bool isProcessingMatch = false;
    private Queue<(GameObject item, string id)> matchQueue = new Queue<(GameObject item, string id)>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Tüm slotları 0 olarak başlatıyoruz (yani hemen kullanılabilir)
        for (int i = 0; i < slots.Count; i++)
            slotNextAvailableTime[i] = 0f;
    }

    /// <summary>
    /// Objeyi slota yerleştirme akışını yöneten coroutine.
    /// </summary>
    /// <summary>
    /// Tıklama geldiğinde: rezerv hemen koy, ardından hareketi başlat.
    /// </summary>
    public void TryPlaceObject3D(GameObject item, string id)
    {
        // **YENİ**: Match işlemi sırasında gelen objeleri match kuyruğuna ekle
        if (isProcessingMatch)
        {
            matchQueue.Enqueue((item, id));
            Debug.Log($"[TryPlaceObject3D] Match processing, added to match queue. Queue count: {matchQueue.Count}");
            return;
        }

        // **YENİ**: Aynı ID'li obje zaten işleniyorsa veya çok yakın zamanda işlendiyse
        if (processingIds.Contains(id))
        {
            // Aynı ID'li objeleri pending listesine ekle
            if (!pendingObjects.ContainsKey(id))
                pendingObjects[id] = new List<GameObject>();
            pendingObjects[id].Add(item);
            Debug.Log($"[TryPlaceObject3D] Same ID {id} already processing, added to pending list. Pending count: {pendingObjects[id].Count}");
            return;
        }

        // **YENİ**: Aynı ID için minimum süre kontrolü
        if (lastProcessTime.ContainsKey(id) && Time.time - lastProcessTime[id] < SAME_ID_PROCESS_DELAY)
        {
            // Aynı ID'li objeleri pending listesine ekle
            if (!pendingObjects.ContainsKey(id))
                pendingObjects[id] = new List<GameObject>();
            pendingObjects[id].Add(item);
            Debug.Log($"[TryPlaceObject3D] Same ID {id} processed too recently, added to pending list. Pending count: {pendingObjects[id].Count}");
            return;
        }

        // **YENİ**: İşlem kilidi kontrolü
        if (isProcessingPlacement)
        {
            placementQueue.Enqueue((item, id));
            return;
        }


        item.GetComponent<ClickableObject>().HandleRigidBody();
        StartCoroutine(ProcessPlacement(item, id));
    }

    private IEnumerator ProcessPlacement(GameObject item, string id)
    {
        isProcessingPlacement = true;
        processingIds.Add(id);

        // ————— rapid tıklama sayısını güncelle —————
        float now = Time.time;
        if (now - lastClickTime < rapidClickThreshold)
            rapidClickCount++;
        else
            rapidClickCount = 0;
        lastClickTime = now;

        // delay = base * count, clampla
        float delay = Mathf.Clamp(baseMoveDelay * rapidClickCount, 0f, maxMoveDelay);

        // 1) Hedef slotu bul
        int insertIdx = FindInsertIndex(id);
        
        // **YENİ**: Eğer bu ID için pending objeler varsa, normal slot bulma mantığını kullan
        // Alternatif slot kullanmıyoruz çünkü aynı ID'li objeler yan yana olmalı
        if (pendingObjects.ContainsKey(id) && pendingObjects[id].Count > 0)
        {
            Debug.Log($"[ProcessPlacement] Pending objects exist for ID {id}, using normal slot finding logic");
        }
        
        if (insertIdx < 0)
        {
            item.SetActive(false);
            processingIds.Remove(id);
            isProcessingPlacement = false;
            ProcessNextInQueue();
            yield break;
        }

        // **YENİ**: Slot'un gerçekten boş olduğunu kontrol et
        if (!slots[insertIdx].IsAvailable())
        {
            // Slot dolu, bu normal çünkü aynı ID'li blok varsa shift yapılacak
            Debug.Log($"[ProcessPlacement] Slot {insertIdx} is not available, this is expected for same ID blocks");
        }

        // **YENİ**: Aynı ID'li blok varsa shift işlemi yap (hem yerleşmiş hem hareket halindeki)
        bool hasSameIdBlock = false;
        
        // Yerleşmiş objeleri kontrol et
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsOccupied && slots[i].StoredUniqueID == id)
            {
                hasSameIdBlock = true;
                break;
            }
        }
        
        // Hareket halindeki objeleri kontrol et
        if (!hasSameIdBlock)
        {
            foreach (var kvp in movingItems)
            {
                if (kvp.Value == id)
                {
                    hasSameIdBlock = true;
                    break;
                }
            }
        }
        
        if (hasSameIdBlock)
        {
            Debug.Log($"[ProcessPlacement] Performing shift from slot {insertIdx} for ID {id}");
            bool shifted = ShiftRightFromData3D(insertIdx);
            if (!shifted)
            {
                Debug.LogError($"[ProcessPlacement] Shift failed for slot {insertIdx}");
                item.SetActive(false);
                processingIds.Remove(id);
                isProcessingPlacement = false;
                ProcessNextInQueue();
                yield break;
            }
            
            // Shift sonrası slot durumunu kontrol et
            if (!slots[insertIdx].IsAvailable())
            {
                Debug.LogError($"[ProcessPlacement] Slot {insertIdx} still not available after shift");
                item.SetActive(false);
                processingIds.Remove(id);
                isProcessingPlacement = false;
                ProcessNextInQueue();
                yield break;
            }
        }

        // 2) Rezerve et (önce rezervasyon yap)
        slots[insertIdx].SetReserved(true);

        // 3) Mover'ı hazırla ve delay'i ayarla
        var mvNew = item.GetComponent<ClickableObject>()
                    ?? item.AddComponent<ClickableObject>();
        mvNew.speed = directMoveSpeed;
        movingItems[mvNew] = id;
        reservedIndices[mvNew] = insertIdx;
        mvNew.reservedSlotIndex = insertIdx;

        // **yeni**: havada bekleme süresini set et
        mvNew.SetMoveDelay(delay);

        Debug.Log($"[TryPlace] rapidCount={rapidClickCount}, moveDelay={delay:F2}, slot={insertIdx}");

        // 4) Hareketi başlat
        mvNew.BeginMove(insertIdx);

        processingIds.Remove(id);
        lastProcessTime[id] = Time.time; // İşlem zamanını kaydet
        isProcessingPlacement = false;
        
        // **YENİ**: Pending objeleri kontrol et
        ProcessPendingObjects(id);
        ProcessNextInQueue();
    }

    private void ProcessNextInQueue()
    {
        if (placementQueue.Count > 0 && !isProcessingPlacement)
        {
            var next = placementQueue.Dequeue();
            TryPlaceObject3D(next.item, next.id);
        }
    }

    // **YENİ**: Pending objeleri işle
    private void ProcessPendingObjects(string id)
    {
        if (pendingObjects.ContainsKey(id) && pendingObjects[id].Count > 0)
        {
            // Pending listesinden bir obje al ve işle
            var pendingItem = pendingObjects[id][0];
            pendingObjects[id].RemoveAt(0);
            
            Debug.Log($"[ProcessPendingObjects] Processing pending object for ID {id}. Remaining pending: {pendingObjects[id].Count}");
            
            // Eğer pending listesi boşsa, listeyi temizle
            if (pendingObjects[id].Count == 0)
            {
                pendingObjects.Remove(id);
            }
            
            // Objeyi işle
            TryPlaceObject3D(pendingItem, id);
        }
    }

    // **YENİ**: Match kuyruğundaki objeleri işle
    private void ProcessMatchQueue()
    {
        Debug.Log($"[ProcessMatchQueue] Processing {matchQueue.Count} items from match queue");
        
        while (matchQueue.Count > 0)
        {
            var next = matchQueue.Dequeue();
            Debug.Log($"[ProcessMatchQueue] Processing queued item with ID {next.id}");
            TryPlaceObject3D(next.item, next.id);
        }
    }

    // … OnMovableArrived, ShiftRightFromData3D, vb. aynı kalır …

    /// <summary>
    /// Slot değişikliği: mv'nin eskisini boşalt, yenisini rezerve et, mv'ye bildir.
    /// </summary>
    public void ChangeReservation(ClickableObject mv, int newIdx)
    {
        if (!reservedIndices.ContainsKey(mv)) return;

        int oldIdx = reservedIndices[mv];
        
        // **YENİ**: Eski slot'un gerçekten bu obje için rezerve edildiğini kontrol et
        if (slots[oldIdx].IsReserved)
        {
            slots[oldIdx].SetReserved(false);
        }

        // **YENİ**: Yeni slot'un boş olduğunu kontrol et
        if (slots[newIdx].IsAvailable())
        {
            slots[newIdx].SetReserved(true);
            reservedIndices[mv] = newIdx;
            mv.reservedSlotIndex = newIdx;
        }
    }

    /// <summary>
    /// Artık bu tek satırlık metot yerine coroutine başlatıyoruz.
    /// </summary>
    /*public void TryPlaceObject3D(GameObject item, string id)
    {
        StartCoroutine(TryPlaceObjectRoutine(item, id));
    }*/

    public void OnMovableArrived(ClickableObject mv)
    {
        if (!movingItems.ContainsKey(mv)) return;

        int idx = reservedIndices[mv];
        string id = movingItems[mv];

        Debug.Log($"[OnMovableArrived] Object {id} arrived at slot {idx}, slot.IsReserved={slots[idx].IsReserved}, slot.IsOccupied={slots[idx].IsOccupied}");

        // **YENİ**: Slot'un hala rezerve edildiğini kontrol et
        if (slots[idx].IsReserved)
        {
            slots[idx].AssignOccupant(mv.gameObject, id);
            slots[idx].SetReserved(false);
            Debug.Log($"[OnMovableArrived] Successfully assigned {id} to slot {idx}");
        }
        else
        {
            // Slot artık rezerve değil, yeni slot bul
            Debug.Log($"[OnMovableArrived] Slot {idx} is not reserved, finding new slot for {id}");
            int newIdx = FindInsertIndex(id);
            if (newIdx >= 0 && slots[newIdx].IsAvailable())
            {
                slots[newIdx].SetReserved(true);
                slots[newIdx].AssignOccupant(mv.gameObject, id);
                slots[newIdx].SetReserved(false);
                reservedIndices[mv] = newIdx;
                mv.reservedSlotIndex = newIdx;
                Debug.Log($"[OnMovableArrived] Assigned {id} to new slot {newIdx}");
            }
        }

        movingItems.Remove(mv);
        reservedIndices.Remove(mv);
        mv.reservedSlotIndex = -1;

        if (movingItems.Count == 0)
            StartCoroutine(ProcessSlotChanges());
    }


    public int FindInsertIndex(string id)
    {
        // 1. Aynı ID'li objelerin bloğunu bul (hem yerleşmiş hem hareket halindeki)
        int firstBlock = -1, lastBlock = -1;
        
        // Önce yerleşmiş objeleri kontrol et
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsOccupied && slots[i].StoredUniqueID == id)
            {
                firstBlock = firstBlock < 0 ? i : firstBlock;
                lastBlock = i;
            }
            else if (firstBlock >= 0)
            {
                break;
            }
        }
        
        // Sonra hareket halindeki objeleri kontrol et
        foreach (var kvp in movingItems)
        {
            if (kvp.Value == id)
            {
                int targetSlot = reservedIndices[kvp.Key];
                if (firstBlock < 0)
                {
                    firstBlock = targetSlot;
                    lastBlock = targetSlot;
                }
                else
                {
                    // Eğer bu slot mevcut bloktan sonra geliyorsa, bloğu genişlet
                    if (targetSlot > lastBlock)
                    {
                        lastBlock = targetSlot;
                    }
                    // Eğer bu slot mevcut bloktan önce geliyorsa, bloğu baştan başlat
                    else if (targetSlot < firstBlock)
                    {
                        firstBlock = targetSlot;
                    }
                }
            }
        }

        // 2. Eğer aynı ID'li blok varsa, onun SONUNA yerleştir (shift gerekli)
        if (firstBlock >= 0)
        {
            int targetSlot = lastBlock + 1;
            
            // Eğer hedef slot array sınırları dışındaysa, son slot'u kullan
            if (targetSlot >= slots.Count)
            {
                Debug.LogWarning($"[FindInsertIndex] Target slot {targetSlot} is out of bounds, using last slot {slots.Count - 1}");
                targetSlot = slots.Count - 1;
            }
            
            Debug.Log($"[FindInsertIndex] Same ID block found at {firstBlock}-{lastBlock}, placing at END of block (slot {targetSlot})");
            return targetSlot; // Blokun sonuna yerleştir, shift işlemi yapılacak
        }

        // 3. Aynı ID yoksa, ilk boş slot'a yerleştir
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsAvailable())
            {
                Debug.Log($"[FindInsertIndex] No same ID found, placing at first available slot {i}");
                return i;
            }
        }

        Debug.LogWarning($"[FindInsertIndex] No available slot found for {id}");
        return -1;
    }

    // **YENİ**: Aynı ID'li objeler için alternatif slot bul
    public int FindAlternativeInsertIndex(string id)
    {
        // Aynı ID'li bloktan ÖNCE ilk boş slot'u bul
        int firstBlock = -1, lastBlock = -1;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsOccupied && slots[i].StoredUniqueID == id)
            {
                firstBlock = firstBlock < 0 ? i : firstBlock;
                lastBlock = i;
            }
            else if (firstBlock >= 0)
            {
                break;
            }
        }

        if (firstBlock >= 0)
        {
            // Bloktan ÖNCE ilk boş slot'u bul
            for (int i = 0; i < firstBlock; i++)
            {
                if (slots[i].IsAvailable())
                {
                    Debug.Log($"[FindAlternativeInsertIndex] Found alternative slot {i} BEFORE block for ID {id}");
                    return i;
                }
            }
        }

        // Eğer bloktan önce boş slot yoksa, ilk boş slot'u bul
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsAvailable())
            {
                Debug.Log($"[FindAlternativeInsertIndex] Found first available slot {i} for ID {id}");
                return i;
            }
        }

        return -1;
    }

    public bool ShiftRightFromData3D(int startIndex)
    {
        if (startIndex < 0 || startIndex >= slots.Count)
            return false;

        Debug.Log($"[ShiftRightFromData3D] Starting shift from index {startIndex}");

        // **YENİ**: Shift işlemi sırasında rezervasyonları geçici olarak sakla
        Dictionary<int, ClickableObject> tempReservations = new Dictionary<int, ClickableObject>();
        foreach (var kvp in reservedIndices)
        {
            tempReservations[kvp.Value] = kvp.Key;
        }

        // **YENİ**: Shift öncesi durumu logla
        Debug.Log($"[ShiftRightFromData3D] Before shift - Reserved indices: {string.Join(", ", reservedIndices.Values)}");

        // Sondan başlayarak kaydır (son slot hariç)
        for (int i = slots.Count - 2; i >= startIndex; i--)
        {
            var src = slots[i];
            var dst = slots[i + 1];

            Debug.Log($"[ShiftRightFromData3D] Shifting from {i} to {i + 1}, src.IsOccupied={src.IsOccupied}, src.IsReserved={src.IsReserved}");

            // 1) Data‐only shift: stationar objeleri sağa taşı
            if (src.IsOccupied)
            {
                var occ = src.Occupant;
                var oid = src.StoredUniqueID;
                src.ClearDataOnly();
                dst.ClearSlot();
                dst.AssignOccupant(occ, oid);
                Debug.Log($"[ShiftRightFromData3D] Moved object {oid} from slot {i} to {i + 1}");
            }
            else
            {
                dst.ClearSlot();
            }

            // 2) Rezerve bayrağını taşı
            bool srcRes = src.IsReserved;
            dst.SetReserved(srcRes);
            if (srcRes)
            {
                Debug.Log($"[ShiftRightFromData3D] Moved reservation from slot {i} to {i + 1}");
            }

            // 3) Havada hareket edenleri de aynı anda retarget et
            if (tempReservations.ContainsKey(i))
            {
                var mv = tempReservations[i];
                reservedIndices[mv] = i + 1;
                mv.reservedSlotIndex = i + 1;
                tempReservations.Remove(i);
                tempReservations[i + 1] = mv;
                Debug.Log($"[ShiftRightFromData3D] Retargeted moving object to slot {i + 1}");
            }
        }

        // Başlangıç slotunu temizle
        slots[startIndex].ClearSlot();
        slots[startIndex].SetReserved(false);
        Debug.Log($"[ShiftRightFromData3D] Cleared starting slot {startIndex}");

        // Eğer havada kalsa bile oradaki rezervasyonu sil
        if (tempReservations.ContainsKey(startIndex))
        {
            var mv = tempReservations[startIndex];
            reservedIndices.Remove(mv);
            mv.reservedSlotIndex = -1;
            Debug.Log($"[ShiftRightFromData3D] Removed reservation for moving object at slot {startIndex}");
        }

        // **YENİ**: Shift sonrası durumu logla
        Debug.Log($"[ShiftRightFromData3D] After shift - Reserved indices: {string.Join(", ", reservedIndices.Values)}");

        return true;
    }
    public void CompactSlots3D()
    {
        var buffer = new List<(GameObject obj, string id)>();
        foreach (var s in slots)
            if (s.IsOccupied)
                buffer.Add((s.Occupant, s.StoredUniqueID));

        foreach (var s in slots)
            s.ClearSlot();

        for (int i = 0; i < buffer.Count; i++)
            slots[i].AssignOccupant(buffer[i].obj, buffer[i].id);
    }

    private IEnumerator CheckForMatchesCoroutine3D()
    {
        if (slots.Count < 3) yield break;
        int count = 1;
        for (int i = 1; i < slots.Count; i++)
        {
            if (slots[i].IsOccupied &&
                slots[i - 1].IsOccupied &&
                slots[i].StoredUniqueID == slots[i - 1].StoredUniqueID)
            {
                count++;
            }
            else
            {
                if (count >= 3)
                {
                    // **YENİ**: Sadece ilk 3 tanesini temizle
                    int matchCount = Mathf.Min(count, 3);
                    Debug.Log($"[CheckForMatchesCoroutine3D] Found {count} matching objects, clearing first {matchCount}");
                    yield return AnimateMatchClearance3D(i - count, matchCount);
                    yield break;
                }
                count = 1;
            }
        }
        if (count >= 3)
        {
            // **YENİ**: Son match için de sadece ilk 3 tanesini temizle
            int matchCount = Mathf.Min(count, 3);
            Debug.Log($"[CheckForMatchesCoroutine3D] Found {count} matching objects at end, clearing first {matchCount}");
            yield return AnimateMatchClearance3D(slots.Count - count, matchCount);
        }
    }

    /// <summary>
    /// Eşleşen öğeleri ortada birleştirip, sonra sadece bu üçlü için Destroy yapar.
    /// Diğer hiçbir noktada objeyi yok etmiyoruz.
    /// </summary>
    private IEnumerator AnimateMatchClearance3D(int startIndex, int count)
    {
        var toDestroy = new List<GameObject>();
        Vector3 center = slots[startIndex].transform.position;

        Debug.Log($"[AnimateMatchClearance3D] Starting match clearance for {count} objects at slot {startIndex}");

        for (int i = startIndex; i < startIndex + count; i++)
        {
            var slot = slots[i];
            if (slot.IsOccupied && slot.Occupant != null)
            {
                var obj = slot.Occupant;
                slot.ClearDataOnly();
                toDestroy.Add(obj);
                Debug.Log($"[AnimateMatchClearance3D] Added object {obj.name} to destroy list");
            }
        }

        // Tween sürelerini ve easing tipini inspector'dan al
        foreach (var obj in toDestroy)
        {
            obj.transform
               .DOMove(center, matchClearanceDuration)
               .SetEase(shiftEase);
            obj.transform
               .DOScale(Vector3.zero, matchClearanceDuration)
               .SetEase(matchEase);
        }

        yield return new WaitForSeconds(matchClearanceDuration + postMatchDelay);

        foreach (var obj in toDestroy)
        {
            ObjectPoolManager.Instance.ReturnObjectToPool(obj); // Sadece match sonrası
        }
        
        Debug.Log($"[AnimateMatchClearance3D] Match clearance completed, destroyed {toDestroy.Count} objects");
    }

    /// <summary>
    /// Şu anda ne kadar boş (veya rezerve edilmemiş) slot varsa
    /// en az bir tane kalmış mı kontrol eder.
    /// </summary>
    public bool HasFreeSlot()
    {
        // **YENİ**: IsAvailable metodunu kullan
        foreach (var s in slots)
            if (s.IsAvailable())
                return true;
        return false;
    }
    
    private IEnumerator ProcessSlotChanges()
    {
        isProcessingMatch = true;
        Debug.Log("[ProcessSlotChanges] Starting match processing");
        
        yield return StartCoroutine(CheckForMatchesCoroutine3D());
        CompactSlots3D();
        yield return StartCoroutine(CheckForMatchesCoroutine3D());
        
        isProcessingMatch = false;
        Debug.Log("[ProcessSlotChanges] Match processing completed");
        
        // **YENİ**: Match kuyruğundaki objeleri işle
        ProcessMatchQueue();
    }

    // **YENİ**: Debug için slot durumlarını yazdır
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugSlotStates()
    {
        Debug.Log("=== Slot States ===");
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            Debug.Log($"Slot {i}: Occupied={slot.IsOccupied}, Reserved={slot.IsReserved}, Available={slot.IsAvailable()}, ID={slot.StoredUniqueID}");
        }
        Debug.Log($"Moving Items: {movingItems.Count}, Reserved Indices: {reservedIndices.Count}");
        Debug.Log($"Pending Objects: {pendingObjects.Count} IDs, Processing IDs: {processingIds.Count}");
        Debug.Log($"Match Queue: {matchQueue.Count} items, Processing Match: {isProcessingMatch}");
        
        // Pending objeleri detaylı göster
        foreach (var kvp in pendingObjects)
        {
            Debug.Log($"Pending ID {kvp.Key}: {kvp.Value.Count} objects waiting");
        }
        Debug.Log("==================");
    }

    // **YENİ**: Aynı ID'li blokları test etmek için debug metodu
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void TestSameIdPlacement(string testId)
    {
        Debug.Log($"[TestSameIdPlacement] Testing placement for ID: {testId}");
        
        // Mevcut durumu göster
        Debug.Log("Current slot states:");
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot.IsOccupied)
            {
                Debug.Log($"  Slot {i}: ID={slot.StoredUniqueID}");
            }
        }
        
        // FindInsertIndex'i test et
        int insertIndex = FindInsertIndex(testId);
        Debug.Log($"[TestSameIdPlacement] FindInsertIndex returned: {insertIndex}");
        
        if (insertIndex >= 0 && insertIndex < slots.Count)
        {
            Debug.Log($"[TestSameIdPlacement] Target slot {insertIndex} is available: {slots[insertIndex].IsAvailable()}");
        }
    }

    // **YENİ**: Tüm slotları temizle (emergency reset)
    public void EmergencyReset()
    {
        foreach (var slot in slots)
        {
            slot.ForceClear();
        }
        
        movingItems.Clear();
        reservedIndices.Clear();
        placementQueue.Clear();
        matchQueue.Clear();
        processingIds.Clear();
        pendingObjects.Clear();
        lastProcessTime.Clear();
        isProcessingPlacement = false;
        isProcessingMatch = false;
        
        Debug.Log("[SlotManager] Emergency reset completed");
    }

    public void ClearAllSlots()
    {
        foreach(var slot in slots)
        {
            slot.ClearSlot();
        }
    }
}
