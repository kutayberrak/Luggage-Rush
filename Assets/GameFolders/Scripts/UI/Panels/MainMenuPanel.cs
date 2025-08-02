using DG.Tweening;
using GameFolders.Scripts.Interfaces;
using UnityEngine;

namespace GameFolders.Scripts.UI.Panels
{
    public class MainMenuPanel : MonoBehaviour,IAnimatedUI
    {
        [Header("Components")] 
        [SerializeField] private RectTransform navBarButtons;
        
        [Header("Settings")]
        [SerializeField] private RectTransform buttonsClosedPosition; 
        [SerializeField] private RectTransform buttonsOpenPosition;
        
        public void DeactivatePanel()
        {
            navBarButtons.DOAnchorPos(new Vector2(navBarButtons.anchoredPosition.x,buttonsClosedPosition.anchoredPosition.y), 0.5f);
        }

        public void ActivatePanel()
        {
            navBarButtons.DOAnchorPos(new Vector2(navBarButtons.anchoredPosition.x,buttonsOpenPosition.pivot.y), 0.5f);
        }
    }
}
