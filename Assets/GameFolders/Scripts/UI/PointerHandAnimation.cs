using DG.Tweening;
using UnityEngine;

namespace GameFolders.Scripts.UI
{
    public class PointerHandAnimation : MonoBehaviour
    {
        enum MoveDirection
        {
            X,
            Y,
            XY
        }
        
        [Header("Settings")] 
        [SerializeField] private float animationSpeed;
        [SerializeField] private Ease animationEase;
        [SerializeField] private float moveAmount = 0.5f;
        [SerializeField] private MoveDirection moveDirection = MoveDirection.Y;

        private Tween _pointerTween;
        
        private void OnEnable()
        {
            PointerAnimation();
        }

        private void OnDisable()
        {
            if (_pointerTween != null && _pointerTween.IsActive())
            {
                _pointerTween.Kill();
            }
        }
        /// <summary>
        /// To set the pointer animation based on the move direction and amount.
        /// Y for vertical movement, X for horizontal movement, XY for diagonal movement.
        /// </summary>
        private void PointerAnimation()
        {
            _pointerTween = moveDirection switch
            {
                MoveDirection.Y => transform.DOLocalMoveY(transform.localPosition.y + moveAmount, animationSpeed)
                    .SetEase(animationEase)
                    .SetLoops(-1, LoopType.Yoyo),
                MoveDirection.X => transform.DOLocalMoveX(transform.localPosition.x + moveAmount, animationSpeed)
                    .SetEase(animationEase)
                    .SetLoops(-1, LoopType.Yoyo),
                MoveDirection.XY => transform.DOLocalMove(transform.localPosition + new  Vector3(moveAmount, moveAmount, 0), animationSpeed)
                    .SetEase(animationEase)
                    .SetLoops(-1,LoopType.Yoyo),
                _ => _pointerTween
            };
        }
    }
}
