using GameFolders.Scripts.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameFolders.Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private LevelDataSO[] levelData;
        public static GameManager Instance { get; private set; }
        public LevelDataSO CurrentLevelData => _currentLevelData;
        public int CurrentLevel => _currentLevelIndex; // Assuming levels are 1-indexed for display purposes

        [SerializeField] private GameObject WinPanel;
        [SerializeField] private GameObject FailPanel;

        [SerializeField] private TextMeshProUGUI levelText;

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
        }

        private void OnDisable()
        {
            GameEvents.OnLevelFailed -= OnLevelFailed;
            GameEvents.OnLevelWin -= OnLevelWin;

        }

        private void Update()
        {
            // Check for A key press to trigger OnLevelWin
            if (Input.GetKeyDown(KeyCode.A))
            {
                // Trigger level win event for all items to return to pool
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
            //load level data...
            _currentLevelData = levelData[_currentLevelIndex];


            SpawnManager.Instance.LoadLevelSpawnRequirements();

            //... can load managers's requirements here
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
            Debug.Log("Level Failed");

            // Play level fail sound
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX("LevelFail");

            FailPanel.SetActive(true);
            ClearLevelData();
        }

        public void OnLevelWin()
        {
            WinPanel.SetActive(true);
            Debug.Log("Level Won!");


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
                Debug.Log("Earned 10 coins!");

                // Play money earn sound
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX("MoneyEarn");
            }
            else
            {
                Debug.LogError("MoneyManager.Instance is null! Make sure MoneyManager exists in the scene.");
            }
        }
        public void NextLevel()
        {
            Debug.Log("Moving to next level...");

            // Move to next level
            LevelUp();

            // Start the new level
            StartLevel();

            WinPanel.SetActive(false);
        }

        public void RestartLevel()
        {
            Debug.Log("Restarting current level...");

            // Clear current level data
            ClearLevelData();

            // Close panels
            FailPanel.SetActive(false);

            // Restart the same level
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
