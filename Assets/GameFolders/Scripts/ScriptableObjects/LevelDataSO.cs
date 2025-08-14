using System.Collections.Generic;
using System.Linq;
using GameFolders.Scripts.Data;
using GameFolders.Scripts.Enums;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameFolders.Scripts.ScriptableObjects
{
    public enum LevelLimitType
    {
        Time,
        MoveCount
    }

    public enum ConveyorType
    {
        ConveyorA,
        ConveyorB,
        ConveyorC
    }

    [CreateAssetMenu(fileName = "LevelDataSO", menuName = "ScriptableObjects/LevelDataSO")]
    public class LevelDataSO : ScriptableObject
    {
        [InfoBox("Luggage target count must be power of 3 (3, 9, 27, etc.)")]

        [Header("Level Configuration")]
        [SerializeField] private LevelLimitType levelLimitType = LevelLimitType.Time;
        
        [ShowIf("levelLimitType", LevelLimitType.Time)]
        [Tooltip("Seconds")]
        [SerializeField] private float levelTimeInSeconds;
        
        [ShowIf("levelLimitType", LevelLimitType.MoveCount)]
        [Tooltip("Number of moves allowed")]
        [SerializeField] private int levelMoveLimitCount;
        
        [SerializeField] private List<TargetLuggageInfo> targetLuggageInfo;
        [SerializeField] private bool hasCollectiblePiece;
        
        [Header("Configuration of objects to be spawned")]
        [SerializeField] private List<CollectiblePieceType> collectablePieceType;
        [SerializeField] private List<LuggageType> luggageTypesToSpawn;
        [SerializeField] private List<JunkPieceType> junkPieceType;
        [SerializeField] private List<SpecialType> specialTypesToSpawn;
        [SerializeField] private SpawnWeightData spawnWeightData;
        [SerializeField] private float spawnInterval = 0.2f;

        [Header("Conveyor Types")]
        [SerializeField] private ConveyorType conveyorType;

        public ConveyorType ConveyorType => conveyorType;
        public float SpawnInterval => spawnInterval;
        public List<JunkPieceType> JunkPieceTypes => junkPieceType;
        public List<LuggageType> LuggageTypesToSpawn => luggageTypesToSpawn;
        public List<SpecialType> SpecialTypesToSpawn => specialTypesToSpawn;
        public List<CollectiblePieceType> CollectablePieceType => collectablePieceType;
        public List<TargetLuggageInfo> TargetLuggageInfo => targetLuggageInfo;
        public SpawnWeightData SpawnWeightData => spawnWeightData;
        public bool HasCollectiblePiece => hasCollectiblePiece;
        public float TimeInSeconds => levelTimeInSeconds;
        public int MoveLimitCount => levelMoveLimitCount;
        public LevelLimitType LimitType => levelLimitType;
        public bool IsTimeBased => levelLimitType == LevelLimitType.Time;
        public bool IsMoveBased => levelLimitType == LevelLimitType.MoveCount;

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
