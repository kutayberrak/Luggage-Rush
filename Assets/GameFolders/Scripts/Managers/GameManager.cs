using GameFolders.Scripts.ScriptableObjects;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GameFolders.Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private LevelDataSO[] levelData;
        [SerializeField] private TextMeshProUGUI levelText;
        public LevelDataSO CurrentLevelData => _currentLevelData;
        public int CurrentLevel => _currentLevelIndex;

        private int _currentLevelIndex = 0;
        private LevelDataSO _currentLevelData;

        private const string LEVEL_INDEX_KEY = "CurrentLevelIndex";

        [Header("Conveyor Adjustment")]
        [SerializeField] private GameObject[] conveyors;
        private GameObject _activeConveyor;

        [Header("Test")]
        public GameObject poleObj;

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
            GameEvents.OnGameStart += StartLevel;
            GameEvents.OnReturnToMainMenu += ClearLevelData;
        }
        private void OnDisable()
        {
            GameEvents.OnLevelFailed -= OnLevelFailed;
            GameEvents.OnLevelWin -= OnLevelWin;
            GameEvents.OnLevelRestarted -= OnRestartLevel;
            GameEvents.OnGameStart -= StartLevel;
            GameEvents.OnReturnToMainMenu -= ClearLevelData;
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

        private void StartLevel()
        {
            ShowLevelText();
            LoadLevelRequirements();

            AdjustConveyor();


            //this function added for testing it will remove before actually game.
            ShowPoleForTest();

            SpawnManager.Instance.RefreshActiveSpawnPoints();
            SpawnManager.Instance.RunSpawn();

            SlotManager.Instance.ClearAllSlots();
            InGameUIManager.Instance.InitializeObjectivesUI();

            // Start appropriate limit system based on level type
            if (_currentLevelData.IsTimeBased)
            {
                // Timer UI'ını göster, MoveCounter UI'ını gizle
                Timer.Instance.ShowUI();
                if (MoveCounter.Instance != null)
                {
                    MoveCounter.Instance.HideUI();
                    MoveCounter.Instance.StopMoveCounter();
                }

                Timer.Instance.SetTimer(_currentLevelData.TimeInSeconds);
                Timer.Instance.StartTimer();
            }
            else if (_currentLevelData.IsMoveBased)
            {
                // MoveCounter UI'ını göster, Timer UI'ını gizle
                if (MoveCounter.Instance != null)
                {
                    MoveCounter.Instance.ShowUI();
                    MoveCounter.Instance.SetMoveLimit(_currentLevelData.MoveLimitCount);
                    MoveCounter.Instance.StartMoveCounter();
                }

                Timer.Instance.HideUI();
                Timer.Instance.StopTimer();
            }
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
            {
                AudioManager.Instance.PlaySFX("LevelFail");
                AudioManager.Instance.TriggerHeavyVibration();
            }
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
            {
                AudioManager.Instance.PlaySFX("LevelWin");
                AudioManager.Instance.TriggerHeavyVibration();
            }

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
            if (_activeConveyor != null)
            {
                _activeConveyor.SetActive(false);
                _activeConveyor = null;
            }

            SpawnManager.Instance.StopSpawning();
            SlotManager.Instance.ClearAllSlots();

            // Stop and hide Timer
            Timer.Instance.StopTimer();
            Timer.Instance.HideUI();

            // Stop and hide MoveCounter if it exists
            if (MoveCounter.Instance != null)
            {
                MoveCounter.Instance.StopMoveCounter();
                MoveCounter.Instance.HideUI();
            }
        }

        private void AdjustConveyor()
        {
            foreach (var c in conveyors)
            {
                if (c != null) c.SetActive(false);
            }

            int index = (int)_currentLevelData.ConveyorType;
            if (index >= 0 && index < conveyors.Length && conveyors[index] != null)
            {
                conveyors[index].SetActive(true);
                _activeConveyor = conveyors[index];
            }
            else
            {
                Debug.LogError($"Conveyor bulunamadı: {_currentLevelData.ConveyorType}");
            }
        }
        public List<ConveyorBeltController> GetActiveConveyors()
        {
            var list = new List<ConveyorBeltController>();
            if (_activeConveyor != null)
            {
                list.AddRange(_activeConveyor.GetComponentsInChildren<ConveyorBeltController>(true));
            }
            return list;
        }

        private void ShowPoleForTest()
        {
            if (CurrentLevelData.name.Equals("France_Level_3_Data"))
            {
                poleObj.SetActive(true);
            }
            else
            {
                poleObj.SetActive(false);
            }
        }

    }
}
