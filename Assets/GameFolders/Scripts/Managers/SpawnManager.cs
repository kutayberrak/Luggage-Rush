using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using GameFolders.Scripts.Enums;
using GameFolders.Scripts.ScriptableObjects;
using GameFolders.Scripts.Managers;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private bool autoSpawn = true;

    [Header("Runtime Controls")]
    [SerializeField] private float newSpawnInterval = 2f;

    private List<LuggageType> allowedTypes = new List<LuggageType>(); // Initialize et
    private CancellationTokenSource _cancellationTokenSource;
    private int _currentSpawnIndex = 0;

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
    }

    // void Start()
    // {
    //     // Allowed types'ı LevelManager'dan al


    //     // if (autoSpawn)
    //     // {
    //     //     StartSpawning().Forget();
    //     // }
    // }



    // void Update()
    // {

    //     if (Input.GetKeyDown(KeyCode.Space)) // Örnek olarak Space tuşuna basıldığında spawn yap
    //     {
    //         if (autoSpawn)
    //         {
    //             StartSpawning().Forget();
    //         }
    //     }
    // }

    void OnDestroy()
    {
        StopSpawning();
    }

    public void LoadLevelSpawnRequirements()//first
    {
        if (LevelManager.Instance != null && LevelManager.Instance.CurrentLevelData != null)
        {
            allowedTypes.Clear();
            allowedTypes = LevelManager.Instance.CurrentLevelData.LuggageTypes;
        }
    }
    public void RunSpawn() //second
    {
        if (autoSpawn)
        {
            StartSpawning().Forget();
        }
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
        catch (System.OperationCanceledException)
        {

            // task canceled, normal situation
            //Debug.Log("Spawning stopped.");
        }
    }

    public void StopSpawning()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }

    private void SpawnObject()
    {
        if (spawnPoints.Length > 0)
        {

            Transform spawnPoint = spawnPoints[_currentSpawnIndex];


            GameObject prefabToSpawn = GetRandomAllowedPrefab();

            if (prefabToSpawn != null)
            {

                GameObject block = ObjectPoolManager.Instance.GetObjectFromPool(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
            }

            _currentSpawnIndex = (_currentSpawnIndex + 1) % spawnPoints.Length;
        }
    }

    private GameObject GetRandomAllowedPrefab()
    {
        List<GameObject> availablePrefabs = new List<GameObject>();
        List<GameObject> allPrefabs = ObjectPoolManager.Instance.GetAllPrefabs();

        if (allowedTypes != null && allowedTypes.Count > 0)
        {
            foreach (GameObject prefab in allPrefabs)
            {
                LuggageInfo luggageInfo = prefab.GetComponent<LuggageInfo>();
                if (luggageInfo != null && allowedTypes.Contains(luggageInfo.luggageType))
                {
                    availablePrefabs.Add(prefab);
                }
            }
        }
        else
        {
            Debug.LogWarning("Allowed types list is empty or not set. Using all prefabs.");
            availablePrefabs = allPrefabs;
        }

        if (availablePrefabs.Count == 0)
        {
            Debug.LogWarning("No allowed prefabs found for spawning.");
            return null;
        }

        return availablePrefabs[Random.Range(0, availablePrefabs.Count)];
    }


    public void ManualSpawn()
    {
        SpawnObject();
    }


    public void SetAutoSpawn(bool enabled)
    {
        if (enabled)
        {
            StartSpawning().Forget();
        }
        else
        {
            StopSpawning();
        }
    }


    [Button("Set New Spawn Interval")]
    public void SetSpawnInterval()
    {
        SetSpawnInterval(newSpawnInterval);
    }

    public void SetSpawnInterval(float interval)
    {
        spawnInterval = interval;

        // if spawning is active, restart it
        if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
        {
            StopSpawning();
            StartSpawning().Forget();
        }
    }
}
