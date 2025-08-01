using DG.Tweening;
using GameFolders.Scripts.Interfaces;
using UnityEngine;

namespace GameFolders.Scripts.UI.Panels
{
    public class GamePanel : MonoBehaviour, IAnimatedUI
    {
        [Header("Settings")] 
        [SerializeField] private RectTransform upperPanelClosedPosition;
        [SerializeField] private RectTransform upperPanelOpenPosition;
        [SerializeField] private RectTransform lowerPanelClosedPosition;
        [SerializeField] private RectTransform lowerPanelOpenPosition;
        
        [Header("Components")]
        [SerializeField] private RectTransform upperPanel;
        [SerializeField] private RectTransform lowerPanel;
        
        public void DeactivatePanel()
        {
            upperPanel.DOAnchorPos(new Vector2(upperPanel.anchoredPosition.x, upperPanelClosedPosition.anchoredPosition.y), 0.5f);
            lowerPanel.DOAnchorPos(new Vector2(lowerPanel.anchoredPosition.x, lowerPanelClosedPosition.anchoredPosition.y), 0.5f);
        }

        public void ActivatePanel()
        {
            upperPanel.DOAnchorPos(new Vector2(upperPanel.anchoredPosition.x, upperPanelOpenPosition.anchoredPosition.y), 0.5f);
            lowerPanel.DOAnchorPos(
                new Vector2(lowerPanel.anchoredPosition.x, lowerPanelOpenPosition.pivot.y), 0.5f);
        }
    }
}