using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SkrilStudio
{
    public class DemoSlider : MonoBehaviour
    {
        public Text volumeText;
        public AudioSource[] allAudioSource;
        public Slider slider;
        private AudioSource audioSource;
        private bool buttonHold;
        void Start()
        {
            audioSource = gameObject.GetComponent<AudioSource>();
        }
        public void Sliders(bool button) // check if slider button is holded down
        {
            buttonHold = button;
            if (!buttonHold)
            {
                audioSource.Stop(); // stop slider sound when handle is released
            }
            for (int i = 0; i < allAudioSource.Length; i++) // change all button sounds volume when just point clicked a value for the slider
            {
                allAudioSource[i].volume = slider.value;
            }
            volumeText.text = "Volume: " + Mathf.Round(slider.value * 10) / 10; // change slider's volume text and also round the value when just point clicked a value for the slider
        }

        public void SliderMoving() // play sound while slider button is changing values
        {
            if (buttonHold)
            {
                if (!audioSource.isPlaying)
                    audioSource.Play();
                for (int i = 0; i < allAudioSource.Length; i++) // also change all button sounds volume while handle is moving
                {
                    allAudioSource[i].volume = slider.value;
                }
                volumeText.text = "Volume: " + Mathf.Round(slider.value * 10) / 10; // change slider's volume text and also round the value
            }
        }
    }
}
