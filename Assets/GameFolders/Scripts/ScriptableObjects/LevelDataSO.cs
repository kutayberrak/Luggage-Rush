using System;
using System.Collections.Generic;
using GameFolders.Scripts.Data;
using GameFolders.Scripts.Enums;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameFolders.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "LevelDataSO", menuName = "ScriptableObjects/LevelDataSO")]
    public class LevelDataSO : ScriptableObject
    {
        [InfoBox("Luggage target count must be power of 3 (3, 9, 27, etc.)")]
        
        [Header("Level Configuration")]
        [Tooltip("Seconds")]
        [SerializeField] private float levelTimeInSeconds;
        [SerializeField] private List<TargetLuggageInfo> targetLuggageInfo;
        [Tooltip("Different luggage types to spawn current level")]
        [SerializeField] private List<LuggageType> luggageTypesToSpawn;
        
        [Header("Collectible Configuration")]
        [SerializeField] private List<CollectablePieceInfo> collectablePieceInfo;
        [SerializeField] private bool hasCollectiblePiece;
        
        [Header("Junk Configuration")]
        [SerializeField] private List<JunkPieceInfo> junkPieceInfo;
        
        public List<LuggageType> LuggageTypesToSpawn => luggageTypesToSpawn;
        public List<CollectablePieceInfo> CollectablePieceInfo => collectablePieceInfo;
        public List<TargetLuggageInfo> TargetLuggageInfo => targetLuggageInfo;
        public bool HasCollectiblePiece => hasCollectiblePiece;
        public float TimeInSeconds => levelTimeInSeconds;
        
    }
}
