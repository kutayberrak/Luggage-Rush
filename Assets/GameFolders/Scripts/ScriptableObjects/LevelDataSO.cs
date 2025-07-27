using System.Collections.Generic;
using GameFolders.Scripts.Data;
using GameFolders.Scripts.Enums;
using UnityEngine;

namespace GameFolders.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "LevelDataSO", menuName = "ScriptableObjects/LevelDataSO")]
    public class LevelDataSO : ScriptableObject
    {
        [Header("Level Configuration")]
        [Tooltip("Seconds")]
        [SerializeField] private float levelTimeInSeconds;
        [SerializeField] private List<TargetLuggageInfo> targetLuggageInfo;
        [Tooltip("Different luggage types to spawn current level")]
        [SerializeField] private List<LuggageType> luggageTypes;
        
        [Header("Collectible Configuration")]
        [SerializeField] private List<CollectablePieceInfo> collectablePieceInfo;
        [SerializeField] private bool hasCollectiblePiece;
        
        [Header("Junk Configuration")]
        [SerializeField] private List<JunkPieceInfo> junkPieceInfo;
        
        public List<LuggageType> LuggageTypes => luggageTypes;
        public List<CollectablePieceInfo> CollectablePieceInfo => collectablePieceInfo;
        public List<TargetLuggageInfo> TargetLuggageInfo => targetLuggageInfo;
        public bool HasCollectiblePiece => hasCollectiblePiece;
        public float TimeInSeconds => levelTimeInSeconds;
    }
}
