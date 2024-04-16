using UnityEngine;

public class RoundManagerCPU : MonoBehaviour
{
    public static RoundManagerCPU Instance { get; private set; }
    public int RoundCount { get; set; } // Property to hold selected rounds

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object alive across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetRounds(int rounds)
    {
        RoundCount = rounds;
    }
}