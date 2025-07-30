using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SkrilStudio
{
    public class DemoController : MonoBehaviour
    {
        public AudioClip[] audioClips;
        private AudioSource audioSource;
        public int currentId = 0;
        public Text infoText;
        private string _infoText;
        void Start()
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            audioSource.clip = audioClips[currentId]; // get choosed audio
            _infoText = gameObject.name; // save original text
            infoText.text = _infoText + " " + (currentId + 1) + "/" + audioClips.Length; // updated text
        }
        public void Previous() // previous clip
        {
            if (currentId != 0)
                currentId -= 1;
            else
                currentId = audioClips.Length - 1;
            audioSource.clip = audioClips[currentId]; // get choosed audio
            infoText.text = _infoText + " " + (currentId + 1) + "/" + audioClips.Length; // updated text
        }
        public void Next() // next clip
        {
            if (currentId != audioClips.Length - 1)
                currentId += 1;
            else
                currentId = 0;
            audioSource.clip = audioClips[currentId]; // get choosed audio
            infoText.text = _infoText + " " + (currentId + 1) + "/" + audioClips.Length; // updated text
        }
    }
}
