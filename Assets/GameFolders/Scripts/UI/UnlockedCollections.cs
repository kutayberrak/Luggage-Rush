using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using GameFolders.Scripts.Enums;
using GameFolders.Scripts.ScriptableObjects;
using GameFolders.Scripts.Managers;
using GameFolders.Scripts;
using System.Collections;

public class UnlockedCollections : MonoBehaviour
{
    [Header("Collection Display Settings")]
    [SerializeField] private GameObject unlockedCollectionPrefab;
    [SerializeField] private Transform collectionsContainer;

    private List<GameObject> instantiatedCollections = new List<GameObject>();

    private void OnEnable()
    {
        // GameStart eventine subscribe ol
        GameEvents.OnGameStart += OnGameStart;
    }

    private void OnDisable()
    {
        // GameStart eventinden unsubscribe ol
        GameEvents.OnGameStart -= OnGameStart;
    }

    private void OnGameStart()
    {
        // InGameUIManager'daki gibi kısa bir gecikme ile başlat
        StartCoroutine(DisplayLevelCollectionsWithDelay());
    }

    private IEnumerator DisplayLevelCollectionsWithDelay()
    {
        yield return new WaitForSeconds(0.1f); // 100ms gecikme - level data yüklensin
        DisplayLevelCollections();
    }

    private void DisplayLevelCollections()
    {
        // Önceki collection'ları temizle
        ClearInstantiatedCollections();

        // GameManager'dan current level data'sını al - InGameUIManager ile aynı yöntem
        if (GameManager.Instance?.CurrentLevelData == null)
        {
            Debug.LogWarning("[UnlockedCollections] GameManager veya CurrentLevelData bulunamadı!");
            return;
        }

        var currentLevelData = GameManager.Instance.CurrentLevelData;
        var collectibleTypes = currentLevelData.CollectablePieceType;

        if (collectibleTypes == null || collectibleTypes.Count == 0)
        {
            Debug.Log("[UnlockedCollections] Bu levelde hiç collection yok.");
            return;
        }

        // Her collection type için prefab oluştur
        foreach (var collectionType in collectibleTypes)
        {
            if (collectionType == CollectiblePieceType.None) continue;

            CreateCollectionDisplay(collectionType);
        }

        Debug.Log($"[UnlockedCollections] {instantiatedCollections.Count} collection gösterildi.");
    }

    private void CreateCollectionDisplay(CollectiblePieceType collectionType)
    {
        if (unlockedCollectionPrefab == null)
        {
            Debug.LogError("[UnlockedCollections] UnlockedCollectionPrefab atanmamış!");
            return;
        }

        if (collectionsContainer == null)
        {
            Debug.LogError("[UnlockedCollections] CollectionsContainer atanmamış!");
            return;
        }

        // Prefab'ı instantiate et
        GameObject collectionGO = Instantiate(unlockedCollectionPrefab, collectionsContainer);

        // Image component'ini bul
        Image collectionImage = collectionGO.GetComponent<Image>();
        if (collectionImage == null)
        {
            collectionImage = collectionGO.GetComponentInChildren<Image>();
        }

        if (collectionImage == null)
        {
            Debug.LogError($"[UnlockedCollections] {collectionType} için Image component bulunamadı!");
            Destroy(collectionGO);
            return;
        }

        // InGameUIManager'dan sprite'ı al - aynı yöntemle
        Sprite collectionSprite = null;
        if (InGameUIManager.Instance != null)
        {
            collectionSprite = InGameUIManager.Instance.GetCollectionSprite(collectionType);
        }
        else
        {
            Debug.LogWarning("[UnlockedCollections] InGameUIManager.Instance bulunamadı!");
        }

        if (collectionSprite != null)
        {
            collectionImage.sprite = collectionSprite;
            Debug.Log($"[UnlockedCollections] {collectionType} collection'ı için sprite atandı.");
        }
        else
        {
            Debug.LogWarning($"[UnlockedCollections] {collectionType} için sprite bulunamadı!");
        }

        // Instantiated list'e ekle
        instantiatedCollections.Add(collectionGO);
    }

    private void ClearInstantiatedCollections()
    {
        // Önceki collection'ları temizle
        foreach (var collectionGO in instantiatedCollections)
        {
            if (collectionGO != null)
            {
                Destroy(collectionGO);
            }
        }

        instantiatedCollections.Clear();
    }

    private void OnDestroy()
    {
        ClearInstantiatedCollections();
    }
}
