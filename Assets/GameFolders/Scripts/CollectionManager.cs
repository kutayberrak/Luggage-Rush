using System.Collections.Generic;
using System.Linq;
using GameFolders.Scripts.Enums;
using UnityEngine;

public class CollectionManager : MonoBehaviour
{
    [Header("Collection Settings")]
    [SerializeField] private List<CollectionData> allCollections = new List<CollectionData>();

    public List<CollectionPanel> collectionPanels;

    // Singleton pattern
    public static CollectionManager Instance { get; private set; }

    // Properties
    public List<CollectionData> AllCollections => allCollections;
    public List<CollectionData> UnlockedCollections => allCollections.Where(c => c.IsUnlocked).ToList();
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCollections();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Load all collections from Resources or ScriptableObjects
    /// </summary>
    private void LoadCollections()
    {
        if (allCollections.Count == 0)
        {
            // Resources klasöründen yükleme örneği
            CollectionData[] loadedCollections = Resources.LoadAll<CollectionData>("Collections");
            allCollections.AddRange(loadedCollections);
            
            // Load saved progress from PlayerPrefs
            LoadCollectionProgress();
        }
    }
    
    /// <summary>
    /// Add collection to the manager
    /// </summary>
    public void AddCollection(CollectionData collection)
    {
        if (!allCollections.Contains(collection))
        {
            allCollections.Add(collection);
        }
    }
    
    /// <summary>
    /// Remove collection from the manager
    /// </summary>
    public void RemoveCollection(CollectionData collection)
    {
        if (allCollections.Contains(collection))
        {
            allCollections.Remove(collection);
        }
    }
    
    /// <summary>
    /// Get collection by CollectiblePieceType
    /// </summary>
    public CollectionData GetCollectionByType(CollectiblePieceType type)
    {
        return allCollections.FirstOrDefault(c => c.CollectionType == type);
    }
    
    /// <summary>
    /// Check if collection is unlocked
    /// </summary>
    public bool IsCollectionUnlocked(CollectiblePieceType type)
    {
        CollectionData collection = GetCollectionByType(type);
        return collection != null && collection.IsUnlocked;
    }
    
    /// <summary>
    /// Unlock collection by CollectiblePieceType
    /// </summary>
    public void UnlockCollection(CollectiblePieceType type)
    {
        CollectionData collection = GetCollectionByType(type);
        if (collection != null && !collection.IsUnlocked)
        {
            collection.UnlockCollection();
            // Save progress
            SaveCollectionProgress();
            
            // Update UI if collection panel exists
            UpdateCollectionPanel(type);
        }
    }
    
    /// <summary>
    /// Unlock collection by CollectionData
    /// </summary>
    public void UnlockCollection(CollectionData collection)
    {
        if (collection != null && !collection.IsUnlocked)
        {
            collection.UnlockCollection();
            // Save progress
            SaveCollectionProgress();
            
            // Update UI if collection panel exists
            UpdateCollectionPanel(collection.CollectionType);
        }
    }
    
    /// <summary>
    /// Unlock multiple collections
    /// </summary>
    public void UnlockCollections(List<CollectiblePieceType> types)
    {
        foreach (var type in types)
        {
            UnlockCollection(type);
        }
    }
    
    /// <summary>
    /// Reset all collections (for testing or new game)
    /// </summary>
    public void ResetAllCollections()
    {
        foreach (var collection in allCollections)
        {
            // Reset method'u CollectionData'ya eklenebilir
            // collection.ResetCollection();
        }
        
        SaveCollectionProgress();
    }
    
    /// <summary>
    /// Get collection progress percentage
    /// </summary>
    public float GetCollectionProgress()
    {
        if (allCollections.Count == 0) return 0f;
        return (float)UnlockedCollections.Count / allCollections.Count * 100f;
    }
    
    /// <summary>
    /// Save collection progress to PlayerPrefs
    /// </summary>
    private void SaveCollectionProgress()
    {
        foreach (var collection in allCollections)
        {
            string key = $"Collection_{collection.CollectionType}";
            PlayerPrefs.SetInt(key, collection.IsUnlocked ? 1 : 0);
        }
        PlayerPrefs.Save();
        
        Debug.Log($"[CollectionManager] Saved collection progress. Unlocked: {UnlockedCollections.Count}/{allCollections.Count}");
    }
    
    /// <summary>
    /// Load collection progress from PlayerPrefs
    /// </summary>
    private void LoadCollectionProgress()
    {
        foreach (var collection in allCollections)
        {
            string key = $"Collection_{collection.CollectionType}";
            bool isUnlocked = PlayerPrefs.GetInt(key, 0) == 1;
            
            if (isUnlocked && !collection.IsUnlocked)
            {
                collection.UnlockCollection();
            }
        }
        
        Debug.Log($"[CollectionManager] Loaded collection progress. Unlocked: {UnlockedCollections.Count}/{allCollections.Count}");
    }
    
    /// <summary>
    /// Called when level is completed (subscribe to your game's win event)
    /// </summary>
    public void OnLevelCompleted(CollectiblePieceType collectedType)
    {
        UnlockCollection(collectedType);
    }
    
    /// <summary>
    /// Called when level is completed with multiple collection types
    /// </summary>
    public void OnLevelCompleted(List<CollectiblePieceType> collectedTypes)
    {
        UnlockCollections(collectedTypes);
    }
    
    /// <summary>
    /// Update collection panel UI
    /// </summary>
    public void UpdateCollectionPanel(CollectiblePieceType type)
    {
        foreach (var collectionPanel in collectionPanels)
        {
            collectionPanel.UpdateCollection(type);
        }
    }
    
    /// <summary>
    /// Update all collection panels
    /// </summary>
    public void UpdateAllCollectionPanels()
    {
        foreach (var collectionPanel in collectionPanels)
        {
            collectionPanel.UpdateAllCollections();
        }
    }
    
    // Debug methods
    [ContextMenu("Print All Collections")]
    private void PrintAllCollections()
    {
        Debug.Log("=== ALL COLLECTIONS ===");
        foreach (var collection in allCollections)
        {
            Debug.Log($"- {collection.CollectionName} ({collection.CollectionType}): {(collection.IsUnlocked ? "UNLOCKED" : "LOCKED")}");
        }
    }
    
    [ContextMenu("Print Unlocked Collections")]
    private void PrintUnlockedCollections()
    {
        Debug.Log("=== UNLOCKED COLLECTIONS ===");
        foreach (var collection in UnlockedCollections)
        {
            Debug.Log($"- {collection.CollectionName} ({collection.CollectionType})");
        }
    }
} 