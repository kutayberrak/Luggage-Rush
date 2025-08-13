using System;
using GameFolders.Scripts.Managers;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace GameFolders.Scripts
{
    public class MoveCounter : MonoBehaviour
    {
        [SerializeField] private TMP_Text moveCountText;
        public static MoveCounter Instance { get; private set; }
        public static bool IsMoveBasedLevel { get; private set; } = false;

        public int CurrentMoveCount => _currentMoveCount;
        public int MaxMoves => _maxMoves;
        public int RemainingMoves => _maxMoves - _currentMoveCount;

        public event Action OnMoveCounterStart;
        public event Action OnMoveCounterStop;
        public event Action OnMoveCounterEnd;
        public event Action<int> OnMoveCountChanged;

        private int _currentMoveCount;
        private int _maxMoves;
        private bool _isMoveCounterActive;

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
            OnMoveCounterEnd += GameEvents.TriggerLevelFailed;
        }

        private void OnDisable()
        {
            OnMoveCounterEnd -= GameEvents.TriggerLevelFailed;
        }

        [Button("Start Move Counter")]
        public void StartMoveCounter()
        {
            _isMoveCounterActive = true;
            IsMoveBasedLevel = true;
            _currentMoveCount = 0;
            _maxMoves = GameManager.Instance.CurrentLevelData.MoveLimitCount;
            UpdateMoveCountText();
            OnMoveCounterStart?.Invoke();
        }

        [Button("Stop Move Counter")]
        public void StopMoveCounter()
        {
            _isMoveCounterActive = false;
            IsMoveBasedLevel = false;
            OnMoveCounterStop?.Invoke();
        }

        public void SetMoveLimit(int moveLimit)
        {
            _maxMoves = moveLimit;
            UpdateMoveCountText();
        }

        [Button("Add Move")]
        public void AddMove()
        {
            if (!_isMoveCounterActive) return;

            _currentMoveCount++;
            UpdateMoveCountText();
            OnMoveCountChanged?.Invoke(_currentMoveCount);

            if (_currentMoveCount >= _maxMoves)
            {
                _isMoveCounterActive = false;
                OnMoveCounterEnd?.Invoke();
            }
        }

        private void UpdateMoveCountText()
        {
            if (moveCountText != null)
            {
                moveCountText.text = RemainingMoves.ToString();
            }
        }

        public void ShowUI()
        {
            if (moveCountText != null)
                moveCountText.transform.parent.gameObject.SetActive(true);
        }

        public void HideUI()
        {
            if (moveCountText != null)
                moveCountText.transform.parent.gameObject.SetActive(false);
        }

        // Call this method when player makes a move (click, drag, etc.)
        public void RegisterMove()
        {
            AddMove();
        }
    }
}
