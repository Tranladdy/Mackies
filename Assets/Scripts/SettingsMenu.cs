using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public void SetMusic (float music) {
        Debug.Log(music);
        audioMixer.SetFloat("music", music);
    }
    public void SetSFX (float sound) {
        Debug.Log(sound);
        audioMixer.SetFloat("sound", sound);
    }
    public void SetMaster (float master) {
        Debug.Log(master);
        audioMixer.SetFloat("master", master);
    }
}
