using UnityEngine;
using System.Collections.Generic;
using GameFolders.Scripts.Enums;

public class LuggageColliderDatas : MonoBehaviour
{
    public static LuggageColliderDatas Instance;
    public List<BoxColData> luggageColliderDatas = new List<BoxColData>();

    [System.Serializable]
    public struct BoxColData
    {
        public Vector3 center;
        public Vector3 size;
        public LuggageType type;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        var prefabs = ObjectPoolManager.Instance.GetObjectsByType(ObjectType.Luggage);
        luggageColliderDatas.Clear();

        foreach (var prefab in prefabs)
        {
            if (prefab == null) continue;

            var col = prefab.GetComponent<BoxCollider>();
            if (col == null)
            {
                continue;
            }

            var luggageItem = prefab.GetComponent<LuggageItem>();
            LuggageType type = luggageItem != null ? luggageItem.luggageType : default;

            luggageColliderDatas.Add(new BoxColData
            {
                center = col.center,
                size = col.size,
                type = type
            });
        }
    }
}
