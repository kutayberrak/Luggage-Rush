using DG.Tweening;
using GameFolders.Scripts.Interfaces;
using UnityEngine;

namespace GameFolders.Scripts.UI.Panels
{
    public class MainMenuPanel : MonoBehaviour,IAnimatedUI
    {
        [Header("Components")] 
        [SerializeField] private RectTransform navBarButtons;
        [SerializeField] private RectTransform startButton;
        [Header("Settings")]
        [SerializeField] private RectTransform buttonsClosedPosition; 
        [SerializeField] private RectTransform buttonsOpenPosition;
        [SerializeField] private RectTransform startButtonClosedPos;
        [SerializeField] private RectTransform startButtonOpenPos;
        
        public void DeactivatePanel()
        {
            navBarButtons.DOAnchorPos(new Vector2(navBarButtons.anchoredPosition.x,buttonsClosedPosition.anchoredPosition.y), 0.5f);
            startButton.DOAnchorPosY(startButtonClosedPos.anchoredPosition.y, 0.5f);
        }

        public void ActivatePanel()
        {
            navBarButtons.DOAnchorPos(new Vector2(navBarButtons.anchoredPosition.x,buttonsOpenPosition.pivot.y), 0.5f);
            startButton.DOAnchorPosY(startButtonOpenPos.anchoredPosition.y, 0.5f);
        }
    }
}
