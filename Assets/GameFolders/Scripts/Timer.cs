using System;
using GameFolders.Scripts.ScriptableObjects;
using UnityEngine;

namespace GameFolders.Scripts
{
    public class Timer : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private LevelDataSO levelData;

        public static Timer Instance { get; private set; }
        public float TimerStartValue => levelData.Time;
        public float CurrentTime => _currentTime;

        public event Action OnTimerStart;
        public event Action OnTimerStop;
        public event Action OnTimerEnd;

        private float _currentTime;
        private bool _isTimerRunning;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
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
            _currentTime = levelData.Time;
            _isTimerRunning = true;
            OnTimerStart?.Invoke();
        }
        public void StopTimer()
        {
            _isTimerRunning = false;
            OnTimerStop?.Invoke();
        }
        public void SetTimer(float time)
        {
            _currentTime = time;
        }
    }
}
