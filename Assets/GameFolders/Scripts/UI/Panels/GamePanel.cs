using DG.Tweening;
using GameFolders.Scripts.Interfaces;
using UnityEngine;

namespace GameFolders.Scripts.UI.Panels
{
    public class GamePanel : MonoBehaviour, IAnimatedUI
    {
        [Header("Settings")] 
        [SerializeField] private float upperPanelClosedPosition;
        [SerializeField] private float upperPanelOpenPosition;
        [SerializeField] private float lowerPanelClosedPosition;
        [SerializeField] private float lowerPanelOpenPosition;
        [SerializeField] private float slotsClosedPosition;
        [SerializeField] private float slotsOpenPosition;
        
        [Header("Components")]
        [SerializeField] private RectTransform upperPanel;
        [SerializeField] private RectTransform lowerPanel;
        [SerializeField] private RectTransform slots;
        
        public void DeactivatePanel()
        {
            upperPanel.DOAnchorPos(new Vector2(upperPanel.anchoredPosition.x, upperPanelClosedPosition), 0.5f);
            lowerPanel.DOAnchorPos(new Vector2(lowerPanel.anchoredPosition.x, lowerPanelClosedPosition), 0.5f);
            slots.DOAnchorPos(new Vector2(slots.anchoredPosition.x, slotsClosedPosition), 0.5f);
        }

        public void ActivatePanel()
        {
            upperPanel.DOAnchorPos(new Vector2(upperPanel.anchoredPosition.x, upperPanelOpenPosition), 0.5f);
            lowerPanel.DOAnchorPos(new Vector2(lowerPanel.anchoredPosition.x, lowerPanelOpenPosition), 0.5f);
            slots.DOAnchorPos(new Vector2(slots.anchoredPosition.x, slotsOpenPosition), 0.5f);
        }
    }
}