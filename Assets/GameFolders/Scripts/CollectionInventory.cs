using DG.Tweening;
using GameFolders.Scripts.Enums;
using GameFolders.Scripts.Managers;
using GameFolders.Scripts.ScriptableObjects;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class CollectionInventory : MonoBehaviour
{
    public static CollectionInventory Instance { get; private set; }

    // Ana veri: LevelDataSO -> O levelde toplanan collectible tipleri
    private Dictionary<LevelDataSO, HashSet<CollectiblePieceType>> collectedByLevel
        = new Dictionary<LevelDataSO, HashSet<CollectiblePieceType>>();

    private LevelDataSO currentLevelData;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public bool TryCollect(GameObject collection)
    {
        // Diðer Collection iþlemleri
        /*var set = collectedByLevel[currentLevelData];

        if (set.Contains(type))
        {
            Debug.Log($"[CollectionInventory] Already collected {type} in level {currentLevelData.name}");
            return false;
        }

        set.Add(type);
        Debug.Log($"[CollectionInventory] Collected {type} in level {currentLevelData.name}");
        return true;*/




        CollectionItem collectionItem = collection.GetComponent<CollectionItem>();
        collectionItem.StartCurve();
        return true;
    }
    

    public int CollectedCount => collectedByLevel[currentLevelData].Count;

    public IReadOnlyCollection<CollectiblePieceType> GetCollectedTypes()
    {
        return collectedByLevel.TryGetValue(currentLevelData, out var set) ? set : new List<CollectiblePieceType>();
    }

    public void ResetCurrentLevel()
    {
        if (collectedByLevel.ContainsKey(currentLevelData))
            collectedByLevel[currentLevelData].Clear();
    }
}
