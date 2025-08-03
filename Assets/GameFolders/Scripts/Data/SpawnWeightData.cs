using System;
using UnityEngine;

namespace GameFolders.Scripts.Data
{
    [Serializable]
    public struct SpawnWeightData
    {
        [Range(0,10)] public float LuggageSpawnWeight;
        [Range(0,10)] public float CollectableSpawnWeight;
        [Range(0,10)] public float JunkSpawnWeight;
    }
}