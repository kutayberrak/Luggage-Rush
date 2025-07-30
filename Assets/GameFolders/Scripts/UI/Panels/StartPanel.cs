using DG.Tweening;
using GameFolders.Scripts.Interfaces;
using UnityEngine;

namespace GameFolders.Scripts.UI.Panels
{
    public class StartPanel : MonoBehaviour,IAnimatedUI
    {
        [Header("Components")] 
        [SerializeField] private RectTransform navBarButtons;
        [SerializeField] private RectTransform startButton;
        [Header("Settings")]
        [SerializeField] private float buttonsClosedPosition; 
        [SerializeField] private float buttonsOpenPosition;
        [SerializeField] private float startButtonClosedPos = -150f;
        [SerializeField] private float startButtonOpenPos = 477f;
        
        private void OnEnable()
        {
            GameEvents.OnGameStart += DeactivatePanel;
        }
        private void OnDisable()
        {
            GameEvents.OnGameStart -= DeactivatePanel;
        }
        public void DeactivatePanel()
        {
            navBarButtons.DOAnchorPos(new Vector2(navBarButtons.anchoredPosition.x,buttonsClosedPosition), 0.5f);
            startButton.DOAnchorPosY(startButtonClosedPos, 0.5f);
        }

        public void ActivatePanel()
        {
            navBarButtons.DOAnchorPos(new Vector2(navBarButtons.anchoredPosition.x,buttonsOpenPosition), 0.5f);
            startButton.DOAnchorPosY(startButtonOpenPos, 0.5f);
        }
    }
}
