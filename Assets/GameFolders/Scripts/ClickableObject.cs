using UnityEngine;
using DG.Tweening;

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

    private void OnMouseDown()
    {
        if (isMoving) return;
        Slot targetSlot = SlotManager.Instance.GetFirstEmptySlot();
        if (targetSlot != null)
        {
            isMoving = true;
            reservedSlot = targetSlot;
            reservedSlot.SetOccupied(true); // Slotu hemen occupied yap

            // Hem scale hem rotasyon animasyonu başlat
            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOScale(clickScale, scaleDuration).SetEase(Ease.InCubic));
            seq.Join(transform.DORotate(Vector3.zero, rotateDuration).SetEase(Ease.InCubic));
            seq.AppendInterval(waitAfterScale); // Büyüme sonrası bekleme
            seq.OnComplete(() =>
            {
                MoveToSlotWithCurve(reservedSlot);
            });
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
            float zDistance = Vector3.Distance(Camera.main.transform.position, transform.position);
            worldTargetPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDistance));
        }
        else
        {
            worldTargetPos = targetSlot.transform.position;
        }

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
            targetSlot.FillSlotWithSprite(objectSprite);
            Destroy(gameObject);
        });
    }
}