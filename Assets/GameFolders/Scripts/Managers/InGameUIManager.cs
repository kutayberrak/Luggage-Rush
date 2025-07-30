using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using GameFolders.Scripts.ScriptableObjects;
using GameFolders.Scripts.Data;
using GameFolders.Scripts.Enums;
using GameFolders.Scripts.Managers;
using System.Collections;

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
    private LevelDataSO currentLevelData;
    
    // **YENİ**: UI başlatıldıktan sonra manuel sisteme geç
    private bool isUIInitialized = false;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Start'ta hemen başlatma, level başladığında başlatılacak
    }
    
    void Update()
    {
        // **YENİ**: UpdateObjectivesProgress'i tamamen kaldırdık çünkü manuel sistem kullanıyoruz
        // Slot'lardaki objeleri saymak yerine, sadece IncreaseCollectCount() ile toplanan sayısını arttırıyoruz
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
        
        foreach (var targetInfo in currentLevelData.TargetLuggageInfo)
        {
            if (targetInfo.Count <= 0) continue; // 0 count olanları gösterme
            
            CreateObjectiveItem(targetInfo);
        }
        
        // Panel'i göster
        if (objectivesPanel != null)
        {
            objectivesPanel.SetActive(true);
        }
        
        Debug.Log($"[InGameUIManager] {currentLevelData.TargetLuggageInfo.Count} hedef gösterildi");
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
    
    // **YENİ**: Debug için hedefleri yazdır
    [ContextMenu("Debug Objectives")]
    public void DebugObjectives()
    {
        if (currentLevelData == null)
        {
            Debug.Log("CurrentLevelData null!");
            return;
        }
        
        Debug.Log("=== Level Objectives ===");
        foreach (var target in currentLevelData.TargetLuggageInfo)
        {
            Debug.Log($"Hedef: {target.LuggageType} - {target.Count} adet");
        }
        Debug.Log("=======================");
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
    
    // **YENİ**: Bütün hedeflerin tamamlanıp tamamlanmadığını kontrol et
    private void CheckAllObjectivesCompleted()
    {
        bool allCompleted = true;
        
        foreach (var kvp in objectiveItems)
        {
            ObjectiveUIItem objectiveItem = kvp.Value;
            if (!objectiveItem.IsCompleted())
            {
                allCompleted = false;
                break;
            }
        }
        
        if (allCompleted)
        {
            OnAllObjectivesCompleted();
        }
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
        Debug.Log("[InGameUIManager] Waiting for match animations to complete...");
        
        // SlotManager'dan match animasyonlarının bitmesini bekle
        if (SlotManager.Instance != null)
        {
            // Match işlemi devam ediyorsa bekle
            while (SlotManager.Instance.IsProcessingMatch())
            {
                Debug.Log("[InGameUIManager] Match animations still in progress, waiting...");
                yield return new WaitForSeconds(0.1f);
            }
            
            // Ek güvenlik için kısa bir süre daha bekle
            yield return new WaitForSeconds(.9f);
        }
        
        Debug.Log("[InGameUIManager] Match animations completed, triggering level win!");
        
        // GameManager'a bildir
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelWin();
        }
    }
}
