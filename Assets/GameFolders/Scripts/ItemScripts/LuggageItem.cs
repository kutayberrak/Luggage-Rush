using GameFolders.Scripts.ScriptableObjects;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using GameFolders.Scripts;
using GameFolders.Scripts.Enums;
using static UnityEngine.ParticleSystem;
using GameFolders.Scripts.Managers;
using Unity.VisualScripting;

public class LuggageItem : ClickableObject, ISlottable
{
    [Header("Auto Return Settings")]
    private float maxLifeTime = 20f;
    private bool useColliderReturn = true;

    private bool _isSlotted;

    private CancellationTokenSource _cancellationTokenSource;

    [SerializeField] float changeCooldown = 0.2f;
    float _nextAllowedChangeTime;

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
            // Debug.Log("Timer was cancelled, normal operation.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (useColliderReturn && other.CompareTag("DestroyZone")) // destroy zone, return to pool
        {
            ReturnToPool();
        }


        if (other.CompareTag("TypeChangeZone") && Time.time >= _nextAllowedChangeTime)
        {
            var newType = ChooseRandomType();
            if (newType != luggageType && newType != LuggageType.None)
            {
                ChangeType(newType);
                _nextAllowedChangeTime = Time.time + changeCooldown;
            }
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

    public void NotifySlotted()
    {
        _isSlotted = true;
        CancelTimer();
    }

    public void NotifyUnslotted()
    {
        _isSlotted = false;
        // Restart timer when leaving slot
        StartMaxLifeTimeTimer().Forget();
    }

    private void ChangeType(LuggageType newType)
    {
        Debug.Log("changetype");
        luggageType = newType;
        ApplyCollider(newType);
    }

    private void ApplyCollider(LuggageType type)
    {
        foreach (var data in LuggageColliderDatas.Instance.luggageColliderDatas)
        {
            if (data.type == type)
            {
                boxCollider.center = data.center;
                boxCollider.size = data.size;
                break;
            }
        }

        var prefabs = ObjectPoolManager.Instance.GetObjectsByType(ObjectType.Luggage);
        foreach (var prefab in prefabs)
        {
            var li = prefab.GetComponent<LuggageItem>();

            if (li != null && li.luggageType == type)
            {
                var prefabRenderer = prefab.GetComponentInChildren<MeshFilter>();
                var myRenderer = GetComponentInChildren<MeshFilter>();
                if (prefabRenderer != null && myRenderer != null)
                {
                    myRenderer.sharedMesh = prefabRenderer.sharedMesh;
                }
                break;
            }
        }
    }

    private LuggageType ChooseRandomType()
    {
        var types = GameManager.Instance.CurrentLevelData.LuggageTypesToSpawn;

        if (types == null || types.Count == 0)
            return luggageType;

        // Tek eleman varsa ve o da geçerli tip deðilse onu döndür, yoksa mevcut tip
        if (types.Count == 1 && (types[0] == luggageType || types[0] == LuggageType.None))
            return luggageType;

        LuggageType randomType;
        int safety = 0;
        do
        {
            randomType = types[Random.Range(0, types.Count)];
            safety++;
        }
        while ((randomType == luggageType || randomType == LuggageType.None) && safety < 10);


        Debug.Log("random type: " + randomType);
        return randomType;
    }

}
