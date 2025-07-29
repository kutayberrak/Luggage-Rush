using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using GameFolders.Scripts.ScriptableObjects;
using GameFolders.Scripts.Managers;
using GameFolders.Scripts;
using GameFolders.Scripts.Data;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private bool autoSpawn = true;

    [Header("Spawn Weights")]
    [SerializeField]
    private SpawnWeightEntry[] spawnWeights = new SpawnWeightEntry[]
    {
        new SpawnWeightEntry { objectType = ObjectType.Luggage, spawnWeight = 5f },
        new SpawnWeightEntry { objectType = ObjectType.Garbage, spawnWeight = 2f },
        new SpawnWeightEntry { objectType = ObjectType.Collection, spawnWeight = 1f }
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

        _hasCollectiblePiece = levelData.HasCollectiblePiece;

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
        if (spawnPoints.Length == 0) return;

        Transform spawnPoint = spawnPoints[_currentSpawnIndex];
        ObjectType selectedType = GetWeightedRandomObjectType();
        GameObject prefabToSpawn = GetRandomPrefabOfType(selectedType);

        if (prefabToSpawn != null)
        {
            ObjectPoolManager.Instance.GetObjectFromPool(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
        }

        _currentSpawnIndex = (_currentSpawnIndex + 1) % spawnPoints.Length;
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
