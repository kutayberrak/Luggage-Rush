using System;
using GameFolders.Scripts.Managers;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace GameFolders.Scripts
{
    public class Timer : MonoBehaviour
    {
        [SerializeField] private TMP_Text timerText;
        public static Timer Instance { get; private set; }
        public float CurrentTime => _currentTime;
        public event Action OnTimerStart;
        public event Action OnTimerStop;
        public event Action OnTimerEnd;

        private float _currentTime;
        private bool _isTimerRunning;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void OnEnable()
        {
            OnTimerEnd += GameEvents.TriggerLevelFailed;
        }
        private void OnDisable()
        {
            OnTimerEnd -= GameEvents.TriggerLevelFailed;
        }
        private void Update()
        {
            if (_isTimerRunning)
            {
                _currentTime -= Time.deltaTime;
                timerText.text = TimerText();
                if (_currentTime <= 0f)
                {
                    _currentTime = 0f;
                    _isTimerRunning = false;
                    timerText.text = TimerText();
                    OnTimerEnd?.Invoke();
                }
            }
        }
        [Button("Start Timer")]
        public void StartTimer()
        {
            // Only start timer for time-based levels
            if (GameManager.Instance?.CurrentLevelData?.IsTimeBased != true)
            {
                Debug.LogWarning("Timer can only be started for time-based levels.");
                return;
            }
            _isTimerRunning = true;
            _currentTime = GameManager.Instance.CurrentLevelData.TimeInSeconds;
            OnTimerStart?.Invoke();

        }

        [Button("Stop Timer")]
        public void StopTimer()
        {
            _isTimerRunning = false;
            OnTimerStop?.Invoke();
        }
        public void SetTimer(float timeInSeconds)
        {
            _currentTime = timeInSeconds;
        }

        public void ShowUI()
        {
            if (timerText != null)
                timerText.transform.parent.gameObject.SetActive(true);
        }

        public void HideUI()
        {
            if (timerText != null)
                timerText.transform.parent.gameObject.SetActive(false);
        }

        private string TimerText()
        {
            int minutes = Mathf.FloorToInt(_currentTime / 60f);
            int seconds = Mathf.FloorToInt(_currentTime % 60f);
            string timeFormatted = $"{minutes:00}:{seconds:00}";
            return timeFormatted;
        }

        public void AddTime(float timeInSeconds)
        {
            _currentTime += timeInSeconds;

            timerText.text = TimerText();
        }

        public void RemoveTime(float seconds)
        {
            _currentTime -= seconds;
            if (_currentTime < 0f)
                _currentTime = 0f;

            timerText.text = TimerText();
        }

    }
}
