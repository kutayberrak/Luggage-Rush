using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FailPanelAnimator : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform failText;
    public RectTransform closeButton;
    public RectTransform failImage;
    public RectTransform playon100Button;
    public RectTransform playon60secButton;

    [Header("Animation Settings")]
    public float animationDuration = 0.8f;
    public float staggerDelay = 0.25f;
    public Ease animationEase = Ease.OutQuart;
    public Ease bounceEase = Ease.OutBounce;

    private Vector3[] originalPositions;
    private Vector3[] originalScales;

    private void Awake()
    {
        StoreOriginalValues();
    }

    private void StoreOriginalValues()
    {
        originalPositions = new Vector3[5];
        originalScales = new Vector3[5];

        // Fail Text
        originalPositions[0] = failText.anchoredPosition;
        originalScales[0] = failText.localScale;

        // Close Button
        originalPositions[1] = closeButton.anchoredPosition;
        originalScales[1] = closeButton.localScale;

        // Fail Image
        originalPositions[2] = failImage.anchoredPosition;
        originalScales[2] = failImage.localScale;

        // Playon100 Button
        originalPositions[3] = playon100Button.anchoredPosition;
        originalScales[3] = playon100Button.localScale;

        // Playon60sec Button
        originalPositions[4] = playon60secButton.anchoredPosition;
        originalScales[4] = playon60secButton.localScale;
    }

    public void PlayFailAnimation()
    {
        // Reset first
        ResetAnimation();

        // Fail Text - Simple slide from left
        failText.anchoredPosition = originalPositions[0] + Vector3.left * 400f;
        failText.localScale = Vector3.zero;

        failText.DOAnchorPos(originalPositions[0], animationDuration)
            .SetEase(Ease.OutQuart);
        failText.DOScale(originalScales[0], animationDuration)
            .SetEase(Ease.OutQuart);

        // Close Button - Simple slide from left
        closeButton.anchoredPosition = originalPositions[1] + Vector3.left * 400f;
        closeButton.localScale = Vector3.zero;

        closeButton.DOAnchorPos(originalPositions[1], animationDuration)
            .SetDelay(staggerDelay)
            .SetEase(Ease.OutQuart);
        closeButton.DOScale(originalScales[1], animationDuration)
            .SetDelay(staggerDelay)
            .SetEase(Ease.OutQuart);

        // Fail Image - Simple slide from left
        failImage.anchoredPosition = originalPositions[2] + Vector3.left * 400f;
        failImage.localScale = Vector3.zero;

        failImage.DOAnchorPos(originalPositions[2], animationDuration)
            .SetDelay(staggerDelay * 2)
            .SetEase(Ease.OutQuart);
        failImage.DOScale(originalScales[2], animationDuration)
            .SetDelay(staggerDelay * 2)
            .SetEase(Ease.OutQuart);

        // Playon100 Button - Simple slide from left
        playon100Button.anchoredPosition = originalPositions[3] + Vector3.left * 400f;
        playon100Button.localScale = Vector3.zero;

        playon100Button.DOAnchorPos(originalPositions[3], animationDuration)
            .SetDelay(staggerDelay * 3)
            .SetEase(Ease.OutQuart);
        playon100Button.DOScale(originalScales[3], animationDuration)
            .SetDelay(staggerDelay * 3)
            .SetEase(Ease.OutQuart);

        // Playon60sec Button - Simple slide from left
        playon60secButton.anchoredPosition = originalPositions[4] + Vector3.left * 400f;
        playon60secButton.localScale = Vector3.zero;

        playon60secButton.DOAnchorPos(originalPositions[4], animationDuration)
            .SetDelay(staggerDelay * 4)
            .SetEase(Ease.OutQuart);
        playon60secButton.DOScale(originalScales[4], animationDuration)
            .SetDelay(staggerDelay * 4)
            .SetEase(Ease.OutQuart);
    }

    public void ResetAnimation()
    {
        // Kill all active tweens
        DOTween.Kill(failText);
        DOTween.Kill(closeButton);
        DOTween.Kill(failImage);
        DOTween.Kill(playon100Button);
        DOTween.Kill(playon60secButton);

        // Reset to original values
        failText.anchoredPosition = originalPositions[0];
        failText.localScale = originalScales[0];
        failText.localRotation = Quaternion.identity;

        closeButton.anchoredPosition = originalPositions[1];
        closeButton.localScale = originalScales[1];
        closeButton.localRotation = Quaternion.identity;

        failImage.anchoredPosition = originalPositions[2];
        failImage.localScale = originalScales[2];
        failImage.localRotation = Quaternion.identity;

        playon100Button.anchoredPosition = originalPositions[3];
        playon100Button.localScale = originalScales[3];
        playon100Button.localRotation = Quaternion.identity;

        playon60secButton.anchoredPosition = originalPositions[4];
        playon60secButton.localScale = originalScales[4];
        playon60secButton.localRotation = Quaternion.identity;
    }

    public void HideAllElements()
    {
        // Hide all elements instantly by setting scale to zero
        failText.localScale = Vector3.zero;
        closeButton.localScale = Vector3.zero;
        failImage.localScale = Vector3.zero;
        playon100Button.localScale = Vector3.zero;
        playon60secButton.localScale = Vector3.zero;
    }

    public void PlayButtonClickAnimation(RectTransform button)
    {
        // Scale down and up animation for button clicks
        button.DOScale(originalScales[GetButtonIndex(button)] * 0.9f, 0.1f)
            .OnComplete(() => {
                button.DOScale(originalScales[GetButtonIndex(button)], 0.1f);
            });
    }

    private int GetButtonIndex(RectTransform button)
    {
        if (button == closeButton) return 1;
        if (button == playon100Button) return 3;
        if (button == playon60secButton) return 4;
        return 0;
    }

    private void OnDisable()
    {
        ResetAnimation();
    }
}

