using UnityEngine;
using System.Collections.Generic;
using GameFolders.Scripts.ScriptableObjects;
using GameFolders.Scripts.Managers;

public class ObjectPoolManager : MonoBehaviour
{
    [System.Serializable]
    public class PooledPrefabEntry
    {
        public GameObject prefab;
        public List<GameObject> objects = new();
    }

    [Header("Prefabs to Pool")]
    [SerializeField] private List<GameObject> prefabsToPool = new();

    public static ObjectPoolManager Instance { get; private set; }

    [SerializeField] private int poolSize = 10;

    private List<PooledPrefabEntry> pooledPrefabs = new();
    private Transform poolParent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        GameObject poolParentObj = new GameObject("PooledObjects");
        poolParent = poolParentObj.transform;

        InitializePool();
    }

    private void InitializePool()
    {
        //List<LuggageType> allowedTypes = LevelManager.Instance.CurrentLevelData.LuggageTypes;

        foreach (GameObject prefab in prefabsToPool)
        {
            if (prefab == null || pooledPrefabs.Exists(p => p.prefab == prefab))
                continue;

            // LuggageInfo luggageInfo = prefab.GetComponent<LuggageInfo>(); // Bavullarda olan script

            // if (luggageInfo == null || !allowedTypes.Contains(luggageInfo.luggageType)) // Bavulda olan type 
            //     continue;

            PooledPrefabEntry entry = new PooledPrefabEntry();
            entry.prefab = prefab;

            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(prefab, poolParent);
                obj.SetActive(false);
                entry.objects.Add(obj);
            }

            pooledPrefabs.Add(entry);
        }
    }

    public GameObject GetObjectFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {

        var entry = pooledPrefabs.Find(p => p.prefab == prefab);

        if (entry == null)
        {
            Debug.LogError($"Prefab {prefab.name} not found in pool.");
            return null;
        }

        foreach (var obj in entry.objects)
        {
            if (!obj.activeInHierarchy)
            {
                obj.transform.SetPositionAndRotation(position, rotation);
                obj.SetActive(true);
                return obj;
            }
        }


        GameObject newObj = Instantiate(prefab, position, rotation);
        newObj.transform.SetParent(poolParent);
        entry.objects.Add(newObj);
        return newObj;
    }


    public List<GameObject> GetAllPrefabs()
    {
        List<GameObject> allPrefabs = new List<GameObject>();
        foreach (var entry in pooledPrefabs)
        {
            allPrefabs.Add(entry.prefab);
        }
        return allPrefabs;
    }

    // public GameObject GetRandomObjectFromPool(Vector3 position, Quaternion rotation)
    // {
    //     if (pooledPrefabs.Count == 0)
    //     {
    //         Debug.LogError("No prefabs initialized in pool.");
    //         return null;
    //     }

    //     int startIndex = Random.Range(0, pooledPrefabs.Count);

    //     for (int i = 0; i < pooledPrefabs.Count; i++)
    //     {
    //         int index = (startIndex + i) % pooledPrefabs.Count;
    //         var entry = pooledPrefabs[index];

    //         foreach (var obj in entry.objects)
    //         {
    //             if (!obj.activeInHierarchy)
    //             {
    //                 obj.transform.SetPositionAndRotation(position, rotation);
    //                 obj.SetActive(true);
    //                 return obj;
    //             }
    //         }
    //     }

    //     int fallbackIndex = Random.Range(0, pooledPrefabs.Count);
    //     var fallbackEntry = pooledPrefabs[fallbackIndex];

    //     GameObject newObj = Instantiate(fallbackEntry.prefab, position, rotation);
    //     newObj.transform.SetParent(poolParent);
    //     fallbackEntry.objects.Add(newObj);
    //     return newObj;
    // }

    public void ReturnObjectToPool(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        obj.SetActive(false);
        obj.transform.SetParent(poolParent);
    }
}
