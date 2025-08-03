using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private bool hasCollectiblePiece;
        
        [Header("Configuration of objects to be spawned")]
        [SerializeField] private List<CollectiblePieceType> collectablePieceType;
        [SerializeField] private List<LuggageType> luggageTypesToSpawn;
        [SerializeField] private List<JunkPieceType> junkPieceType;
        [SerializeField] private SpawnWeightData spawnWeightData;
        [SerializeField] private float spawnInterval = 0.2f;
        public float SpawnInterval => spawnInterval;
        public List<JunkPieceType> JunkPieceTypes => junkPieceType;
        public List<LuggageType> LuggageTypesToSpawn => luggageTypesToSpawn;
        public List<CollectiblePieceType> CollectablePieceType => collectablePieceType;
        public List<TargetLuggageInfo> TargetLuggageInfo => targetLuggageInfo;
        public SpawnWeightData SpawnWeightData => spawnWeightData;
        public bool HasCollectiblePiece => hasCollectiblePiece;
        public float TimeInSeconds => levelTimeInSeconds;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (targetLuggageInfo == null || luggageTypesToSpawn == null) return;
            
            var targetTypes = targetLuggageInfo
                .Select(t => t.LuggageType)
                .Distinct()
                .ToHashSet();

            var missingTypes = targetTypes
                .Where(t => !luggageTypesToSpawn.Contains(t))
                .ToList();

            // Add missing types to luggageTypesToSpawn if they are not already present
            foreach (var type in missingTypes)
            {
                luggageTypesToSpawn.Add(type);
                Debug.LogWarning($"[AutoFix] Added missing LuggageType '{type}' to luggageTypesToSpawn in '{name}'", this);
            }
#endif
        }
    }
}
