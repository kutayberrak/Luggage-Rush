using System;
using GameFolders.Scripts.ScriptableObjects;
using UnityEngine;

namespace GameFolders.Scripts
{
    public class Timer : MonoBehaviour
    {
        public static Timer Instance { get; private set; }
        public float CurrentLevelTime => levelData.Time;

        public Action OnTimerStop;
        public Action OnTimerStart;
        public Action OnTimerEnd;
        
        [Header("References")] 
        [SerializeField] private LevelDataSO levelData;
        
        private float _currentTime;
        private bool _isTimerRunning;
        
        private void Awake()
        {
            Instance = this;
        }
        private void OnEnable()
        {
            OnTimerStart += StartTimer;
            OnTimerStop += StopTimer;
        }

        private void Update()
        {
            if (_isTimerRunning)
            {
                _currentTime += Time.deltaTime;
                if (_currentTime >= levelData.Time)
                {
                    _isTimerRunning = false;
                    OnTimerEnd?.Invoke();
                    _currentTime = 0f;
                }
            }
        }

        private void OnDisable()
        {
            OnTimerStart -= StartTimer;
            OnTimerStop -= StopTimer;
        }

        public void StartTimer()
        {
            OnTimerStart?.Invoke();
            _isTimerRunning = true; 
        }

        public void StopTimer()
        {
            OnTimerStop?.Invoke();
            _isTimerRunning = false;
        }
    }
}
