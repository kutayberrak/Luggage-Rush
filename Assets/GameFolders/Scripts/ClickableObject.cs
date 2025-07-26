using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class ClickableObject : MonoBehaviour
{
    public Sprite objectSprite;
    public string uniqueId;

    public float moveSpeed = 5f;
    public float curveHeight = 1.5f;
    public float stopDistance = 0.05f;

    public float clickScale = 1.2f;
    public float scaleDuration = 0.15f;
    public float rotateDuration = 0.25f;
    public float waitAfterScale = 0.15f;

    public Canvas mainCanvas;

    private bool isMoving = false;
    private Slot targetSlot = null;
    private bool isArriving = false;

    public float finalScale = 0.2f;
    public float shrinkStartDistance = 1.0f;

    void Update()
    {
        if (!isArriving || targetSlot == null) return;

        Vector3 targetPos = GetSlotWorldPosition(targetSlot);
        targetPos.z = transform.position.z;

        Vector3 direction = (targetPos - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        float distance = Vector3.Distance(transform.position, targetPos);

        // 📏 Yaklaştıkça küçült
        if (distance < shrinkStartDistance)
        {
            float t = 1f - (distance / shrinkStartDistance); // 0 → 1
            float scaleValue = Mathf.Lerp(1f, finalScale, t);
            transform.localScale = Vector3.one * scaleValue;
        }

        if (distance < stopDistance)
        {
            ArriveAtSlot();
        }
    }

    private void OnMouseDown()
    {
        if (isMoving) return;

        // Eşleşen slotları bul
        var matches = new List<Slot>();
        foreach (var slot in SlotManager.Instance.slots)
        {
            if (slot.IsOccupied && slot.StoredUniqueID == uniqueId)
            {
                matches.Add(slot);
            }
        }

        // Hedef slot belirle
        if (matches.Count > 0)
        {
            int insertIndex = SlotManager.Instance.slots.IndexOf(matches[0]);
            bool shifted = SlotManager.Instance.ShiftRightFrom(insertIndex);
            if (!shifted) return;
            targetSlot = SlotManager.Instance.slots[insertIndex];
        }
        else
        {
            targetSlot = SlotManager.Instance.GetFirstEmptySlot();
            if (targetSlot == null) return;
        }

        // Rezerve et
        targetSlot.SetReserved(true);
        isMoving = true;

        // Sadece scale/rotate efekti yapıyoruz burada
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(clickScale, scaleDuration).SetEase(Ease.InCubic));
        seq.Join(transform.DORotate(Vector3.zero, rotateDuration).SetEase(Ease.InCubic));
        seq.AppendInterval(waitAfterScale);
        seq.OnComplete(() =>
        {
            isArriving = true; // Hareket başlayacak
        });
    }

    private Vector3 GetSlotWorldPosition(Slot slot)
    {
        RectTransform rect = slot.GetComponent<RectTransform>();
        Camera cam = mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCanvas.worldCamera;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, rect.position);
        float z = Vector3.Distance(Camera.main.transform.position, transform.position);
        return Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, z));
    }

    private void ArriveAtSlot()
    {
        transform.position = GetSlotWorldPosition(targetSlot);

        targetSlot.FillSlotWithSprite(objectSprite);
        targetSlot.StoredUniqueID = uniqueId;
        targetSlot.SetOccupied(true);
        targetSlot.SetReserved(false);

        isMoving = false;
        isArriving = false;

        SlotManager.Instance.CheckForMatches(targetSlot);
        Destroy(gameObject);
    }
}