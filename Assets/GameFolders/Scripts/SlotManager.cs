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

    // --- bu ikisi eklendi ---
    // hareket halindeki nesneleri takip edeceğiz
    private Dictionary<ClickableObject, string> movingItems = new Dictionary<ClickableObject, string>();
    private Dictionary<ClickableObject, int> reservedIndices = new Dictionary<ClickableObject, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Objeyi slota yerleştirme akışını yöneten coroutine.
    /// </summary>
    private IEnumerator TryPlaceObjectRoutine(GameObject item, string id)
    {
        // 1) Hedef indeksi hesapla
        int idx = FindInsertIndex(id);
        if (idx < 0)
        {
            item.SetActive(false);
            yield break;
        }

        // 2) Eğer doluysa önce veriyi sağa kaydır
        if (slots[idx].IsOccupied)
        {
            // (isteğe bağlı: görsel shift animasyonu)
            // yield return StartCoroutine(PerformVisualShift3D(idx, visualShiftDuration));

            bool shifted = ShiftRightFromData3D(idx);
            if (!shifted)
            {
                item.SetActive(false);
                yield break;
            }
        }

        // 3) Slot'u rezerve et
        slots[idx].SetReserved(true);

        // 4) Hareket komponentini hazırla
        var mv = item.GetComponent<ClickableObject>()
                 ?? item.AddComponent<ClickableObject>();
        mv.speed = directMoveSpeed;
        movingItems[mv] = id;
        reservedIndices[mv] = idx;

        // 5) Hareketi başlat
        mv.BeginMove(idx);

        // Coroutine bu noktada biter; OnMovableArrived çağrısı ile kaldığı yerden devam eder.
    }

    /// <summary>
    /// Artık bu tek satırlık metot yerine coroutine başlatıyoruz.
    /// </summary>
    public void TryPlaceObject3D(GameObject item, string id)
    {
        StartCoroutine(TryPlaceObjectRoutine(item, id));
    }

    public void OnMovableArrived(ClickableObject mv)
    {
        if (!movingItems.ContainsKey(mv)) return;
        int idx = reservedIndices[mv];
        string id = movingItems[mv];

        // 1) Rezerveyi kaldır
        slots[idx].SetReserved(false);

        // 2) Slot’a ata
        slots[idx].AssignOccupant(mv.gameObject, id);

        // 3) Tracking’den temizle
        movingItems.Remove(mv);
        reservedIndices.Remove(mv);

        // 4) Eşleşme & sıkıştırma sürecini başlat
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

    private int FindInsertIndex(string id)
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
        if (startIndex < 0 || startIndex >= slots.Count) return false;

        for (int i = slots.Count - 2; i >= startIndex; i--)
        {
            var src = slots[i];
            var dst = slots[i + 1];

            // --- burada rezerv flag’lerini taşı ---
            bool srcRes = src.IsReserved;
            dst.SetReserved(srcRes);

            // eğer bu slotta hareket eden bir Movable varsa, onun reservedIndices’ini de güncelle
            foreach (var kv in new List<ClickableObject>(reservedIndices.Keys))
            {
                if (reservedIndices[kv] == i)
                    reservedIndices[kv] = i + 1;
            }

            if (src.IsOccupied)
            {
                // objeyi taşı
                var occ = src.Occupant;
                var oid = src.StoredUniqueID;
                src.ClearDataOnly();
                dst.ClearSlot();
                dst.AssignOccupant(occ, oid);
            }
            else
            {
                dst.ClearSlot();
            }
        }
        // başlangıç rezerv flag’ini temizle
        slots[startIndex].SetReserved(false);
        // bu pozisyondaki var ise taşıma mapping’ini sil
        foreach (var kv in new List<ClickableObject>(reservedIndices.Keys))
            if (reservedIndices[kv] == startIndex)
                reservedIndices.Remove(kv);

        slots[startIndex].ClearSlot();
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
