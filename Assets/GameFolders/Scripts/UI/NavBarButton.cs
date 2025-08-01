using DG.Tweening;
using UnityEngine;

namespace GameFolders.Scripts.UI
{
    public class NavBarButton : MonoBehaviour
    {
        [SerializeField] private RectTransform shopPanel;
        [SerializeField] private RectTransform collectionPanel;
        [SerializeField] private RectTransform screenParent;
        [SerializeField] private RectTransform selectedImage;
        [SerializeField] private float moveSpeed = 0.1f;
        [SerializeField] private Ease moveEase = Ease.Linear;
        
        private float _screenWidth;
        private float _screenHeight;
        private float _selectedImageOffset;
        private void Start()
        {
            _screenWidth = screenParent.parent.GetComponent<RectTransform>().rect.width;
            _selectedImageOffset = _screenWidth / 3f;
            shopPanel.anchoredPosition = new Vector2(-_screenWidth,0f);
            collectionPanel.anchoredPosition = new Vector2(_screenWidth,0f);
        }

        public void SetScreenPos(int screenWidthOffsetMultiplier)
        {
            Vector2 targetPosition = new Vector2(_screenWidth * screenWidthOffsetMultiplier,0f);
            screenParent.DOAnchorPos(targetPosition,moveSpeed).SetEase(moveEase);
        }

        public void SetSelectedImagePos(int multiplier)
        {
            if (selectedImage == null)
                return;
            selectedImage.DOAnchorPosX(_selectedImageOffset * multiplier,moveSpeed).SetEase(moveEase);
        }
    }
}
