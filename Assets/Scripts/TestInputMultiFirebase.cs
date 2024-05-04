using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Firebase.Storage;
using UnityEngine.Networking;

public class TestInputMultiFirebase : MonoBehaviour
{
    public TMP_InputField userInputField;
    // Reference to the GameManager script
    public TestInputMultiFirebase gameManager;
    public GameObject gameCanvas;
    public GameObject player1VictoryScreen;
    public GameObject player2VictoryScreen;
    public GameObject tieScreen;
    private string defaultResultText = "Waiting...";

    public TMP_ColorGradient player1Gradient;
    public TMP_ColorGradient player2Gradient;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI playerTurn;
    public TextMeshProUGUI proteinText; // Reference to the TextMeshProUGUI for displaying the protein
    public TextMeshProUGUI gramsText; // Reference to the TextMeshProUGUI for displaying the grams
    public TextMeshProUGUI scaleGramsText; // Reference to the TextMeshProUGUI for displaying scale grams
    public TextMeshProUGUI scoreText; // Reference to the TextMeshProUGUI for displaying the score
    public TextMeshProUGUI scoreText2; // Reference to the TextMeshProUGUI for displaying the score2
    public TextMeshProUGUI roundText; // Reference to the TextMeshProUGUI for displaying the round number
   
    public SpriteRenderer foodSpriteRenderer; // Reference to the SpriteRenderer for displaying the food image

    public int player1Score = 0; // Variable to store Player 1's score
    public int player2Score = 0; // Variable to store Player 2's score
    private bool isPlayer1Turn = true; // Variable to track whose turn it is

    private Dictionary<string, float> proteins = new Dictionary<string, float>(); // Dictionary to store proteins and their values
    private Dictionary<int, Sprite> foodImages = new Dictionary<int, Sprite>(); // Dictionary to store food images
    private Dictionary<string, int> foodImagesIds = new Dictionary<string, int>();

    private int currentGrams; // Variable to store the generated grams value
    private string currentProtein; // Variable to store the selected protein
    private int currentImageID; // Variable to store the selected food image ID

    private Vector3 startingPosition; // Variable to store the starting position of the sprite object

    private int maxRounds;  // Added maxRounds variable
    private int round = 1; // Variable to store the current round number

    private const float timerDuration = 10f; // Duration of the timer in seconds

    public AudioSource audioSound;
    public AudioSource dissapointing;
    public AudioSource bad;
    public AudioSource okay;
    public AudioSource good;
    public AudioSource great;
    public AudioSource excellent;
    public AudioSource perfect;

    public TextMeshProUGUI player1AnswerText;
    public TextMeshProUGUI player2AnswerText;

    Color orange = new Color(1f, 0.5f, 0f); // Define the orange color

    private void Start()
    {
        maxRounds = RoundManagerLocal.Instance.RoundCount;
        resultText.text = defaultResultText;
        resultText.color = Color.cyan;
        startingPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1.1f, 10));
        UpdatePlayerTurnText();
        StartCoroutine(InitializeGame());
    }

    private IEnumerator InitializeGame()
    {
        // Start loading proteins from CSV
        yield return StartCoroutine(LoadProteinsFromCSV());

        // Start the main game functionality once data is ready
        StartCoroutine(GenerateChickenBreastCoroutine());
        UpdateRoundText();

        // Now safe to log information if currentProtein has been initialized
        if (!string.IsNullOrEmpty(currentProtein))
        {
            LogFoodInformation();
        }
    }

    private void LogFoodInformation()
    {
        if (!string.IsNullOrEmpty(currentProtein) && proteins.ContainsKey(currentProtein))
        {
            Debug.Log("Protein: " + currentProtein + ", Grams: " + currentGrams + ", Expected Output: " + (proteins[currentProtein] * currentGrams));
        }
        else
        {
            Debug.LogWarning("currentProtein is not set or not found in the dictionary");
        }
    }

    private void UpdateScoreText()
    {
        scoreText.text = "PLAYER 1 SCORE: " + player1Score.ToString();
        scoreText2.text = "PLAYER 2 SCORE: " + player2Score.ToString();
    }

    private void UpdateRoundText()
    {
        roundText.text = "Round: " + round.ToString() + "/" + maxRounds;
        UpdateScoreText();
    }

    public void ValidateUserInput()
    {

        Debug.Log("ValidateUserInput called"); // Add this for debugging
        string input = userInputField.text;
        float userInput;
        bool isValidInput = float.TryParse(input, out userInput);

        if (isValidInput)
        {
            // Stop all running coroutines to reset the game state
            StopAllCoroutines();

            float proteinValue = proteins[currentProtein];
            float expectedOutput = proteinValue * currentGrams;
            float percentageDifference = Mathf.Abs(userInput - expectedOutput) / expectedOutput * 100;

            // Display results and manage game state transitions
            if (isPlayer1Turn)
            {
                player1AnswerText.text = input + "g";
                StartCoroutine(UpdateScorePlayer1(percentageDifference));
                isPlayer1Turn = false;
            }
            else
            {
                player2AnswerText.text = input + "g";
                StartCoroutine(UpdateScorePlayer2(percentageDifference));
                isPlayer1Turn = true;
            }

            // Clear the input field
            userInputField.text = "";
        }
        else
        {
            resultText.text = "Please enter a valid number.";
            resultText.color = Color.red;
        }
    }

    private IEnumerator UpdateScorePlayer1(float percentageDifference)
    {
        Debug.Log($"Player 1 scored with a percentage difference of: {percentageDifference}"); // Debugging line

        string message = "Invalid input";
        Color color = Color.gray;

        if (percentageDifference <= 2.5f)
        {
            player1Score += 12;
            message = "Perfect Player 1!!!! +12 Points";
            color = Color.cyan;
            perfect.Play();
        }
        else if (percentageDifference <= 7f)
        {
            player1Score += 6;
            message = "Excellent Player 1!!! +6 Points";
            color = Color.green;
            excellent.Play();
        }
        else if (percentageDifference <= 9f)
        {
            player1Score += 5;
            message = "Great Job Player 1! +5 Points";
            color = Color.green;
            great.Play();
        }
        else if (percentageDifference <= 30f)
        {
            player1Score += 4;
            message = "Good Job Player 1 +4 Points";
            color = Color.yellow;
            good.Play();
        }
        else if (percentageDifference <= 40f)
        {
            player1Score += 3;
            message = "Unsatisfactory Player 1. +3 Points";
            color = Color.yellow;
            okay.Play();
        }
        else if (percentageDifference <= 50f)
        {
            player1Score += 2;
            message = "Bad Player 1.. +2 Points";
            color = orange;
            bad.Play();
        }
        else if (percentageDifference > 50f)
        {
            message = "Disappointing Player 1...";
            color = Color.red;
            dissapointing.Play();
        }

        resultText.text = message;
        resultText.color = color;
        UpdateScoreText();

        yield return new WaitForSeconds(2f);

        resultText.text = defaultResultText;
        resultText.color = Color.cyan;

        // Transition to the next player or round
        isPlayer1Turn = false; // Switch turn
        UpdatePlayerTurnText();
        if (isPlayer1Turn)
        {
            StartCoroutine(GenerateChickenBreastCoroutine());
        }
        else
        {
            StartCoroutine(TimerCoroutine()); // Restart timer for next player
        }
    }


    private IEnumerator UpdateScorePlayer2(float percentageDifference)
    {
        string message = "Invalid input";
        Color color = Color.gray;

        if (percentageDifference <= 2.5f)
        {
            player2Score += 12;
            message = "Perfect Player 2!!!! +12 Points";
            color = Color.cyan;
            perfect.Play();
        }
        else if (percentageDifference <= 7f)
        {
            player2Score += 6;
            message = "Excellent Player 2!!! +6 Points";
            color = Color.green;
            excellent.Play();
        }
        else if (percentageDifference <= 9f)
        {
            player2Score += 5;
            message = "Great Job Player 2! +5 Points";
            color = Color.green;
            great.Play();
        }
        else if (percentageDifference <= 30f)
        {
            player2Score += 4;
            message = "Good Job Player 2 +4 Points";
            color = Color.yellow;
            good.Play();
        }
        else if (percentageDifference <= 40f)
        {
            player2Score += 3;
            message = "Unsatisfactory Player 2. +3 Points";
            color = Color.yellow;
            okay.Play();
        }
        else if (percentageDifference <= 50f)
        {
            player2Score += 2;
            message = "Bad Player 2.. +2 Points";
            color = orange;
            bad.Play();
        }
        else if (percentageDifference > 50f)
        {
            message = "Disappointing Player 2...";
            color = Color.red;
            dissapointing.Play();
        }

        resultText.text = message;
        resultText.color = color;
        UpdateScoreText();

        yield return new WaitForSeconds(2f);

        // Display the expected result
        DisplayExpectedResult();

        yield return new WaitForSeconds(2f); // Show the expected result for 2 seconds

        resultText.text = defaultResultText;
        resultText.color = Color.cyan;
        proteinText.text = ""; // Clear food name
        gramsText.text = ""; // Clear grams display

        // Check if the current round is the last round
        round++; // Increment round count
        if (round > maxRounds)
        {
            // This is the final round, so we handle the end of the game
            DisplayExpectedResult();

            // Show the expected result for 2 seconds
            yield return new WaitForSeconds(2f); 

            resultText.text = ""; // Clear the text
            proteinText.text = ""; // Clear food name
            gramsText.text = ""; // Clear grams display

            // Call the end of game handling method on the GameManager
            gameManager.HandleEndOfGame();
        }
        else
        {
            // Not the final round, so just prepare for the next round
            isPlayer1Turn = true; // Switch turn back to player 1
            UpdatePlayerTurnText();
            StartCoroutine(GenerateChickenBreastCoroutine()); // Initiate next round
        }
    }

    public void HandleEndOfGame()
    {
        // Deactivate the Game Canvas
        if (gameCanvas != null)
        {
            gameCanvas.SetActive(false);
        }

        // Activate the appropriate victory screen
        if (player1Score > player2Score)
        {
            if (player1VictoryScreen != null)
            {
                player1VictoryScreen.SetActive(true);
            }
        }
        else if (player2Score > player1Score)
        {
            if (player2VictoryScreen != null)
            {
                player2VictoryScreen.SetActive(true);
            }
        }
        else // It's a tie
        {
            if (tieScreen != null)
            {
                tieScreen.SetActive(true);
            }
        }
    }

    private void DisplayExpectedResult()
    {
        float expectedOutput = proteins[currentProtein] * currentGrams;
        resultText.text = "Answer: " + expectedOutput.ToString("F2") + "g";
        resultText.color = Color.cyan;

        // Enable TMP Text Objects
        player1AnswerText.gameObject.SetActive(true);
        player2AnswerText.gameObject.SetActive(true);
    }

    private int GenerateRandomGrams()
    {
        float randomValue = UnityEngine.Random.value;
        if (randomValue < 0.6f)
        {
            return UnityEngine.Random.Range(1, 501);
        }
        else if (randomValue < 0.9f)
        {
            return UnityEngine.Random.Range(501, 751);
        }
        else
        {
            return UnityEngine.Random.Range(751, 1001);
        }
    }

    private IEnumerator GenerateChickenBreastCoroutine()
    {
        // Wait until proteins are loaded
        yield return new WaitUntil(() => proteins.Count > 0);

        UpdateRoundText();

        if (proteins.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, proteins.Count);
            currentProtein = new List<string>(proteins.Keys)[randomIndex];
            currentGrams = GenerateRandomGrams();
            
            yield return new WaitForSeconds(1.5f);
            gramsText.text = currentGrams + "g";  // Display the generated grams in UI
            audioSound.Play();
            scaleGramsText.text = currentGrams + "";  // Update any other UI elements as necessary

            LogFoodInformation();
            yield return new WaitForSeconds(2.5f);
            audioSound.Play();
            proteinText.text = currentProtein + "?";
            
            currentImageID = GetImageID(currentProtein);
            if (currentImageID != -1)
            {
                if (!foodImages.ContainsKey(currentImageID))
                {
                    StartCoroutine(DownloadAndSetImage(currentImageID.ToString()));
                }
                else
                {
                    ActivateFoodSprite(currentImageID);
                }
            }

            StartCoroutine(TimerCoroutine());
        }
    }

    private void SetFoodImage()
    {
        currentImageID = GetImageID(currentProtein);
        if (currentImageID != -1 && foodImages.ContainsKey(currentImageID))
        {
            foodSpriteRenderer.sprite = foodImages[currentImageID];
            // Move the sprite to the starting position above the camera view.
            foodSpriteRenderer.transform.position = startingPosition;
            foodSpriteRenderer.gameObject.SetActive(true);

            // Start the falling animation coroutine
            StartCoroutine(FallIntoPlace(foodSpriteRenderer.transform, new Vector3(startingPosition.x, 0, startingPosition.z), 0.5f));
        }
        else
        {
            Debug.LogError("Food sprite not found for ID: " + currentImageID);
        }
    }

    private IEnumerator FallIntoPlace(Transform objectTransform, Vector3 endPosition, float duration)
    {
        Vector3 startPosition = objectTransform.position;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            objectTransform.position = Vector3.Lerp(startPosition, endPosition, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        objectTransform.position = endPosition;
    }

    private IEnumerator LoadProteinsFromCSV()
    {
        string csvUrl = "https://firebasestorage.googleapis.com/v0/b/mackies-9f1b0.appspot.com/o/FoodInfo.csv?alt=media&token=ae00244e-bd9e-40f8-befc-e584e9f0ea6c"; // URL from Firebase Storage
        UnityWebRequest www = UnityWebRequest.Get(csvUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError($"Failed to download CSV: {www.error}");
        } else {
            ParseCSV(www.downloadHandler.text);
        }
    }

    private void ParseCSV(string csvData)
    {
        StringReader reader = new StringReader(csvData);
        string line = reader.ReadLine();

        while ((line = reader.ReadLine()) != null)
        {
            string[] values = line.Split(',');
            if (values.Length >= 3)
            {
                string foodName = values[0].Trim();
                float proteinValue;
                int imageId;

                if (float.TryParse(values[1].Trim(), out proteinValue) && int.TryParse(values[2].Trim(), out imageId))
                {
                    proteins.Add(foodName, proteinValue);
                    foodImagesIds.Add(foodName, imageId);
                }
            }
        }
    }

    private IEnumerator DownloadAndSetImage(string imageId)
    {
        string accessToken = "636d15af-2cb2-4628-8c0c-bd30fd47629f";
        string imageUrl = $"https://firebasestorage.googleapis.com/v0/b/mackies-9f1b0.appspot.com/o/FoodImages%2F{imageId}.png?alt=media&token={accessToken}";

        Debug.Log("Requesting image from URL: " + imageUrl); // This will log the exact URL

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) 
        {
            Debug.LogError($"Failed to download image ID {imageId}: {www.error}, URL: {imageUrl}");
        } 
        else 
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(www);
            Sprite newSprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            int parsedId = int.Parse(imageId);
            if (parsedId != -1)
            {
                foodImages[parsedId] = newSprite; // Add or replace the sprite in the dictionary

                // Ensure the sprite is set for animation
                SetFoodImage();
            }
        }
    }

    private void ActivateFoodSprite(int imageId)
    {
        if (foodImages.ContainsKey(imageId))
        {
            foodSpriteRenderer.sprite = foodImages[imageId];
            foodSpriteRenderer.gameObject.SetActive(true);
        }
    }

    private int GetImageID(string protein)
    {
        if (foodImagesIds.TryGetValue(protein, out int imageId))
        {
            return imageId;
        }
        return -1; // Return -1 if no matching image ID is found
    }

    private IEnumerator TimerCoroutine()
    {
        float timer = timerDuration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            int roundedTimer = Mathf.CeilToInt(timer);
            timerText.text = roundedTimer.ToString(); // Update the text of the timerText object

            // Update timer color based on remaining time
            UpdateTimerColor(roundedTimer);

            yield return null;
        }

        HandleTimerExpired();
    }

    private void UpdateTimerColor(int timer)
    {
        if (timer >= 8)
            timerText.color = Color.cyan;
        else if (timer >= 6)
            timerText.color = Color.green;
        else if (timer >= 4)
            timerText.color = Color.yellow;
        else
            timerText.color = Color.red;
    }

    private void HandleTimerExpired()
    {
        // Handle when the timer expires
        if (userInputField != null)
        {
            userInputField.text = "0";
            ValidateUserInput();
        }
        else
        {
            Debug.LogWarning("userInputField is null in HandleTimerExpired");
        }
    }

    private void UpdatePlayerTurnText()
    {
        if (isPlayer1Turn)
        {
            playerTurn.text = "Player 1's Turn";
            playerTurn.colorGradientPreset = player1Gradient;
            // Disable TMP Text Objects
            player1AnswerText.gameObject.SetActive(false);
            player2AnswerText.gameObject.SetActive(false);
        }
        else
        {
            playerTurn.text = "Player 2's Turn";
            playerTurn.colorGradientPreset = player2Gradient;
        }
        playerTurn.ForceMeshUpdate(); // Update the mesh to apply the gradient
    }

}