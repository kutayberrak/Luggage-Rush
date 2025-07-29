using DG.Tweening;
using UnityEngine;

namespace GameFolders.Scripts.UI
{
    public class NavBarButton : MonoBehaviour
    {
        [SerializeField] private RectTransform screenParent;
        [SerializeField] private float moveSpeed = 0.1f;
        [SerializeField] private Ease moveEase = Ease.Linear;
        private float _screenWidth;
        private void Start()
        {
            _screenWidth = Screen.width;
        }

        public void SetScreenPos(int screenWidthOffsetMultiplier)
        {
            Vector2 targetPosition = new Vector2(_screenWidth * screenWidthOffsetMultiplier,0f);
            screenParent.DOAnchorPos(targetPosition,moveSpeed).SetEase(moveEase);
        }
    }
}
