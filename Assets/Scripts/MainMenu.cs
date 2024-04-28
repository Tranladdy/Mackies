using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public AudioSource audioSound;
    public void playButton() {
        audioSound.Play();
    }

    private void OnCollisionEnter2D(Collision2D obj)
    {
        if(obj.gameObject.CompareTag("Scale"))
        {
            audioSound.Play();
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}
