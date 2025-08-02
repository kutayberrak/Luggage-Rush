using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameFolders.Scripts.Managers
{
    public class SagaMapManager : MonoBehaviour
    {
        [SerializeField] private List<Button> buttons;

        [Header("Button Sprites")]
        [SerializeField] private Sprite lockedLevelSprite;
        [SerializeField] private Sprite completedLevelSprite;
        [SerializeField] private Sprite currentLevelSprite;

        [Header("Button Settings")]
        [SerializeField] private float activeScale = 0.8f;
        [SerializeField] private float inactiveScale = 0.7f;

        private int _currentLevel;

        private void Start()
        {
            _currentLevel = GameManager.Instance.CurrentLevel;
            InitializeButtons();
        }

        private void OnEnable() => GameEvents.OnLevelWin += OnLevelWin;
        private void OnDisable() => GameEvents.OnLevelWin -= OnLevelWin;

        private void InitializeButtons()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                if (i < _currentLevel)
                    SetButtonVisual(buttons[i], completedLevelSprite, inactiveScale, 1f);
                else if (i == _currentLevel)
                    SetButtonVisual(buttons[i], currentLevelSprite, activeScale, 1f);
                else
                    SetButtonVisual(buttons[i], lockedLevelSprite, inactiveScale, 1f);
            }
        }

        private void OnLevelWin()
        {
            int currentLevel = GameManager.Instance.CurrentLevel;
            SetButtonVisual(buttons[currentLevel], completedLevelSprite, inactiveScale, 0.9f);

            if (currentLevel >= buttons.Count - 1) return;

            SetButtonVisual(buttons[currentLevel + 1], currentLevelSprite, activeScale, 1f);
        }

        private void SetButtonVisual(Button button, Sprite sprite, float scale, float alpha)
        {
            Image image = button.GetComponent<Image>();
            image.sprite = sprite;
            button.transform.localScale = Vector3.one * scale;
            image.color = new Color(1, 1, 1, alpha);
        }

        public void StartLevelButton(int levelIndex)
        {
            if (GameManager.Instance.CurrentLevel != levelIndex) return;
            GameEvents.TriggerGameStart();
        }
    }
}