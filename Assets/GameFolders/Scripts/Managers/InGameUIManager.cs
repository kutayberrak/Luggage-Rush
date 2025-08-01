using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using GameFolders.Scripts.ScriptableObjects;
using GameFolders.Scripts.Data;
using GameFolders.Scripts.Enums;
using GameFolders.Scripts.Managers;
using System.Collections;
using GameFolders.Scripts;

public class InGameUIManager : MonoBehaviour
{
    public static InGameUIManager Instance;

    [Header("Level Objectives UI")]
    [SerializeField] private GameObject objectivesPanel;
    [SerializeField] private Transform objectivesContainer;
    [SerializeField] private GameObject objectiveItemPrefab;
    
    [Header("Objective Item UI Elements")]
    [SerializeField] private Sprite[] luggageTypeSprites; // Inspector'da atanacak
    [SerializeField] private string[] luggageTypeNames; // Inspector'da atanacak
    
    private Dictionary<LuggageType, ObjectiveUIItem> objectiveItems = new Dictionary<LuggageType, ObjectiveUIItem>();
    private Dictionary<CollectiblePieceType, ObjectiveUIItem> collectionObjectiveItems = new Dictionary<CollectiblePieceType, ObjectiveUIItem>();
    private LevelDataSO currentLevelData;
    
    // **YENİ**: UI başlatıldıktan sonra manuel sisteme geç
    private bool isUIInitialized = false;

    private void Awake()
    {
        Instance = this;
    }
    
    public void InitializeObjectivesUI()
    {
        if (GameManager.Instance?.CurrentLevelData == null)
        {
            Debug.LogWarning("GameManager veya CurrentLevelData bulunamadı!");
            return;
        }
        
        currentLevelData = GameManager.Instance.CurrentLevelData;
        
        // Mevcut objective item'ları temizle
        ClearObjectivesUI();
        
        // Level hedeflerini UI'da göster
        ShowLevelObjectives();
        
        // **YENİ**: UI başlatıldı, artık manuel sisteme geç
        isUIInitialized = true;
    }
    
    private void ShowLevelObjectives()
    {
        if (currentLevelData == null || objectivesContainer == null) return;
        
        // Luggage hedeflerini göster
        foreach (var targetInfo in currentLevelData.TargetLuggageInfo)
        {
            if (targetInfo.Count <= 0) continue; // 0 count olanları gösterme
            
            CreateObjectiveItem(targetInfo);
        }
        
        // Collection hedeflerini göster
        foreach (var collectionType in currentLevelData.CollectablePieceType)
        {
            if (collectionType == CollectiblePieceType.None) continue; // None olanları gösterme
            
            CreateCollectionObjectiveItem(collectionType);
        }
        
        // Panel'i göster
        if (objectivesPanel != null)
        {
            objectivesPanel.SetActive(true);
        }
        
        Debug.Log($"[InGameUIManager] {currentLevelData.TargetLuggageInfo.Count} luggage hedefi ve {currentLevelData.CollectablePieceType.Count} collection hedefi gösterildi");
    }
    
    private void CreateObjectiveItem(TargetLuggageInfo targetInfo)
    {
        if (objectiveItemPrefab == null)
        {
            Debug.LogError("Objective Item Prefab atanmamış!");
            return;
        }
        
        GameObject itemGO = Instantiate(objectiveItemPrefab, objectivesContainer);
        ObjectiveUIItem objectiveItem = itemGO.GetComponent<ObjectiveUIItem>();
        
        if (objectiveItem == null)
        {
            Debug.LogError("ObjectiveUIItem component bulunamadı!");
            return;
        }
        
        // Objective item'ı yapılandır
        objectiveItem.Initialize(targetInfo.LuggageType, targetInfo.Count, GetLuggageSprite(targetInfo.LuggageType), GetLuggageName(targetInfo.LuggageType));
        
        // Dictionary'e ekle
        objectiveItems[targetInfo.LuggageType] = objectiveItem;
    }
    
    private void CreateCollectionObjectiveItem(CollectiblePieceType collectionType)
    {
        if (objectiveItemPrefab == null)
        {
            Debug.LogError("Objective Item Prefab atanmamış!");
            return;
        }
        
        GameObject itemGO = Instantiate(objectiveItemPrefab, objectivesContainer);
        ObjectiveUIItem objectiveItem = itemGO.GetComponent<ObjectiveUIItem>();
        
        if (objectiveItem == null)
        {
            Debug.LogError("ObjectiveUIItem component bulunamadı!");
            return;
        }
        
        // Collection objective item'ı yapılandır (1 adet toplanacak)
        objectiveItem.InitializeCollection(collectionType, 1, GetCollectionSprite(collectionType), GetCollectionName(collectionType));
        
        // Dictionary'e ekle
        collectionObjectiveItems[collectionType] = objectiveItem;
    }
    
    // **YENİ**: UpdateObjectivesProgress metodunu kaldırdık çünkü artık manuel sistem kullanıyoruz
    // Slot'lardaki objeleri saymak yerine, sadece IncreaseCollectCount() ile toplanan sayısını arttırıyoruz
    
    private void ClearObjectivesUI()
    {
        if (objectivesContainer == null) return;
        
        // Mevcut child'ları temizle
        for (int i = objectivesContainer.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(objectivesContainer.GetChild(i).gameObject);
        }
        
        objectiveItems.Clear();
        collectionObjectiveItems.Clear();
    }
    
    private Sprite GetLuggageSprite(LuggageType luggageType)
    {
        int index = (int)luggageType - 1; // None = 0, LuggageType1 = 1, vs.
        if (index >= 0 && index < luggageTypeSprites.Length)
        {
            return luggageTypeSprites[index];
        }
        return null;
    }
    
    private string GetLuggageName(LuggageType luggageType)
    {
        int index = (int)luggageType - 1; // None = 0, LuggageType1 = 1, vs.
        if (index >= 0 && index < luggageTypeNames.Length)
        {
            return luggageTypeNames[index];
        }
        return luggageType.ToString();
    }
    
    public Sprite GetCollectionSprite(CollectiblePieceType collectionType)
    {
        // CollectionManager'dan collection data'sını al
        if (CollectionManager.Instance != null)
        {
            var collectionData = CollectionManager.Instance.GetCollectionByType(collectionType);
            if (collectionData != null)
            {
                return collectionData.CollectionImage;
            }
        }
        return null;
    }
    
    private string GetCollectionName(CollectiblePieceType collectionType)
    {
        // CollectionManager'dan collection data'sını al
        if (CollectionManager.Instance != null)
        {
            var collectionData = CollectionManager.Instance.GetCollectionByType(collectionType);
            if (collectionData != null)
            {
                return collectionData.CollectionName;
            }
        }
        return collectionType.ToString();
    }
    
    // **YENİ**: Level başladığında çağrılacak metod
    public void OnLevelStarted()
    {
        // Kısa bir gecikme ile UI'ı başlat (level data yüklensin)
        StartCoroutine(InitializeUIWithDelay());
    }
    
    private IEnumerator InitializeUIWithDelay()
    {
        yield return new WaitForSeconds(0.1f); // 100ms gecikme
        InitializeObjectivesUI();
    }
    
    // **YENİ**: Level bittiğinde çağrılacak metod
    public void OnLevelEnded()
    {
        if (objectivesPanel != null)
        {
            objectivesPanel.SetActive(false);
        }
        ClearObjectivesUI();
    }
    
    
    // **YENİ**: Obje slot'a ulaştığında toplanan sayısını arttır
    public void OnObjectReachedSlot(LuggageType luggageType)
    {
        if (objectiveItems.ContainsKey(luggageType))
        {
            ObjectiveUIItem objectiveItem = objectiveItems[luggageType];
            objectiveItem.IncreaseCollectCount(); // Bu zaten UI'ı güncelliyor
            Debug.Log($"[InGameUIManager] {luggageType} toplanan sayısı arttırıldı. Mevcut: {objectiveItem.GetCurrentCollected()}");
            
            // **YENİ**: Bütün hedeflerin tamamlanıp tamamlanmadığını kontrol et
            CheckAllObjectivesCompleted();
        }
        else
        {
            Debug.LogWarning($"[InGameUIManager] {luggageType} için objective item bulunamadı!");
        }
    }
    
    // **YENİ**: Collection toplandığında çağrılacak method
    public void OnCollectionCollected(CollectiblePieceType collectionType)
    {
        if (collectionObjectiveItems.ContainsKey(collectionType))
        {
            ObjectiveUIItem objectiveItem = collectionObjectiveItems[collectionType];
            objectiveItem.IncreaseCollectCount(); // Bu zaten UI'ı güncelliyor
            Debug.Log($"[InGameUIManager] {collectionType} collection'ı toplandı. Mevcut: {objectiveItem.GetCurrentCollected()}");
            
            // **YENİ**: Bütün hedeflerin tamamlanıp tamamlanmadığını kontrol et
            CheckAllObjectivesCompleted();
        }
        else
        {
            Debug.LogWarning($"[InGameUIManager] {collectionType} için collection objective item bulunamadı!");
        }
    }
    
    // **YENİ**: Bütün hedeflerin tamamlanıp tamamlanmadığını kontrol et
    private void CheckAllObjectivesCompleted()
    {
        bool allCompleted = true;
        
        // Luggage hedeflerini kontrol et
        foreach (var kvp in objectiveItems)
        {
            ObjectiveUIItem objectiveItem = kvp.Value;
            if (!objectiveItem.IsCompleted())
            {
                allCompleted = false;
                break;
            }
        }
        
        // Collection hedeflerini kontrol et
        if (allCompleted)
        {
            foreach (var kvp in collectionObjectiveItems)
            {
                ObjectiveUIItem objectiveItem = kvp.Value;
                if (!objectiveItem.IsCompleted())
                {
                    allCompleted = false;
                    break;
                }
            }
        }
        
        if (allCompleted)
        {
            OnAllObjectivesCompleted();
        }
    }
    public Transform GetCollectionObjectiveTransform(CollectiblePieceType type)
    {
        if (collectionObjectiveItems.TryGetValue(type, out var uiItem))
            return uiItem.IconRectTransform;
        return null;
    }

    // **YENİ**: Bütün hedefler tamamlandığında çağrılır
    private void OnAllObjectivesCompleted()
    {  
        // Match animasyonlarının bitmesini bekle
        StartCoroutine(WaitForMatchAnimationsAndWin());
    }
    
    // **YENİ**: Match animasyonlarının bitmesini bekleyip oyunu kazandır
    private IEnumerator WaitForMatchAnimationsAndWin()
    {
        // SlotManager'dan match animasyonlarının bitmesini bekle
        if (SlotManager.Instance != null)
        {
            // Match işlemi devam ediyorsa bekle
            while (SlotManager.Instance.IsProcessingMatch())
            {
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(.9f);
        }
        
        // GameEvents üzerinden level win tetikle
        GameEvents.TriggerLevelWin();
    }
}
