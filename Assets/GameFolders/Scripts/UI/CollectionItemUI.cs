using UnityEngine;
using UnityEngine.UI;
using GameFolders.Scripts.Enums;
using TMPro;

public class CollectionItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image collectionImage;
    
    public CollectionData collectionData;
    public CollectiblePieceType CollectionType => collectionData?.CollectionType ?? CollectiblePieceType.None;

    /// <summary>
    /// Initialize the collection item with data
    /// </summary>
    public void Initialize(CollectionData collection)
    {
        collectionData = collection;
        UpdateVisual();
    }

    /// <summary>
    /// Update the visual state of the collection item
    /// </summary>
    public void UpdateVisual()
    {
        if (collectionData == null) return;

        // Update image
        if (collectionImage != null)
        {
            collectionImage.sprite = collectionData.CollectionImage;
        }

        collectionImage.color = collectionData.IsUnlocked ? Color.white : Color.black; 

    }
    
    /// <summary>
    /// Get collection data
    /// </summary>
    public CollectionData GetCollectionData()
    {
        return collectionData;
    }
    
    /// <summary>
    /// Check if collection is unlocked
    /// </summary>
    public bool IsUnlocked()
    {
        return collectionData?.IsUnlocked ?? false;
    }

} 