using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public AudioSource audio;
    public void playButton() {
        audio.Play();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
