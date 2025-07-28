using GameFolders.Scripts.ScriptableObjects;
using GameFolders.Scripts;
using UnityEngine;

namespace GameFolders.Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private LevelDataSO[] levelData;
        public static GameManager Instance { get; private set; }
        public LevelDataSO CurrentLevelData => _currentLevelData;
        public int CurrentLevel => _currentLevelIndex; // Assuming levels are 1-indexed for display purposes

        private int _currentLevelIndex = 0;
        private LevelDataSO _currentLevelData;

        private const string LEVEL_INDEX_KEY = "CurrentLevelIndex";

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            LoadLevelIndex();


        }

        private void OnEnable()
        {
            if (Timer.Instance != null)
                Timer.Instance.OnTimerEnd += OnLevelFailed;
        }

        private void OnDisable()
        {
            if (Timer.Instance != null)
                Timer.Instance.OnTimerEnd -= OnLevelFailed;
        }

        public void LevelUp()
        {
            _currentLevelIndex++;
            SaveLevelIndex();
        }

        private void LoadLevelIndex()
        {
            _currentLevelIndex = PlayerPrefs.GetInt(LEVEL_INDEX_KEY, 0);

        }

        private void SaveLevelIndex()
        {
            PlayerPrefs.SetInt(LEVEL_INDEX_KEY, _currentLevelIndex);
            PlayerPrefs.Save();
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



            Timer.Instance.SetTimer(_currentLevelData.TimeInSeconds);
            Timer.Instance.StartTimer();
        }

        private void OnLevelFailed()
        {
            Debug.Log("Level Failed");

            // Stop spawning when level fails
            SpawnManager.Instance.StopSpawning();
        }
    }
}
