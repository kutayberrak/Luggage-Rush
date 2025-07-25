using UnityEngine;

namespace GameFolders.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "LevelDataSO", menuName = "ScriptableObjects/LevelDataSO")]
    public class LevelDataSO : ScriptableObject
    {
        [Header("Level Information")] 
        [SerializeField] private float levelTime;
        [SerializeField] private BagType targetBagType;
        [SerializeField] private CollectiblePieceType collectiblePieceType;
        [SerializeField] private int targetBagCount;
        [SerializeField] private int junkPieceCount;
        [SerializeField] private bool hasCollectiblePiece;

        public float Time => levelTime;
        public BagType TargetBagType => targetBagType;
        public CollectiblePieceType CollectiblePieceType => collectiblePieceType;
        public bool HasCollectiblePiece => hasCollectiblePiece;
        public int TargetBagCount => targetBagCount;
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
    public enum BagType
    {
        None,
    }
}
