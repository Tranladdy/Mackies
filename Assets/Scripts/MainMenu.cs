using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public AudioSource audioSound;
    public void playButton() {
        audioSound.Play();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
