using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WinPanelAnimator : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform wellDoneImage;
    public RectTransform collectedPanel;
    public RectTransform continueButton;
    public RectTransform x2PrizeButton;

    [Header("Animation Settings")]
    public float animationDuration = 0.6f;
    public float staggerDelay = 0.2f;
    public Ease animationEase = Ease.OutBack;

    private Vector3[] originalPositions;
    private Vector3[] originalScales;

    private void Awake()
    {
        StoreOriginalValues();
    }
    private void Start()
    {
        // Store original values
        
    }

    private void StoreOriginalValues()
    {
        originalPositions = new Vector3[4];
        originalScales = new Vector3[4];

        // WellDone Image
        originalPositions[0] = wellDoneImage.anchoredPosition;
        originalScales[0] = wellDoneImage.localScale;

        // Collected Panel
        originalPositions[1] = collectedPanel.anchoredPosition;
        originalScales[1] = collectedPanel.localScale;

        // Continue Button
        originalPositions[2] = continueButton.anchoredPosition;
        originalScales[2] = continueButton.localScale;

        // X2Prize Button
        originalPositions[3] = x2PrizeButton.anchoredPosition;
        originalScales[3] = x2PrizeButton.localScale;
    }

    public void PlayWinAnimation()
    {
        // Reset first
        ResetAnimation();

        // WellDone Image - Scale from small to normal
        wellDoneImage.localScale = Vector3.zero;

        wellDoneImage.DOScale(originalScales[0], animationDuration)
            .SetEase(animationEase);

        // Collected Panel - Slide from top
        collectedPanel.anchoredPosition = originalPositions[1] + Vector3.up * 200f;

        collectedPanel.DOAnchorPos(originalPositions[1], animationDuration)
            .SetDelay(staggerDelay)
            .SetEase(animationEase);

        // Continue Button - Slide from right + Scale bounce
        continueButton.anchoredPosition = originalPositions[2] + Vector3.right * 300f;
        continueButton.localScale = Vector3.zero;

        continueButton.DOAnchorPos(originalPositions[2], animationDuration)
            .SetDelay(staggerDelay * 2)
            .SetEase(animationEase);
        continueButton.DOScale(originalScales[2], animationDuration)
            .SetDelay(staggerDelay * 2)
            .SetEase(animationEase);

        // X2Prize Button - Slide from left + Scale bounce
        x2PrizeButton.anchoredPosition = originalPositions[3] + Vector3.left * 300f;
        x2PrizeButton.localScale = Vector3.zero;

        x2PrizeButton.DOAnchorPos(originalPositions[3], animationDuration)
            .SetDelay(staggerDelay * 3)
            .SetEase(animationEase);
        x2PrizeButton.DOScale(originalScales[3], animationDuration)
            .SetDelay(staggerDelay * 3)
            .SetEase(animationEase);
    }


    public void ResetAnimation()
    {
        // Kill all active tweens
        DOTween.Kill(wellDoneImage);
        DOTween.Kill(collectedPanel);
        DOTween.Kill(continueButton);
        DOTween.Kill(x2PrizeButton);

        // Reset to original values
        wellDoneImage.anchoredPosition = originalPositions[0];
        wellDoneImage.localScale = originalScales[0];

        collectedPanel.anchoredPosition = originalPositions[1];
        collectedPanel.localScale = originalScales[1];

        continueButton.anchoredPosition = originalPositions[2];
        continueButton.localScale = originalScales[2];

        x2PrizeButton.anchoredPosition = originalPositions[3];
        x2PrizeButton.localScale = originalScales[3];
    }

    public void HideAllElements()
    {
        // Hide all elements instantly by setting scale to zero
        wellDoneImage.localScale = Vector3.zero;
        collectedPanel.localScale = Vector3.zero;
        continueButton.localScale = Vector3.zero;
        x2PrizeButton.localScale = Vector3.zero;
    }

    private void OnDisable()
    {
        ResetAnimation();
    }
}