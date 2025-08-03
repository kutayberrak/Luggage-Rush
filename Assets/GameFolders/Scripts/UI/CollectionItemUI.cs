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
    /// OnEnable'da UI'� g�ncelle
    /// </summary>
    private void OnEnable()
    {
        // E�er collection data varsa g�ncelle
        if (collectionData != null)
        {
            UpdateVisual();
        }
    }

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

        // CollectionData art�k PlayerPrefs'ten okuyacak
        bool isUnlocked = collectionData.IsUnlocked;

        // G�rsel durumu g�ncelle
        if (collectionImage != null)
        {
            collectionImage.color = isUnlocked ? Color.white : Color.black;
        }

        Debug.Log($"[CollectionItemUI] Updated visual for {collectionData.CollectionType} in {collectionData.CollectionCountry}: {(isUnlocked ? "UNLOCKED" : "LOCKED")}");
    }

    /// <summary>
    /// Get collection data
    /// </summary>
    public CollectionData GetCollectionData()
    {
        return collectionData;
    }

    /// <summary>
    /// Check if collection is unlocked (CollectionData'dan)
    /// </summary>
    public bool IsUnlocked()
    {
        return collectionData?.IsUnlocked ?? false;
    }

    /// <summary>
    /// Koleksiyonu a� (CollectionData �zerinden)
    /// </summary>
    public void UnlockCollection()
    {
        collectionData?.UnlockCollection();
        UpdateVisual();
    }

    /// <summary>
    /// Koleksiyonu kilitle (CollectionData �zerinden)
    /// </summary>
    public void LockCollection()
    {
        collectionData?.ResetCollection();
        UpdateVisual();
    }

    /// <summary>
    /// Koleksiyon durumunu de�i�tir (toggle)
    /// </summary>
    public void ToggleCollection()
    {
        if (IsUnlocked())
        {
            LockCollection();
        }
        else
        {
            UnlockCollection();
        }
    }
}