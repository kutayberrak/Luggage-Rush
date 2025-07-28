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
                    OnTimerEnd?.Invoke();
                }
            }
        }
        [Button("Start Timer")]
        public void StartTimer()
        {
            _isTimerRunning = true;
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
        private string TimerText()
        {
            int minutes = Mathf.FloorToInt(_currentTime / 60f);
            int seconds = Mathf.FloorToInt(_currentTime % 60f);
            string timeFormatted = string.Format("{0:00}:{1:00}", minutes, seconds);
            return timeFormatted;
        }
    }
}
