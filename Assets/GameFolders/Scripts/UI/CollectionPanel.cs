using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameFolders.Scripts.Enums;
using TMPro;

public class CollectionPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform collectionContainer;
    [SerializeField] private GameObject collectionItemPrefab;
    
    private List<CollectionItemUI> collectionItems = new List<CollectionItemUI>();

    public TextMeshProUGUI collectionText;

    public string collectionCountry;

    public int collectionAmount;
    public int unlockedAmount;

    
    private void Start()
    {
        InitializePanel();
        UpdateAllCollections();
    }
    
    /// <summary>
    /// Initialize collection panel
    /// </summary>
    private void InitializePanel()
    {
        collectionAmount = 0;
        unlockedAmount = 0;
        // Clear existing items
        ClearCollectionItems();
        
        // Get all collections from CollectionManager
        var allCollections = CollectionManager.Instance.AllCollections;
        
        // Create UI items for each collection
        foreach (var collection in allCollections)
        {
            if (collection.CollectionCountry == collectionCountry)
            {
                CreateCollectionItem(collection);
                collectionAmount++;
            }
        }

        UpdateCollectionText();
        

        Debug.Log($"[CollectionPanel] Initialized with {collectionItems.Count} collection items");
    }

    private void UpdateCollectionText()
    {
        collectionText.text = collectionCountry + " " + CalculateUnlockedCollections() + "/" + collectionAmount;
    }
    
    /// <summary>
    /// Create a single collection item UI
    /// </summary>
    private void CreateCollectionItem(CollectionData collection)
    {
        if (collectionItemPrefab == null)
        {
            Debug.LogError("[CollectionPanel] Collection item prefab is not assigned!");
            return;
        }
        
        // Instantiate the prefab
        GameObject itemGO = Instantiate(collectionItemPrefab, collectionContainer);
        itemGO.transform.localPosition = collection.collectionInstantiatePosition;
        itemGO.transform.localRotation = collection.collectionInstantiateRotation;
        itemGO.transform.localScale = collection.collectionInstantiateScale;
        CollectionItemUI itemUI = itemGO.GetComponent<CollectionItemUI>();
        
        if (itemUI != null)
        {
            itemUI.Initialize(collection);
            collectionItems.Add(itemUI);
        }
        else
        {
            Debug.LogError("[CollectionPanel] CollectionItemUI component not found on prefab!");
        }
    }
    /// <summary>
    /// Update all collection items
    /// </summary>
    public void UpdateAllCollections()
    {
        foreach (var item in collectionItems)
        {
            item.UpdateVisual();
        }
    }

    public int CalculateUnlockedCollections()
    {
        unlockedAmount = 0;
        foreach (var item in collectionItems)
        {
            if (item.collectionData.CollectionCountry == collectionCountry && item.collectionData.IsUnlocked)
            {
                unlockedAmount++;
            }
        }

        return unlockedAmount;
    }

    /// <summary>
    /// Update specific collection item
    /// </summary>
    public void UpdateCollection(CollectiblePieceType collectionType)
    {
        var item = collectionItems.Find(x => x.CollectionType == collectionType);
        if (item != null)
        {
            item.UpdateVisual();
        }

        UpdateCollectionText();
    }
    
    /// <summary>
    /// Clear all collection items
    /// </summary>
    private void ClearCollectionItems()
    {
        foreach (var item in collectionItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        collectionItems.Clear();
    }
    
    /// <summary>
    /// Refresh the entire panel (reinitialize)
    /// </summary>
    public void RefreshPanel()
    {
        InitializePanel();
    }
    
    /// <summary>
    /// Get collection progress text
    /// </summary>
    public string GetProgressText()
    {
        var unlockedCount = CollectionManager.Instance.UnlockedCollections.Count;
        var totalCount = CollectionManager.Instance.AllCollections.Count;
        return $"{unlockedCount}/{totalCount}";
    }
    
    /// <summary>
    /// Get collection progress percentage
    /// </summary>
    public float GetProgressPercentage()
    {
        return CollectionManager.Instance.GetCollectionProgress();
    }
} 