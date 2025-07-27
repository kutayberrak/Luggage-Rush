using GameFolders.Scripts.ScriptableObjects;
using UnityEngine;

namespace GameFolders.Scripts.Managers
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private LevelDataSO[] levelData;
        public static LevelManager Instance { get; private set; }
        public LevelDataSO CurrentLevelData => _currentLevelData;
        public int CurrentLevel => _currentLevelIndex + 1; // Assuming levels are 1-indexed for display purposes

        private int _currentLevelIndex = 0;
        private LevelDataSO _currentLevelData;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void LevelUp()
        {
            _currentLevelIndex++;
        }

        private void LoadLevelRequirements()
        {
            //load level data...
            _currentLevelData = levelData[_currentLevelIndex];

            SpawnManager.Instance.LoadLevelSpawnRequirements();

            //... can load managers's requirements here

        }

        public void StartLevel()
        {
            LoadLevelRequirements();
            SpawnManager.Instance.RunSpawn();
        }
    }
}
