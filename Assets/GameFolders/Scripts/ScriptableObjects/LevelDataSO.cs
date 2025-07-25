using System.Collections.Generic;
using UnityEngine;

namespace GameFolders.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "LevelDataSO", menuName = "ScriptableObjects/LevelDataSO")]
    public class LevelDataSO : ScriptableObject
    {
        [Header("Level Information")] 
        [SerializeField] private float levelTime;
        [SerializeField] private LuggageType _targetLuggageType;
        [SerializeField] private CollectiblePieceType collectiblePieceType;
        [SerializeField] private int targetLuggageCount;
        [SerializeField] private int junkPieceCount;
        [SerializeField] private bool hasCollectiblePiece;
        [SerializeField] private List<LuggageType> luggageTypes;
        
        public List<LuggageType> LuggageTypes => luggageTypes;
        public float Time => levelTime;
        public LuggageType TargetLuggageType => _targetLuggageType;
        public CollectiblePieceType CollectiblePieceType => collectiblePieceType;
        public bool HasCollectiblePiece => hasCollectiblePiece;
        public int TargetLuggageCount => targetLuggageCount;
        public int JunkPieceCount => junkPieceCount;
    }

    public enum CollectiblePieceType
    {
        None,
    }
    public enum JunkPieceType
    {
        None
    }
    public enum LuggageType
    {
        None,
        Type1,
        Type2,
        Type3,
        Type4,
        Type5,
        Type6,
    }
}
