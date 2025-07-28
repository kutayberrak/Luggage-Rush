using UnityEngine;
using System.Collections.Generic;

public class ObjectPoolManager : MonoBehaviour
{
    [System.Serializable]
    public class PooledPrefabEntry
    {
        public GameObject prefab;
        public List<GameObject> objects = new();
    }

    [Header("Luggage Prefabs")]
    [SerializeField] private List<GameObject> luggagePrefabs = new();

    [Header("Garbage Prefabs")]
    [SerializeField] private List<GameObject> garbagePrefabs = new();

    [Header("Collection Prefabs")]
    [SerializeField] private List<GameObject> collectionPrefabs = new();

    public static ObjectPoolManager Instance { get; private set; }

    [SerializeField] private int poolSize = 10;

    private List<PooledPrefabEntry> pooledPrefabs = new();
    private Transform poolParent;
    private Transform lugageParent;
    private Transform garbageParent;
    private Transform collecitonParent;

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

        GameObject luggageParenObj = new GameObject("Lugages");
        lugageParent = luggageParenObj.transform;
        lugageParent.SetParent(poolParent);

        GameObject garbageParentObj = new GameObject("Garbages");
        garbageParent = garbageParentObj.transform;
        garbageParent.SetParent(poolParent);

        GameObject collectionParenObj = new GameObject("Collections");
        collecitonParent = collectionParenObj.transform;
        collecitonParent.SetParent(poolParent);

        InitializePool();
    }

    private void InitializePool()
    {
        AddToPool(luggagePrefabs, lugageParent);
        AddToPool(garbagePrefabs, garbageParent);
        AddToPool(collectionPrefabs, collecitonParent);
    }

    private void AddToPool(List<GameObject> prefabs, Transform parent)
    {
        foreach (GameObject prefab in prefabs)
        {
            if (prefab == null || pooledPrefabs.Exists(p => p.prefab == prefab))
                continue;

            PooledPrefabEntry entry = new PooledPrefabEntry();
            entry.prefab = prefab;

            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(prefab, parent);
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
        newObj.transform.SetParent(GetParentTransformFor(prefab));
        entry.objects.Add(newObj);
        return newObj;
    }
    private Transform GetParentTransformFor(GameObject prefab)
    {
        if (luggagePrefabs.Contains(prefab))
            return lugageParent;

        if (garbagePrefabs.Contains(prefab))
            return garbageParent;

        if (collectionPrefabs.Contains(prefab))
            return collecitonParent;

        return poolParent; // fallback (kategori dýþý)
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

    public List<GameObject> GetObjectsByType(ObjectType type)
    {
        switch (type)
        {
            case ObjectType.Luggage:
                return luggagePrefabs;
            case ObjectType.Garbage:
                return garbagePrefabs;
            case ObjectType.Collection:
                return collectionPrefabs;
            default:
                return new List<GameObject>();
        }
    }
    public void ReturnObjectToPool(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.useGravity = true;
        }
        foreach (var entry in pooledPrefabs)
        {
            if (entry.objects.Contains(obj))
            {
                obj.transform.localScale = entry.prefab.transform.localScale;
                break;
            }
        }

        obj.SetActive(false);
    }
}
