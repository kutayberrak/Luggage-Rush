using System;
using GameFolders.Scripts.ScriptableObjects;
using UnityEngine;

namespace GameFolders.Scripts
{
    public class Timer : MonoBehaviour
    {
        public static Timer Instance { get; private set; }
        public float CurrentLevelTime => levelData.Time;

        public Action OnTimerEnd;
        public Action OnTimerStart;
        
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
            OnTimerEnd += StopTimer;
        }

        private void Update()
        {
            if (_isTimerRunning)
            {
                _currentTime += Time.deltaTime;
                if (_currentTime >= levelData.Time)
                {
                    _isTimerRunning = false;
                    _currentTime = 0f;
                    OnTimerEnd?.Invoke();
                }
            }
        }

        private void OnDisable()
        {
            OnTimerStart -= StartTimer;
            OnTimerEnd -= StopTimer;
        }

        public void StartTimer() => _isTimerRunning = true;
        public void StopTimer() => _isTimerRunning = false;
    }
}
