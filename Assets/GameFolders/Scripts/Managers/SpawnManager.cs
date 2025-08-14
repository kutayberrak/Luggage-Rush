using Cysharp.Threading.Tasks;
using GameFolders.Scripts.Enums;
using GameFolders.Scripts.Managers;
using GameFolders.Scripts.ScriptableObjects;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    private List<Transform> _activeSpawnPoints = new List<Transform>();
    private int _currentActiveIndex = 0;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private bool autoSpawn = true;

    [Header("Spawn Weights")]
    [SerializeField]
    private SpawnWeightEntry[] spawnWeights = new SpawnWeightEntry[]
    {
        new SpawnWeightEntry { objectType = ObjectType.Luggage, spawnWeight = 5f },
        new SpawnWeightEntry { objectType = ObjectType.Garbage, spawnWeight = 2f },
        new SpawnWeightEntry { objectType = ObjectType.Collection, spawnWeight = 1f },
        new SpawnWeightEntry { objectType = ObjectType.Special, spawnWeight = 0.5f }
    };

    [Header("Runtime Controls")]
    [SerializeField] private float newSpawnInterval = 2f;

    private Dictionary<ObjectType, List<GameObject>> allowedObjectsByType = new Dictionary<ObjectType, List<GameObject>>();
    private CancellationTokenSource _cancellationTokenSource;
    private int _currentSpawnIndex = 0;

    private bool _hasCollectiblePiece;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        allowedObjectsByType[ObjectType.Luggage] = new List<GameObject>();
        allowedObjectsByType[ObjectType.Garbage] = new List<GameObject>();
        allowedObjectsByType[ObjectType.Collection] = new List<GameObject>();
        allowedObjectsByType[ObjectType.Special] = new List<GameObject>();
    }

    public void LoadLevelSpawnRequirements()
    {
        if (GameManager.Instance?.CurrentLevelData == null)
        {
            Debug.LogError("LevelManager.Instance or CurrentLevelData is null!");
            return;
        }

        var levelData = GameManager.Instance.CurrentLevelData;

        var luggagePrefabs = ObjectPoolManager.Instance.GetObjectsByType(ObjectType.Luggage);
        var garbagePrefabs = ObjectPoolManager.Instance.GetObjectsByType(ObjectType.Garbage);
        var collectionPrefabs = ObjectPoolManager.Instance.GetObjectsByType(ObjectType.Collection);
        var specialPrefabs = ObjectPoolManager.Instance.GetObjectsByType(ObjectType.Special);

        _hasCollectiblePiece = levelData.HasCollectiblePiece;

        // Load spawn interval from level data
        spawnInterval = levelData.SpawnInterval;

        //load spawn weights from level data
        LoadSpawnWeightsFromLevelData(levelData);

        // Clear existing allowed objects
        foreach (var kvp in allowedObjectsByType)
        {
            kvp.Value.Clear();
        }

        foreach (var luggageType in levelData.LuggageTypesToSpawn)
        {

            foreach (GameObject prefab in luggagePrefabs)
            {
                var luggageInfo = prefab.GetComponent<LuggageItem>();
                if (luggageInfo != null && luggageInfo.luggageType == luggageType)
                {
                    allowedObjectsByType[ObjectType.Luggage].Add(prefab);

                    break;
                }
            }
        }


        foreach (var garbageType in levelData.JunkPieceTypes)
        {

            foreach (GameObject prefab in garbagePrefabs)
            {
                var garbageInfo = prefab.GetComponent<GarbageItem>();
                if (garbageInfo != null && garbageInfo.garbageType == garbageType)
                {
                    allowedObjectsByType[ObjectType.Garbage].Add(prefab);

                    break;
                }
            }
        }


        foreach (var collectionType in levelData.CollectablePieceType)
        {

            foreach (GameObject prefab in collectionPrefabs)
            {
                var collectionInfo = prefab.GetComponent<CollectionItem>();
                if (collectionInfo != null && collectionInfo.collectionType == collectionType)
                {
                    allowedObjectsByType[ObjectType.Collection].Add(prefab);

                    break;
                }
            }
        }

        foreach (var specialType in levelData.SpecialTypesToSpawn)
        {
            foreach (GameObject prefab in specialPrefabs)
            {
                var specialItem = prefab.GetComponent<SpecialItem>();
                if (specialItem != null && specialItem.specialType == specialType)
                {
                    allowedObjectsByType[ObjectType.Special].Add(prefab);

                    break;
                }
            }
        }


        if (allowedObjectsByType.TryGetValue(ObjectType.Collection, out var collectionList))
        {
            _hasCollectiblePiece = collectionList.Count > 0;
        }
        else
        {
            _hasCollectiblePiece = false;
        }
    }

    private void LoadSpawnWeightsFromLevelData(LevelDataSO levelData)
    {
        var weightData = levelData.SpawnWeightData;

        foreach (var entry in spawnWeights)
        {
            switch (entry.objectType)
            {
                case ObjectType.Luggage:
                    entry.spawnWeight = weightData.LuggageSpawnWeight;
                    break;
                case ObjectType.Garbage:
                    entry.spawnWeight = weightData.JunkSpawnWeight;
                    break;
                case ObjectType.Collection:
                    entry.spawnWeight = weightData.CollectableSpawnWeight;
                    break;
                case ObjectType.Special:
                    entry.spawnWeight = weightData.SpecialSpawnWeight;
                    break;
            }

            entry.currentWeight = 0f;
        }

        Debug.Log($"Updated spawn weights - Luggage: {weightData.LuggageSpawnWeight}, Garbage: {weightData.JunkSpawnWeight}, Collection: {weightData.CollectableSpawnWeight}");
    }

    public void RunSpawn()
    {
        if (autoSpawn)
        {
            StartSpawning().Forget();
        }
    }

    private void SpawnObject()
    {
        // Cache’lenmiþ aktif listeyi kullan
        if (_activeSpawnPoints.Count == 0) return;

        Transform spawnPoint = _activeSpawnPoints[_currentActiveIndex];
        _currentActiveIndex = (_currentActiveIndex + 1) % _activeSpawnPoints.Count;

        ObjectType selectedType = GetWeightedRandomObjectType();
        GameObject prefabToSpawn = GetRandomPrefabOfType(selectedType);
        if (prefabToSpawn != null)
        {
            ObjectPoolManager.Instance.GetObjectFromPool(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
        }
    }

    public void RefreshActiveSpawnPoints()
    {
        _activeSpawnPoints.Clear();
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            var t = spawnPoints[i];
            if (t != null && t.gameObject.activeInHierarchy)
                _activeSpawnPoints.Add(t);
        }

        _currentActiveIndex = 0;

        if (_activeSpawnPoints.Count == 0)
        {
            Debug.LogWarning("[SpawnManager] Aktif spawn point bulunamadý. Spawn durduruluyor.");
            StopSpawning();
        }
    }

    private ObjectType GetWeightedRandomObjectType()
    {

        foreach (var entry in spawnWeights)
        {

            if (entry.objectType == ObjectType.Collection && !_hasCollectiblePiece)
                continue;

            entry.currentWeight += entry.spawnWeight * Time.deltaTime;
        }


        SpawnWeightEntry selectedEntry = null;
        foreach (var entry in spawnWeights)
        {

            if (entry.objectType == ObjectType.Collection && !_hasCollectiblePiece)
                continue;

            if (selectedEntry == null || entry.currentWeight > selectedEntry.currentWeight)
            {
                selectedEntry = entry;
            }
        }


        //reset selected weight
        selectedEntry.currentWeight = 0f;
        return selectedEntry.objectType;
    }
    private GameObject GetRandomPrefabOfType(ObjectType objectType)
    {
        if (!allowedObjectsByType.ContainsKey(objectType)) return null;

        var prefabs = allowedObjectsByType[objectType];
        if (prefabs.Count == 0) return null;

        return prefabs[Random.Range(0, prefabs.Count)];
    }

    public async UniTaskVoid StartSpawning()
    {
        StopSpawning();
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                await UniTask.Delay((int)(spawnInterval * 1000), cancellationToken: _cancellationTokenSource.Token);
                SpawnObject();
            }
        }
        catch (System.OperationCanceledException) { }
    }

    public void StopSpawning()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }

    public void DisableCollectionType(CollectiblePieceType collectionType)
    {
        if (!allowedObjectsByType.TryGetValue(ObjectType.Collection, out var list) || list.Count == 0)
            return;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            var prefab = list[i];
            var collectionInfo = prefab.GetComponent<CollectionItem>();
            if (collectionInfo != null && collectionInfo.collectionType == collectionType)
            {
                list.RemoveAt(i);
            }
        }
        _hasCollectiblePiece = list.Count > 0;
    }

    void OnDestroy()
    {
        StopSpawning();
    }

    [Button("Manual Spawn")]
    public void ManualSpawn() => SpawnObject();

    [Button("Set New Spawn Interval")]
    public void SetSpawnInterval() => SetSpawnInterval(newSpawnInterval);

    public void SetSpawnInterval(float interval)
    {
        spawnInterval = interval;
        if (_cancellationTokenSource?.Token.IsCancellationRequested == false)
        {
            StopSpawning();
            StartSpawning().Forget();
        }
    }
}
