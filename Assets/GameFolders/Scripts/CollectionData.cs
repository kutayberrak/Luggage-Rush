using UnityEngine;
using GameFolders.Scripts.Enums;

[CreateAssetMenu(fileName = "New Collection Data", menuName = "Game/Collection Data")]
public class CollectionData : ScriptableObject
{
    [Header("Collection Information")]
    [SerializeField] private CollectiblePieceType collectionType;
    [SerializeField] private Sprite collectionImage;
    [SerializeField] private string collectionCountry;

    public Vector3 collectionInstantiatePosition = Vector3.zero;
    public Quaternion collectionInstantiateRotation = Quaternion.identity;
    public Vector3 collectionInstantiateScale = Vector3.one;

    // Properties
    public CollectiblePieceType CollectionType => collectionType;
    public Sprite CollectionImage => collectionImage;
    public string CollectionCountry => collectionCountry;

    // Additional Properties
    [Header("Additional Properties")]
    [SerializeField] private string collectionName;
    [SerializeField] private string description;

    // ⚠️ Bu field artık sadece default değer için kullanılıyor, gerçek veri PlayerPrefs'ten geliyor
    [SerializeField] private bool defaultUnlocked = false;

    public string CollectionName => collectionName;
    public string Description => description;

    /// <summary>
    /// PlayerPrefs'ten unlock durumunu kontrol et
    /// Runtime'da her zaman PlayerPrefs'ten okur
    /// </summary>
    public bool IsUnlocked
    {
        get
        {
            // Eğer Application çalışmıyorsa (Editor'da design time) default değeri döndür
            if (!Application.isPlaying)
            {
                return defaultUnlocked;
            }

            // Runtime'da PlayerPrefs'ten oku
            return CollectionSaveSystem.IsCollectionUnlocked(collectionType, collectionCountry);
        }
        set
        {
            // Setter sadece PlayerPrefs'e yazılması için
            if (Application.isPlaying)
            {
                if (value)
                {
                    CollectionSaveSystem.UnlockCollection(collectionType, collectionCountry);
                }
                else
                {
                    CollectionSaveSystem.LockCollection(collectionType, collectionCountry);
                }
            }
            else
            {
                // Editor'da default değeri ayarla
                defaultUnlocked = value;
            }
        }
    }

    /// <summary>
    /// Koleksiyonu aç (PlayerPrefs'e kaydet)
    /// </summary>
    public void UnlockCollection()
    {
        if (Application.isPlaying)
        {
            CollectionSaveSystem.UnlockCollection(collectionType, collectionCountry);
            Debug.Log($"[CollectionData] Unlocked: {collectionName} ({collectionType}) in {collectionCountry}");
        }
        else
        {
            defaultUnlocked = true;
            Debug.Log($"[CollectionData] Set default unlock for: {collectionName} (Editor mode)");
        }
    }

    /// <summary>
    /// Koleksiyonu kilitle (PlayerPrefs'e kaydet)
    /// </summary>
    public void ResetCollection()
    {
        if (Application.isPlaying)
        {
            CollectionSaveSystem.LockCollection(collectionType, collectionCountry);
            Debug.Log($"[CollectionData] Reset: {collectionName} ({collectionType}) in {collectionCountry}");
        }
        else
        {
            defaultUnlocked = false;
            Debug.Log($"[CollectionData] Reset default unlock for: {collectionName} (Editor mode)");
        }
    }

    /// <summary>
    /// Koleksiyon verilerini ayarla
    /// </summary>
    public void SetCollectionData(CollectiblePieceType type, Sprite image, string name = "", string desc = "")
    {
        collectionType = type;
        collectionImage = image;
        collectionName = name;
        description = desc;
    }

    /// <summary>
    /// Debug için - koleksiyon durumunu logla
    /// </summary>
    [ContextMenu("Debug Collection Status")]
    public void DebugCollectionStatus()
    {
        Debug.Log($"=== COLLECTION DEBUG ===");
        Debug.Log($"Name: {collectionName}");
        Debug.Log($"Type: {collectionType}");
        Debug.Log($"Country: {collectionCountry}");
        Debug.Log($"Is Unlocked: {IsUnlocked}");
        Debug.Log($"Default Unlocked: {defaultUnlocked}");
        Debug.Log($"Application Playing: {Application.isPlaying}");
        Debug.Log($"======================");
    }

    /// <summary>
    /// Editor'da test için - unlock toggle
    /// </summary>
    [ContextMenu("Toggle Unlock (Test)")]
    public void ToggleUnlockForTest()
    {
        if (IsUnlocked)
        {
            ResetCollection();
        }
        else
        {
            UnlockCollection();
        }
    }

    // Editor için validation
    private void OnValidate()
    {
        // Eğer scale sıfırsa, default olarak (1,1,1) yap
        if (collectionInstantiateScale == Vector3.zero)
        {
            collectionInstantiateScale = Vector3.one;
        }

        // Eğer isim boşsa, type'dan otomatik isim oluştur
        if (string.IsNullOrEmpty(collectionName) && collectionType != CollectiblePieceType.None)
        {
            collectionName = collectionType.ToString();
        }
    }
}