using UnityEngine;

public class ButtonAudio : MonoBehaviour
{
    public void ClickSound()
    {
        // Play button click sound
        AudioManager.Instance.PlaySFX("ButtonClickSFX");

    }

}
