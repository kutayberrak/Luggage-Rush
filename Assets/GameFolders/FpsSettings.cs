using TMPro;
using UnityEngine;

public class FpsSettings : MonoBehaviour
{
    void Start()
    {
        SetTargetFrameRate();
    }

    private void SetTargetFrameRate()
    {
        int refreshRate = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.numerator /
                                           Screen.currentResolution.refreshRateRatio.denominator);
        Application.targetFrameRate = refreshRate;
    }
}
