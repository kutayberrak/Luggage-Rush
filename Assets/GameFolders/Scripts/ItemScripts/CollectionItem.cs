using GameFolders.Scripts.ScriptableObjects;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using GameFolders.Scripts.Enums;
using DG.Tweening;
using GameFolders.Scripts;

public class CollectionItem : ClickableObject
{
    [Header("Auto Return Settings")]
    private float maxLifeTime = 20f;
    private bool useColliderReturn = true;

    private Sequence curveMovementSequence;

    private CancellationTokenSource _cancellationTokenSource;
    Vector3 endPos;
    protected override void StartClickAnimation()
    {
        if (isMoving) return;

        isMoving = true;

        // Orijinal pozisyon ve rotasyonu kaydet
        originalPosition = transform.position;
        originalRotation = transform.eulerAngles;

        // Y�kselme animasyonu
        Vector3 risePosition = originalPosition + Vector3.up * riseHeight;

        // Pozisyon ve rotasyon animasyonunu ayn� anda ba�lat
        Sequence clickSequence = DOTween.Sequence();
        HandleRigidBody();
        CollectionInventory.Instance.TryCollect(this.gameObject);

        clickSequence.Append(transform.DOMove(risePosition, riseDuration).SetEase(riseEase));
        clickSequence.Join(transform.DORotate(Vector3.zero, riseDuration).SetEase(riseEase));

        // Animasyon tamamland���nda slot hareketini ba�lat
        clickSequence.OnComplete(() => {
            isMoving = false;
        });

        Debug.Log($"[ClickableObject] Started click animation for {UniqueID}");
    }
    public void StartCurve(Vector3 endPos)
    {
        this.endPos  = endPos;
        StartCurveMovement();
    }

    protected override void StartCurveMovement()
    {
        if (isInCurveMovement) return;

        isInCurveMovement = true;

        Transform objTransform = transform.transform;
        Vector3 startPos = objTransform.position;

        float distance = Vector3.Distance(startPos, endPos);
        float adjustedCurveHeight = Mathf.Clamp(2f * (distance / 5f), 0.5f, 2f);

        Vector3 midPoint = Vector3.Lerp(startPos, endPos, 0.5f);
        midPoint.y += adjustedCurveHeight;

        Vector3[] path = new Vector3[] {
            startPos,
            Vector3.Lerp(startPos, midPoint, 0.4f), // Ba�lang�� e�risi
            midPoint,
            Vector3.Lerp(midPoint, endPos, 0.6f),   // Biti� e�risi
            endPos
        };

        curveMovementSequence = DOTween.Sequence();

        curveMovementSequence.Append(transform.DOPath(path, 1f, PathType.CatmullRom)
            .SetEase(Ease.InOutSine));
        curveMovementSequence.OnComplete(() => {
            isInCurveMovement = false;
            ObjectPoolManager.Instance.ReturnObjectToPool(gameObject);
            if (InGameUIManager.Instance != null)
            {
                InGameUIManager.Instance.OnCollectionCollected(collectionType);
            }
        });
    }
    private void OnEnable()
    {
        StartMaxLifeTimeTimer().Forget();

        // Subscribe to level win event
        GameEvents.OnLevelWin += ReturnToPool;
        GameEvents.OnLevelFailed += ReturnToPool;
    }

    private void OnDisable()
    {
        CancelTimer();

        // Unsubscribe from level win event
        GameEvents.OnLevelWin -= ReturnToPool;
        GameEvents.OnLevelFailed -= ReturnToPool;
    }

    private async UniTaskVoid StartMaxLifeTimeTimer()
    {
        CancelTimer();

        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            await UniTask.Delay((int)(maxLifeTime * 1000), cancellationToken: _cancellationTokenSource.Token);

            if (gameObject.activeInHierarchy)
            {
                ReturnToPool();
            }
        }
        catch (System.OperationCanceledException)
        {
            // Timer iptal edildi
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (useColliderReturn && other.CompareTag("DestroyZone"))
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        CancelTimer();
        ObjectPoolManager.Instance.ReturnObjectToPool(gameObject);
    }

    private void CancelTimer()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }

    private void OnDestroy()
    {
        CancelTimer();
    }
}
