using System;
using UnityEngine;

namespace GameFolders.Scripts.Data
{
    [Serializable]
    public struct SpawnWeightData
    {
        [Range(0,10)] public int LuggageSpawnWeight;
        [Range(0,10)] public int CollectableSpawnWeight;
        [Range(0,10)] public int JunkSpawnWeight;
    }
}