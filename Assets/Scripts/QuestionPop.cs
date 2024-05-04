using UnityEngine;
using System.Collections;

public class QuestionPop : MonoBehaviour
{
    // Reference to the game objects you want to enable
    public GameObject objectToEnable1;
    public GameObject objectToEnable2;
    public GameObject objectToEnable3;
    public GameObject objectToEnable4;
    public GameObject objectToEnable5;

    // Delay time for each object
    public float delayTime1 = 1.0f; // Delay time in seconds for objectToEnable1
    public float delayTime2 = 2.0f; // Delay time in seconds for objectToEnable2
    public float delayTime3 = 3.0f; // Delay time in seconds for objectToEnable3

    public AudioSource audioSound;

    private void OnCollisionEnter2D(Collision2D obj)
    {
        // Check if the collision involves the game object you want to detect
        if (obj.gameObject.CompareTag("Scale"))
        {
            // Start the coroutine to enable each object with the specified delay
            StartCoroutine(EnableObjectWithDelay(objectToEnable1, delayTime1));
            StartCoroutine(EnableObjectWithDelay(objectToEnable2, delayTime2));
            StartCoroutine(EnableObjectWithDelay(objectToEnable3, delayTime3));
            StartCoroutine(EnableObjectWithDelay(objectToEnable4, delayTime3));
            audioSound.Play();
        }
        else if(obj.gameObject.CompareTag("Food"))
        {
            audioSound.Play();
        }
    }

    IEnumerator EnableObjectWithDelay(GameObject objectToEnable, float delayTime)
    {
        // Wait for the specified delay time
        yield return new WaitForSeconds(delayTime);

        // Enable the specified game object after the delay
        objectToEnable.SetActive(true);
    }
}