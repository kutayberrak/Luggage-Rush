using GameFolders.Scripts.ScriptableObjects;
using UnityEngine;
using TMPro;

namespace GameFolders.Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private LevelDataSO[] levelData;
        [SerializeField] private TextMeshProUGUI levelText;
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
            GameEvents.OnLevelFailed += OnLevelFailed;
            GameEvents.OnLevelWin += OnLevelWin;
            GameEvents.OnLevelRestarted += OnRestartLevel;
        }
        private void OnDisable()
        {
            GameEvents.OnLevelFailed -= OnLevelFailed;
            GameEvents.OnLevelWin -= OnLevelWin;
            GameEvents.OnLevelRestarted -= OnRestartLevel;
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                GameEvents.TriggerLevelWin();
            }
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
            _currentLevelData = levelData[_currentLevelIndex];
            SpawnManager.Instance.LoadLevelSpawnRequirements();
        }

        public void StartLevel()
        {
            GameEvents.TriggerGameStart();
            ShowLevelText();
            LoadLevelRequirements();

            SpawnManager.Instance.RunSpawn();

            SlotManager.Instance.ClearAllSlots();
            InGameUIManager.Instance.InitializeObjectivesUI();
            Timer.Instance.SetTimer(_currentLevelData.TimeInSeconds);
            Timer.Instance.StartTimer();
        }

        private void ShowLevelText()
        {
            if (levelText != null)
            {
                levelText.text = $"Level {_currentLevelIndex + 1}";
            }
            else
            {
                Debug.LogWarning("Level Text is not assigned in GameManager.");
            }
        }
        private void OnLevelFailed()
        {
            // Play level fail sound
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX("LevelFail");
            ClearLevelData();
        }

        public void OnLevelWin()
        {
            LevelUp();

            //burada o leveldaki collectionları toplamış oluyoruz.
            foreach (var collection in _currentLevelData.CollectablePieceType)
            {
                CollectionManager.Instance.UnlockCollection(collection);
            }

            // Play level win sound
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX("LevelWin");

            ClearLevelData();

            // Earn money
            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.EarnMoney(10);
                // Play money earn sound
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX("MoneyEarn");
            }
            else
            {
                Debug.LogError("MoneyManager.Instance is null! Make sure MoneyManager exists in the scene.");
            }
        }
        private void OnRestartLevel()
        {
            ClearLevelData();
            StartLevel();
        }
        private void ClearLevelData()
        {
            SpawnManager.Instance.StopSpawning();
            SlotManager.Instance.ClearAllSlots();
            Timer.Instance.StopTimer();
        }
    }
}
