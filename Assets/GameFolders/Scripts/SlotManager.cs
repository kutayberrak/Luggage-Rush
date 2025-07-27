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

    [Header("Movement Timings")]
    public float directMoveDuration = 0.1f;
    public float visualShiftDuration = 0.2f;
    public float matchClearanceDuration = 0.25f;
    public float postMatchDelay = 0.1f;

    [Header("Easing Settings")]
    [Tooltip("Slot’a yürüyüşte kullanılacak Ease tipi")]
    public Ease moveEase = Ease.InOutSine;

    [Tooltip("Pre‑shift animasyonunda kullanılacak Ease tipi")]
    public Ease shiftEase = Ease.InOutQuad;

    [Tooltip("Match ölçek animasyonunda kullanılacak Ease tipi")]
    public Ease matchEase = Ease.InQuad;

    [Header("Movement")]
    public float directMoveSpeed = 5f;

    [Header("Click Timing")]
    [Tooltip("Bu süreden daha hızlı ikinci tıklama olursa delay uygula")]
    public float rapidClickThreshold = 0.25f;
    [Tooltip("Havada bekleme süresi (saniye)")]
    public float moveDelayDuration = 0.2f;

    private float lastClickTime = -Mathf.Infinity;

    // --- bu ikisi eklendi ---
    // hareket halindeki nesneleri takip edeceğiz
    private Dictionary<ClickableObject, string> movingItems = new Dictionary<ClickableObject, string>();
    private Dictionary<ClickableObject, int> reservedIndices = new Dictionary<ClickableObject, int>();
    private Dictionary<int, float> slotNextAvailableTime = new Dictionary<int, float>();

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
        Debug.Log($"[TryPlace] Clicked id={id} at time={Time.time:F2}");

        int idx = FindInsertIndex(id);
        Debug.Log($"[TryPlace] FindInsertIndex -> {idx}");

        if (idx < 0)
        {
            Debug.Log($"[TryPlace] No slot for id={id}, deactivating.");
            item.SetActive(false);
            return;
        }

        slots[idx].SetReserved(true);
        Debug.Log($"[TryPlace] Slot {idx} reserved for id={id}");

        var mv = item.GetComponent<ClickableObject>() ?? item.AddComponent<ClickableObject>();
        mv.speed = directMoveSpeed;
        movingItems[mv] = id;
        reservedIndices[mv] = idx;
        mv.reservedSlotIndex = idx;

        StartCoroutine(PlaceCoroutine(item, id, idx, mv));
    }

    // … OnMovableArrived, ShiftRightFromData3D, vb. aynı kalır …

    /// <summary>
    /// Slot değişikliği: mv’nin eskisini boşalt, yenisini rezerve et, mv’ye bildir.
    /// </summary>
    public void ChangeReservation(ClickableObject mv, int newIdx)
    {
        int oldIdx = reservedIndices[mv];
        slots[oldIdx].SetReserved(false);

        slots[newIdx].SetReserved(true);
        reservedIndices[mv] = newIdx;
        mv.reservedSlotIndex = newIdx;
    }

    private IEnumerator PlaceCoroutine(GameObject item, string id, int idx, ClickableObject mv)
    {
        Debug.Log($"[PlaceCoroutine] Start idx={idx} IsOccupied={slots[idx].IsOccupied}");

        if (slots[idx].IsOccupied)
        {
            Debug.Log($"[PlaceCoroutine] Slot {idx} occupied, shifting data right from {idx}");
            bool shifted = ShiftRightFromData3D(idx);
            Debug.Log($"[PlaceCoroutine] ShiftRightFromData3D returned {shifted}");
        }

        Debug.Log($"[PlaceCoroutine] BeginMove to slot {idx}");
        mv.BeginMove(idx);
        yield break;
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
        Debug.Log($"[Arrived] {mv.UniqueID} arrived at reservedSlotIndex={mv.reservedSlotIndex}");

        if (!movingItems.ContainsKey(mv))
        {
            Debug.LogWarning($"[Arrived] {mv.UniqueID} not found in movingItems!");
            return;
        }

        int idx = reservedIndices[mv];
        string id = movingItems[mv];
        Debug.Log($"[Arrived] processing id={id} at idx={idx}, slot.IsOccupied={slots[idx].IsOccupied}");

        // --- Burada en son saniye kollaması: eğer slot’a başkası yerleştiyse retarget et ---
        if (slots[idx].IsOccupied && slots[idx].Occupant != mv.gameObject)
        {
            int newIdx = FindInsertIndex(id);
            if (newIdx >= 0)
            {
                ChangeReservation(mv, newIdx);
                mv.BeginMove(newIdx);
                return;
            }
            else
            {
                // artık yer yok
                mv.gameObject.SetActive(false);
                movingItems.Remove(mv);
                reservedIndices.Remove(mv);
                return;
            }
        }

        // --- Normal varış akışı: önce ASSIGN, sonra rezervi kaldır ---
        slots[idx].AssignOccupant(mv.gameObject, id);
        slots[idx].SetReserved(false);

        movingItems.Remove(mv);
        reservedIndices.Remove(mv);
        mv.reservedSlotIndex = -1;

        StartCoroutine(ProcessSlotChanges());
    }




    private IEnumerator HandleDirectPlacement3D(GameObject item, string id, int idx)
    {
        // 1) Inspector’dan ayarlanan süreyi kullan
        yield return item.transform
                         .DOMove(slots[idx].transform.position, directMoveDuration)
                         .SetEase(moveEase)
                         .WaitForCompletion();

        // 2) Veriyi ata
        slots[idx].AssignOccupant(item, id);

        // 3) Match & compact
        yield return ProcessSlotChanges();
    }


    private IEnumerator HandlePlacementWithVisualShift3D(GameObject item, string id, int idx)
    {
        // Ön‑kaydırma
        yield return PerformVisualShift3D(idx, visualShiftDuration);

        bool shifted = ShiftRightFromData3D(idx);
        if (!shifted)
        {
            item.SetActive(false);
            item.transform.SetParent(null, true);
            yield break;
        }

        // Slot’a taşıma
        yield return item.transform
                         .DOMove(slots[idx].transform.position, directMoveDuration)
                         .SetEase(moveEase)
                         .WaitForCompletion();

        slots[idx].AssignOccupant(item, id);

        yield return ProcessSlotChanges();
    }

    public int FindInsertIndex(string id)
    {
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
            int next = lastBlock + 1;
            if (next < slots.Count && !slots[next].IsOccupied)
                return next;
            return firstBlock;
        }

        for (int i = 0; i < slots.Count; i++)
            if (!slots[i].IsOccupied && !slots[i].IsReserved)
                return i;

        return -1;
    }

    public bool ShiftRightFromData3D(int startIndex)
    {
        Debug.Log($"[ShiftData] Called with startIndex={startIndex}");
        if (startIndex < 0 || startIndex >= slots.Count)
        {
            Debug.LogError($"[ShiftData] Invalid startIndex {startIndex}");
            return false;
        }

        for (int i = slots.Count - 2; i >= startIndex; i--)
        {
            var src = slots[i];
            var dst = slots[i + 1];
            Debug.Log($"[ShiftData] i={i}: src.Occ={src.IsOccupied}/{src.StoredUniqueID} -> dstIdx={i + 1}");

            // rezerv flag
            bool srcRes = src.IsReserved;
            dst.SetReserved(srcRes);
            Debug.Log($"[ShiftData]   Reserved moved: dst.IsReserved now={dst.IsReserved}");

            // hareket edenler
            foreach (var mv in new List<ClickableObject>(reservedIndices.Keys))
            {
                if (reservedIndices[mv] == i)
                {
                    reservedIndices[mv] = i + 1;
                    mv.reservedSlotIndex = i + 1;
                    Debug.Log($"[ShiftData]   MovingItem {mv.UniqueID} reservedIdx updated to {i + 1}");
                }
            }

            if (src.IsOccupied)
            {
                var occ = src.Occupant;
                var oid = src.StoredUniqueID;
                Debug.Log($"[ShiftData]   Moving data {oid} from slot {i} to slot {i + 1}");
                src.ClearDataOnly();
                dst.ClearSlot();
                dst.AssignOccupant(occ, oid);
            }
            else
            {
                Debug.Log($"[ShiftData]   src empty -> clearing dst slot");
                dst.ClearSlot();
            }
        }

        // başlangıç temizleme
        slots[startIndex].SetReserved(false);
        Debug.Log($"[ShiftData]   Cleared reserve at startIndex {startIndex}");
        foreach (var mv in new List<ClickableObject>(reservedIndices.Keys))
        {
            if (reservedIndices[mv] == startIndex)
            {
                reservedIndices.Remove(mv);
                mv.reservedSlotIndex = -1;
                Debug.Log($"[ShiftData]   Removed movingItem {mv.UniqueID} from reservedIndices");
            }
        }

        slots[startIndex].ClearSlot();
        Debug.Log($"[ShiftData]   Cleared slot {startIndex} data-only");
        return true;
    }

    // (geri kalan FindInsertIndex, PerformVisualShift3D, CompactSlots3D, 
    //  CheckForMatchesCoroutine3D, AnimateMatchClearance3D, ProcessSlotChanges 
    //   metotların aynısını tutacağız)

    private IEnumerator PerformVisualShift3D(int startIndex, float duration)
    {
        for (int i = startIndex; i < slots.Count - 1; i++)
        {
            if (slots[i].IsOccupied)
            {
                slots[i].Occupant
                    .transform
                    .DOMove(slots[i + 1].transform.position, duration)
                    .SetEase(shiftEase);
            }
        }
        yield return new WaitForSeconds(duration);
    }

    private void CompactSlots3D()
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
                    yield return AnimateMatchClearance3D(i - count, count);
                    yield break;
                }
                count = 1;
            }
        }
        if (count >= 3)
            yield return AnimateMatchClearance3D(slots.Count - count, count);
    }

    /// <summary>
    /// Eşleşen öğeleri ortada birleştirip, sonra sadece bu üçlü için Destroy yapar.
    /// Diğer hiçbir noktada objeyi yok etmiyoruz.
    /// </summary>
    private IEnumerator AnimateMatchClearance3D(int startIndex, int count)
    {
        var toDestroy = new List<GameObject>();
        Vector3 center = slots[startIndex].transform.position;

        for (int i = startIndex; i < startIndex + count; i++)
        {
            var slot = slots[i];
            if (slot.IsOccupied && slot.Occupant != null)
            {
                var obj = slot.Occupant;
                slot.ClearDataOnly();
                obj.transform.SetParent(null, true);
                toDestroy.Add(obj);
            }
        }

        // Tween sürelerini ve easing tipini inspector’dan al
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
            Destroy(obj); // Sadece match sonrası
        }
    }

    private IEnumerator ProcessSlotChanges()
    {
        yield return StartCoroutine(CheckForMatchesCoroutine3D());
        CompactSlots3D();
        yield return StartCoroutine(CheckForMatchesCoroutine3D());
    }

}
