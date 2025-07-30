using UnityEngine;
using GameFolders.Scripts.Enums;

[CreateAssetMenu(fileName = "New Collection Data", menuName = "Game/Collection Data")]
public class CollectionData : ScriptableObject
{
    [Header("Collection Information")]
    [SerializeField] private CollectiblePieceType collectionType;
    [SerializeField] private Sprite collectionImage;
    
    // Properties
    public CollectiblePieceType CollectionType => collectionType;
    public Sprite CollectionImage => collectionImage;
    
    // Optional: Additional properties you might need
    [Header("Additional Properties")]
    [SerializeField] private string collectionName;
    [SerializeField] private string description;
    [SerializeField] private bool isUnlocked = false;
    
    public string CollectionName => collectionName;
    public string Description => description;
    public bool IsUnlocked => isUnlocked;
    
    // Method to unlock the collection
    public void UnlockCollection()
    {
        isUnlocked = true;
    }
    
    // Method to set collection data
    public void SetCollectionData(CollectiblePieceType type, Sprite image, string name = "", string desc = "")
    {
        collectionType = type;
        collectionImage = image;
        collectionName = name;
        description = desc;
    }
} 