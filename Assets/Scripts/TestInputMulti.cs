using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TestInputMulti : MonoBehaviour
{
    // Reference to the GameManager script
    public TestInputMulti gameManager;
    public GameObject gameCanvas;
    public GameObject player1VictoryScreen;
    public GameObject player2VictoryScreen;
    public GameObject tieScreen;
    private string defaultResultText = "Awaiting result...";

    public TMP_ColorGradient player1Gradient;
    public TMP_ColorGradient player2Gradient;
    public InputField userInputField;
    public Text nonTMPText;
    public Text timerText;
    public TextMeshProUGUI playerTurn;
    public TextMeshProUGUI TMPText; // Reference to the TextMeshProUGUI for displaying the protein
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

    private int currentGrams; // Variable to store the generated grams value
    private string currentProtein; // Variable to store the selected protein
    private int currentImageID; // Variable to store the selected food image ID

    private Vector3 startingPosition; // Variable to store the starting position of the sprite object

    private int maxRounds;  // Added maxRounds variable
     private int round = 1; // Variable to store the current round number

    private const float timerDuration = 10f; // Duration of the timer in seconds

    private void Start()
    {
        maxRounds = RoundManagerLocal.Instance.RoundCount;  // Retrieve the number of rounds from RoundManager
        nonTMPText.text = defaultResultText;
        nonTMPText.color = Color.cyan; // Ensure it starts as cyan
        // Get the starting position outside the main camera's view
        startingPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1.1f, 10));
        LoadProteinsFromCSV(); // Load proteins from CSV file
        LoadFoodImages(); // Load food images
        StartCoroutine(GenerateChickenBreastCoroutine()); // Start coroutine
        UpdateRoundText(); // Update round text
        UpdatePlayerTurnText();
        LogFoodInformation(); // Debugger for information
    }

    private void LogFoodInformation()
    {
        // Ensure currentProtein is not null and exists in the dictionary to avoid ArgumentNullException
        if (!string.IsNullOrEmpty(currentProtein) && proteins.ContainsKey(currentProtein))
        {
            Debug.Log("Protein: " + currentProtein + ", Grams: " + currentGrams + ", Expected Output: " + (proteins[currentProtein] * currentGrams));
        }
        else
        {
            Debug.Log("Attempted to log food information before currentProtein was set.");
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
                StartCoroutine(UpdateScorePlayer1(percentageDifference));
                isPlayer1Turn = false;
            }
            else
            {
                StartCoroutine(UpdateScorePlayer2(percentageDifference));
                isPlayer1Turn = true;
            }

            // Clear the input field
            userInputField.text = "";
        }
        else
        {
            nonTMPText.text = "Please enter a valid number.";
            nonTMPText.color = Color.red;
        }
    }

    private IEnumerator DisplayResultsTemporary(string message, Color color)
    {
        nonTMPText.text = message;
        nonTMPText.color = color;
        yield return new WaitForSeconds(2f); // Display results for 2 seconds
        nonTMPText.text = ""; // Optionally clear the text or set to default message

        nonTMPText.text = defaultResultText;
        nonTMPText.color = Color.cyan;
    }

    private IEnumerator UpdateScorePlayer1(float percentageDifference)
    {
        //Debug.Log("Percentage Difference Player 1: " + percentageDifference);  // Log for debugging

        string message = "Invalid input";  // This is a fallback message
        Color color = Color.gray;  // Default color is gray

        if (percentageDifference <= 2.5f)
        {
            player1Score += 12;
            message = "Perfect Player 1!!!! +12 Points";
            color = Color.cyan;
        }
        else if (percentageDifference <= 7f)
        {
            player1Score += 6;
            message = "Excellent Player 1!!! +6 Points";
            color = Color.green;
        }
        else if (percentageDifference <= 9f)
        {
            player1Score += 5;
            message = "Great Job Player 1! +5 Points";
            color = Color.green;
        }
        else if (percentageDifference <= 30f)
        {
            player1Score += 4;
            message = "Good Job Player 1 +4 Points";
            color = Color.yellow;
        }
        else if (percentageDifference <= 40f)
        {
            player1Score += 3;
            message = "Unsatisfactory Player 1. +3 Points";
            color = Color.yellow;
        }
        else if (percentageDifference <= 50f)
        {
            player1Score += 2;
            message = "Bad Player 1.. +2 Points";
            color = Color.red;
        }
        else if (percentageDifference > 50f)
        {
            message = "Disappointing Player 1...";
            color = Color.red;
        }

        nonTMPText.text = message;
        nonTMPText.color = color;
        UpdateScoreText();

        yield return new WaitForSeconds(2f);

        nonTMPText.text = defaultResultText;
        nonTMPText.color = Color.cyan;

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
        string message = "Invalid input";  // This is a fallback message
        Color color = Color.gray;  // Default color is gray

        if (percentageDifference <= 2.5f)
        {
            player2Score += 12;
            message = "Perfect Player 2!!!! +12 Points";
            color = Color.cyan;
        }
        else if (percentageDifference <= 7f)
        {
            player2Score += 6;
            message = "Excellent Player 2!!! +6 Points";
            color = Color.green;
        }
        else if (percentageDifference <= 9f)
        {
            player2Score += 5;
            message = "Great Job Player 2! +5 Points";
            color = Color.green;
        }
        else if (percentageDifference <= 30f)
        {
            player2Score += 4;
            message = "Good Job Player 2 +4 Points";
            color = Color.yellow;
        }
        else if (percentageDifference <= 40f)
        {
            player2Score += 3;
            message = "Unsatisfactory Player 2. +3 Points";
            color = Color.yellow;
        }
        else if (percentageDifference <= 50f)
        {
            player2Score += 2;
            message = "Bad Player 2.. +2 Points";
            color = Color.red;
        }
        else if (percentageDifference > 50f)
        {
            message = "Disappointing Player 2...";
            color = Color.red;
        }

        nonTMPText.text = message;
        nonTMPText.color = color;
        UpdateScoreText();

        yield return new WaitForSeconds(2f);

        // Display the expected result
        DisplayExpectedResult();

        yield return new WaitForSeconds(2f); // Show the expected result for 2 seconds

        nonTMPText.text = defaultResultText;
        nonTMPText.color = Color.cyan;
        TMPText.text = ""; // Clear food name
        gramsText.text = ""; // Clear grams display

        // Check if the current round is the last round
        round++; // Increment round count
        if (round > maxRounds)
        {
            // This is the final round, so we handle the end of the game
            DisplayExpectedResult();

            // Show the expected result for 2 seconds
            yield return new WaitForSeconds(2f); 

            nonTMPText.text = ""; // Clear the text
            TMPText.text = ""; // Clear food name
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
        nonTMPText.text = "Answer: " + expectedOutput.ToString("F2") + "g"; // Display the expected result formatted to two decimal places
        nonTMPText.color = Color.cyan; // Change color if necessary
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
        // Update round text and other UI updates here
        UpdateRoundText();

        // Select a random protein after displaying previous results
        yield return new WaitForSeconds(1f); // Wait for 1 second before starting new food selection

        // Select a random protein and ensure it's valid before proceeding
        int randomIndex = UnityEngine.Random.Range(0, proteins.Count);
        currentProtein = new List<string>(proteins.Keys)[randomIndex];
        currentGrams = GenerateRandomGrams(); // Generate random grams

        // Set the grams text and scaleGrams text after waiting
        yield return new WaitForSeconds(1.5f); // Simulate some processing delay
        gramsText.text = currentGrams + "g";
        scaleGramsText.text = currentGrams + "";

        // Log food information after setting grams and protein
        LogFoodInformation();

        // Display the protein name after a further delay
        yield return new WaitForSeconds(2.5f); // Wait before displaying the protein name
        TMPText.text = currentProtein + "?";

        // Select the corresponding food image
        SetFoodImage();

        // Restart the timer with a delay after selecting food
        StartCoroutine(TimerCoroutine());
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
    }

    private IEnumerator FallIntoPlace(Transform transform, Vector3 targetPosition, float duration)
    {
        float elapsedTime = 0;
        Vector3 initialPosition = transform.position;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(initialPosition, targetPosition, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }
    private void LoadProteinsFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("FoodInfo"); // Load CSV file from Resources folder
        StringReader reader = new StringReader(csvFile.text);

        string line;
        while ((line = reader.ReadLine()) != null)
        {
            string[] values = line.Split(',');
            if (values.Length >= 2)
            {
                string proteinName = values[0].Trim();
                float proteinValue;
                if (float.TryParse(values[1].Trim(), out proteinValue))
                {
                    proteins.Add(proteinName, proteinValue);
                }
            }
        }
    }

    private void LoadFoodImages()
    {
        // Load food images from folder
        Sprite[] sprites = Resources.LoadAll<Sprite>("FoodImages"); 
        // Assign each image to its corresponding image ID
        foreach (Sprite sprite in sprites)
        {
            int imageID;
            if (int.TryParse(sprite.name, out imageID))
            {
                foodImages.Add(imageID, sprite);

                // Adjust the scale of the sprite to ensure consistent size
                AdjustSpriteScale(sprite);
            }
        }
    }

    private void AdjustSpriteScale(Sprite sprite)
    {
        // Calculate the desired scale based on the original aspect ratio of the image
        float aspectRatio = sprite.rect.width / sprite.rect.height;
        float desiredWidth = 0.25f; // Set the desired width of the sprite
        float desiredHeight = desiredWidth / aspectRatio;

        // Set the scale of the sprite
        foodSpriteRenderer.transform.localScale = new Vector3(desiredWidth, desiredHeight, 1f);
    }

    private int GetImageID(string protein)
    {
        // Map each protein to its corresponding image ID
        switch (protein)
        {
            case "Chicken Breast":
                return 1;
            case "Chicken Thigh":
                return 2;
            case "Chicken Wing":
                return 3;
            case "Pork Chop":
                return 4;
            case "Pork Loin":
                return 5;
            case "Pork Belly":
                return 6;
            case "Ribeye Steak":
                return 7;
            case "New York Strip":
                return 8;
            case "Flank Steak":
                return 9;
            case "Lamb Chop":
                return 10;
            case "Salmon":
                return 11;
            case "Tuna":
                return 12;
            case "Tilapia":
                return 13;
            case "Yellow Tail":
                return 14;
            case "Egg":
                return 15;
            case "Milk":
                return 16;
            case "Spinach":
                return 17;
            case "Lettuce":
                return 18;
            case "Apple":
                return 19;
            case "Watermelon":
                return 20;
            case "Pineapple":
                return 21;
            case "Apple Juice":
                return 22;
            case "Orange":
                return 23;
            case "Dragon Fruit":
                return 24;
            case "Cheese Pizza":
                return 25;
            case "Pepperoni Pizza":
                return 26;
            case "BBQ Chicken Pizza":
                return 27;
            case "Chocolate Milk":
                return 28;
            case "Pickles":
                return 29;
            case "Double Cheeseburger":
                return 30;
            case "Hot Dog":
                return 31;
            case "Rice":
                return 32;
            case "Pasta Noodles":
                return 33;
            case "Macaroni and Cheese":
                return 34;
            case "White Bread":
                return 35;
            case "Whole Wheat Bread":
                return 36;
            case "Cheetos":
                return 37;
            case "Grilled Cheese":
                return 38;
            case "Fried Rice":
                return 39;
            case "Chow Mein":
                return 40;
            case "Orange Chicken":
                return 41;
            case "Brocoli Beef":
                return 42;
            case "Canned Salmon":
                return 43;
            case "Canned Tuna":
                return 44;
            case "Beef Taco":
                return 45;
            case "Pulled Pork":
                return 46;
            case "Brisket":
                return 47;
            case "Coleslaw":
                return 48;
            case "Mayo":
                return 49;
            case "Oyster":
                return 50;
            case "Rotisserie Chicken":
                return 51;
            case "Cheese":
                return 52;
            case "Coffee":
                return 53;
            case "Chicken Sandwich":
                return 54;
            case "Bacon":
                return 55;
            case "Tamale":
                return 56;
            case "Chicken Noodle Soup":
                return 57;
            case "Protein Powder":
                return 58;
            case "Greek Yogurt":
                return 59;
            case "Banana":
                return 60;
            case "Peanut Butter":
                return 61;
            case "Butter":
                return 62;
            case "Caesar Salad":
                return 63;
            case "Shrimp":
                return 64;
            case "Lobster":
                return 65;
            case "Cricket":
                return 66;
            case "Ham":
                return 67;
            case "Corn Bread":
                return 68;
            case "Pancake":
                return 69;
            case "Waffle":
                return 70;
            case "Falafel":
                return 71;
            case "Cookie Dough Ice Cream":
                return 72;
            case "Alligator":
                return 73;
            case "Escargot":
                return 74;
            case "Turkey":
                return 75;
            default:
                return -1; // Return -1 if no matching image ID is found
        }
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

        // If the timer expires, handle it
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
        // For example, defaulting the answer to zero
        userInputField.text = "0";
        ValidateUserInput();
    }

    private void ResetFoodSpritePosition()
    {
    // Set the position of the sprite object to its starting position
    foodSpriteRenderer.transform.position = startingPosition; // Assuming startingPosition is a Vector3 representing the starting position of the sprite object
    }

    private void UpdatePlayerTurnText()
    {
        if (isPlayer1Turn)
        {
            playerTurn.text = "Player 1's Turn";
            playerTurn.colorGradientPreset = player1Gradient;
        }
        else
        {
            playerTurn.text = "Player 2's Turn";
            playerTurn.colorGradientPreset = player2Gradient;
        }
        playerTurn.ForceMeshUpdate(); // Update the mesh to apply the gradient
    }

}