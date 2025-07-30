using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkrilStudio
{
    public class DemoStopAll : MonoBehaviour
    {
        public AudioSource[] allAudioSources;
        public void StopAllSounds() // only used by "Stop All Sounds" gameobject
        {
            for (int i = 0; i < allAudioSources.Length; i++)
            {
                allAudioSources[i].Stop();
            }
        }
    }
}
