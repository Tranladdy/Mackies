using UnityEngine;

public class CameraSway : MonoBehaviour
{
    public float swayAmount = 0.5f; // Amount of sway
    public float swaySpeed = 1f; // Speed of the sway

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position; // Get the initial position of the background
    }

    void Update()
    {
        // Calculate the sway movement using sine wave
        float sway = Mathf.Sin(Time.time * swaySpeed) * swayAmount;

        // Apply sway to the background's position
        transform.position = startPosition + Vector3.right * sway;
    }
}