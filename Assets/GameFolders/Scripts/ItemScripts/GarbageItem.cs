using GameFolders.Scripts.ScriptableObjects;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using GameFolders.Scripts.Enums;

public class GarbageItem : ClickableObject
{
    [Header("Auto Return Settings")]
    private float maxLifeTime = 20f;
    private bool useColliderReturn = true;

    private CancellationTokenSource _cancellationTokenSource;

    private void OnEnable()
    {
        StartMaxLifeTimeTimer().Forget();
    }

    private void OnDisable()
    {
        CancelTimer();
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
