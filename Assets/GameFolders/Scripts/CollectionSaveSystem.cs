using System.Collections.Generic;
using UnityEngine;
using GameFolders.Scripts.Enums;

/// <summary>
/// PlayerPrefs tabanlý koleksiyon kayýt sistemi
/// </summary>
public static class CollectionSaveSystem
{
    private const string COLLECTION_KEY_PREFIX = "Collection_";
    private const string COLLECTION_LIST_KEY = "CollectionList";
    
    /// <summary>
    /// Koleksiyonu açýk olarak iþaretle
    /// </summary>
    public static void UnlockCollection(CollectiblePieceType collectionType, string country)
    {
        string key = GetCollectionKey(collectionType, country);
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        
        Debug.Log($"[CollectionSaveSystem] Unlocked: {collectionType} in {country}");
    }
    
    /// <summary>
    /// Koleksiyonu kilitli olarak iþaretle
    /// </summary>
    public static void LockCollection(CollectiblePieceType collectionType, string country)
    {
        string key = GetCollectionKey(collectionType, country);
        PlayerPrefs.SetInt(key, 0);
        PlayerPrefs.Save();
        
        Debug.Log($"[CollectionSaveSystem] Locked: {collectionType} in {country}");
    }
    
    /// <summary>
    /// Koleksiyonun açýk olup olmadýðýný kontrol et
    /// </summary>
    public static bool IsCollectionUnlocked(CollectiblePieceType collectionType, string country)
    {
        string key = GetCollectionKey(collectionType, country);
        return PlayerPrefs.GetInt(key, 0) == 1;
    }
    
    /// <summary>
    /// Belirli bir ülkedeki açýk koleksiyon sayýsýný getir
    /// </summary>
    public static int GetUnlockedCountForCountry(string country, List<CollectionData> allCollections)
    {
        int unlockedCount = 0;
        
        foreach (var collection in allCollections)
        {
            if (collection.CollectionCountry == country && 
                IsCollectionUnlocked(collection.CollectionType, country))
            {
                unlockedCount++;
            }
        }
        
        return unlockedCount;
    }
    
    /// <summary>
    /// Belirli bir ülkedeki toplam koleksiyon sayýsýný getir
    /// </summary>
    public static int GetTotalCountForCountry(string country, List<CollectionData> allCollections)
    {
        int totalCount = 0;
        
        foreach (var collection in allCollections)
        {
            if (collection.CollectionCountry == country)
            {
                totalCount++;
            }
        }
        
        return totalCount;
    }
    
    /// <summary>
    /// Tüm koleksiyonlarý sýfýrla
    /// </summary>
    public static void ResetAllCollections()
    {
        // Tüm koleksiyon anahtarlarýný sil
        foreach (CollectiblePieceType pieceType in System.Enum.GetValues(typeof(CollectiblePieceType)))
        {
            if (pieceType == CollectiblePieceType.None) continue;
            
            // Tüm olasý ülkeler için sil (bu listeyi geniþletebilirsiniz)
            string[] countries = { "Turkey", "USA", "Germany", "France", "Japan" }; // Örnek ülkeler
            
            foreach (string country in countries)
            {
                string key = GetCollectionKey(pieceType, country);
                if (PlayerPrefs.HasKey(key))
                {
                    PlayerPrefs.DeleteKey(key);
                }
            }
        }
        
        PlayerPrefs.Save();
        Debug.Log("[CollectionSaveSystem] All collections reset!");
    }
    
    /// <summary>
    /// Koleksiyon için benzersiz anahtar oluþtur
    /// </summary>
    private static string GetCollectionKey(CollectiblePieceType collectionType, string country)
    {
        return $"{COLLECTION_KEY_PREFIX}{country}_{collectionType}";
    }
    
    /// <summary>
    /// Belirli bir ülke için progress yüzdesi hesapla
    /// </summary>
    public static float GetProgressPercentageForCountry(string country, List<CollectionData> allCollections)
    {
        int totalCount = GetTotalCountForCountry(country, allCollections);
        if (totalCount == 0) return 0f;
        
        int unlockedCount = GetUnlockedCountForCountry(country, allCollections);
        return (float)unlockedCount / totalCount * 100f;
    }
    
    /// <summary>
    /// Genel progress yüzdesi hesapla
    /// </summary>
    public static float GetOverallProgressPercentage(List<CollectionData> allCollections)
    {
        int totalCount = allCollections.Count;
        if (totalCount == 0) return 0f;
        
        int unlockedCount = 0;
        foreach (var collection in allCollections)
        {
            if (IsCollectionUnlocked(collection.CollectionType, collection.CollectionCountry))
            {
                unlockedCount++;
            }
        }
        
        return (float)unlockedCount / totalCount * 100f;
    }
    
    /// <summary>
    /// Debug için tüm kayýtlý koleksiyonlarý listele
    /// </summary>
    public static void DebugLogAllSavedCollections()
    {
        Debug.Log("=== SAVED COLLECTIONS ===");
        
        foreach (CollectiblePieceType pieceType in System.Enum.GetValues(typeof(CollectiblePieceType)))
        {
            if (pieceType == CollectiblePieceType.None) continue;
            
            string[] countries = { "Turkey", "USA", "Germany", "France", "Japan" }; // Örnek ülkeler
            
            foreach (string country in countries)
            {
                string key = GetCollectionKey(pieceType, country);
                if (PlayerPrefs.HasKey(key))
                {
                    bool isUnlocked = PlayerPrefs.GetInt(key) == 1;
                    Debug.Log($"{country} - {pieceType}: {(isUnlocked ? "UNLOCKED" : "LOCKED")}");
                }
            }
        }
        
        Debug.Log("=== END ===");
    }
}