using System.Collections.Generic;
using UnityEngine;
using GameFolders.Scripts.Enums;

/// <summary>
/// PlayerPrefs tabanl� koleksiyon kay�t sistemi
/// </summary>
public static class CollectionSaveSystem
{
    private const string COLLECTION_KEY_PREFIX = "Collection_";
    private const string COLLECTION_LIST_KEY = "CollectionList";
    
    /// <summary>
    /// Koleksiyonu a��k olarak i�aretle
    /// </summary>
    public static void UnlockCollection(CollectiblePieceType collectionType, string country)
    {
        string key = GetCollectionKey(collectionType, country);
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        
        Debug.Log($"[CollectionSaveSystem] Unlocked: {collectionType} in {country}");
    }
    
    /// <summary>
    /// Koleksiyonu kilitli olarak i�aretle
    /// </summary>
    public static void LockCollection(CollectiblePieceType collectionType, string country)
    {
        string key = GetCollectionKey(collectionType, country);
        PlayerPrefs.SetInt(key, 0);
        PlayerPrefs.Save();
        
        Debug.Log($"[CollectionSaveSystem] Locked: {collectionType} in {country}");
    }
    
    /// <summary>
    /// Koleksiyonun a��k olup olmad���n� kontrol et
    /// </summary>
    public static bool IsCollectionUnlocked(CollectiblePieceType collectionType, string country)
    {
        string key = GetCollectionKey(collectionType, country);
        return PlayerPrefs.GetInt(key, 0) == 1;
    }
    
    /// <summary>
    /// Belirli bir �lkedeki a��k koleksiyon say�s�n� getir
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
    /// Belirli bir �lkedeki toplam koleksiyon say�s�n� getir
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
    /// T�m koleksiyonlar� s�f�rla
    /// </summary>
    public static void ResetAllCollections()
    {
        // T�m koleksiyon anahtarlar�n� sil
        foreach (CollectiblePieceType pieceType in System.Enum.GetValues(typeof(CollectiblePieceType)))
        {
            if (pieceType == CollectiblePieceType.None) continue;
            
            // T�m olas� �lkeler i�in sil (bu listeyi geni�letebilirsiniz)
            string[] countries = { "Turkey", "USA", "Germany", "France", "Japan" }; // �rnek �lkeler
            
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
    /// Koleksiyon i�in benzersiz anahtar olu�tur
    /// </summary>
    private static string GetCollectionKey(CollectiblePieceType collectionType, string country)
    {
        return $"{COLLECTION_KEY_PREFIX}{country}_{collectionType}";
    }
    
    /// <summary>
    /// Belirli bir �lke i�in progress y�zdesi hesapla
    /// </summary>
    public static float GetProgressPercentageForCountry(string country, List<CollectionData> allCollections)
    {
        int totalCount = GetTotalCountForCountry(country, allCollections);
        if (totalCount == 0) return 0f;
        
        int unlockedCount = GetUnlockedCountForCountry(country, allCollections);
        return (float)unlockedCount / totalCount * 100f;
    }
    
    /// <summary>
    /// Genel progress y�zdesi hesapla
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
    /// Debug i�in t�m kay�tl� koleksiyonlar� listele
    /// </summary>
    public static void DebugLogAllSavedCollections()
    {
        Debug.Log("=== SAVED COLLECTIONS ===");
        
        foreach (CollectiblePieceType pieceType in System.Enum.GetValues(typeof(CollectiblePieceType)))
        {
            if (pieceType == CollectiblePieceType.None) continue;
            
            string[] countries = { "Turkey", "USA", "Germany", "France", "Japan" }; // �rnek �lkeler
            
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