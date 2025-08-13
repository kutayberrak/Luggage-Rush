using Cysharp.Threading.Tasks;
using GameFolders.Scripts;
using System.Threading;
using UnityEngine;

public class SpecialItem : ClickableObject
{
    private float maxLifeTime = 20f;
    private CancellationTokenSource _cancellationTokenSource;

    private void OnEnable()
    {
        StartMaxLifeTimeTimer().Forget();

        // Subscribe to level win event
        GameEvents.OnLevelWin += ReturnToPool;
        GameEvents.OnLevelFailed += ReturnToPool;
        GameEvents.OnReturnToMainMenu += ReturnToPool;
    }

    private void OnDisable()
    {
        CancelTimer();

        // Unsubscribe from level win event
        GameEvents.OnLevelWin -= ReturnToPool;
        GameEvents.OnLevelFailed -= ReturnToPool;
        GameEvents.OnReturnToMainMenu -= ReturnToPool;
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
        if (other.CompareTag("DestroyZone"))
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
