using System.Collections.Generic;
using DG.Tweening;
using GameFolders.Scripts;
using GameFolders.Scripts.Managers;
using UnityEngine;

public class Hourglass : SpecialItem
{
    [SerializeField] private float timeToAdd = 5f;

    [SerializeField] private float moveDuration = 1f;

    public override void OnClickedByPlayer()
    {
                // **YENÝ**: Týklama cooldown kontrolü
        if (Time.time - lastClickTime < CLICK_COOLDOWN)
        {
            return;
        }
        lastClickTime = Time.time;

        // **YENÝ**: Týklama animasyonu sýrasýnda tekrar týklamayý engelle
        if (isInClickAnimation)
        {
            Debug.Log($"[ClickableObject] Click animation in progress for {UniqueID}");
            return;
        }

        gameObject.GetComponent<Collider>().isTrigger = true;

        PlayCollectAnimation();
    }

    public void PlayCollectAnimation()
    {
        if (isMoving) return;
        isMoving = true;

        Transform objTransform = transform;
        Vector3 startPos = objTransform.position + Vector3.up;
        Vector3 endPos = Timer.Instance.GetTextPosition();

        float distance = Vector3.Distance(startPos, endPos);
        float adjustedCurveHeight = Mathf.Clamp(2f * (distance / 5f), 0.5f, 2f);

        Vector3 midPoint = Vector3.Lerp(startPos, endPos, 0.5f);
        midPoint.y += adjustedCurveHeight;

        Vector3[] path = new Vector3[]
        {
            startPos,
            Vector3.Lerp(startPos, midPoint, 0.4f),
            midPoint,
            Vector3.Lerp(midPoint, endPos, 0.6f),
            endPos
        };

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOPath(path, moveDuration, PathType.CatmullRom)
            .SetEase(Ease.InOutSine));

        seq.Insert(0.5f, transform.DOScale(Vector3.zero, 0.5f)
            .SetEase(Ease.InQuad));

        seq.OnComplete(() =>
        {
            Timer.Instance.AddTime(5f);
            Timer.Instance.FlashTimerColor(Color.green);
            ObjectPoolManager.Instance.ReturnObjectToPool(gameObject);
            isMoving = false;
        });
    }
}
