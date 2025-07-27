using System;
using GameFolders.Scripts.ScriptableObjects;
using UnityEngine;

namespace GameFolders.Scripts
{
    public class Timer : MonoBehaviour
    {
        //[Header("References")]
        //[SerializeField] private LevelDataSO levelData;

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
                if (_currentTime <= 0f)
                {
                    _currentTime = 0f;
                    _isTimerRunning = false;
                    OnTimerEnd?.Invoke();
                }
            }
        }
        public void StartTimer()
        {
            _isTimerRunning = true;
            OnTimerStart?.Invoke();
        }
        public void StopTimer()
        {
            _isTimerRunning = false;
            OnTimerStop?.Invoke();
        }
        public void ResetTimer(float timerStartValue)
        {
            _currentTime = timerStartValue;
        }
    }
}
