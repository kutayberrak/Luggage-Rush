using TMPro;
using UnityEngine;

public class FpsSettings : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;

    private float deltaTime;

    void Start()
    {
        SetTargetFrameRate();
    }

    void Update()
    {
        // Ortalama FPS hesaplama (daha stabil görünüm için deltaTime üzerinden)
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;

        if (fpsText != null)
            fpsText.text = $"FPS: {Mathf.CeilToInt(fps)}";
    }

    private void SetTargetFrameRate()
    {
        int refreshRate = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.numerator /
                                           Screen.currentResolution.refreshRateRatio.denominator);
        Application.targetFrameRate = refreshRate;
        Debug.Log($"Target FPS set to: {refreshRate}");
    }
}
