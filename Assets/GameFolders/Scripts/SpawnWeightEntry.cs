using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class SpawnWeightEntry
{
    public ObjectType objectType;
    [Range(0f, 10f)] public float spawnWeight = 1f;
    [ReadOnly] public float currentWeight = 0f;
}
