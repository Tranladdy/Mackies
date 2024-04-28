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
using Photon.Pun;
using Photon.Realtime;
using PlayFab.ClientModels;
using PlayFab;

public class TestInputOnlineFirebase : MonoBehaviourPunCallbacks
{
    // Reference to the GameManager script
    public TestInputOnlineFirebase gameManager;
    public GameObject gameCanvas;
    public GameObject player1VictoryScreen;
    public GameObject player2VictoryScreen;
    public GameObject player1VictoryScreenQuit;
    public GameObject player2VictoryScreenQuit;
    public GameObject eventSystemGame;
    public GameObject eventSystemWinQuit;
    public GameObject tieScreen;
    private string defaultResultText = "Waiting...";

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
    private bool gameOfficiallyEnded = false;

    private Dictionary<string, float> proteins = new Dictionary<string, float>(); // Dictionary to store proteins and their values
    private Dictionary<int, Sprite> foodImages = new Dictionary<int, Sprite>(); // Dictionary to store food images
    private Dictionary<string, int> foodImagesIds = new Dictionary<string, int>();

    private int currentGrams; // Variable to store the generated grams value
    private string currentProtein; // Variable to store the selected protein
    private int currentImageID; // Variable to store the selected food image ID

    private Vector3 startingPosition; // Variable to store the starting position of the sprite object

    private int maxRounds = 5;  // Added maxRounds variable
    private int round = 1; // Variable to store the current round number

    private const float timerDuration = 12f; // Duration of the timer in seconds

    public PlayfabManager playfabManager;

    private void Start()
    {
        nonTMPText.text = defaultResultText;
        nonTMPText.color = Color.cyan;
        startingPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1.1f, 10));
        userInputField.interactable = PhotonNetwork.IsMasterClient; // Master client starts first
        UpdatePlayerTurnText();
        StartCoroutine(InitializeGame());
    }

    private IEnumerator InitializeGame()
    {
        // Start loading proteins from CSV
        yield return StartCoroutine(LoadProteinsFromCSV());

        // Assuming LoadFoodImages depends on proteins being loaded
        //LoadFoodImages();

        // Start the main game functionality once data is ready
        StartCoroutine(GenerateChickenBreastCoroutine());
        UpdateRoundText();

        // Now safe to log information if currentProtein has been initialized
        if (!string.IsNullOrEmpty(currentProtein))
        {
            LogFoodInformation();  // Ensure this is called after currentProtein is set
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

    [PunRPC]
    void StopAllCoroutinesOnClients()
    {
        StopAllCoroutines();  // This stops all coroutines on the client where it's called.
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
            photonView.RPC("StopAllCoroutinesOnClients", RpcTarget.All);

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

    [PunRPC]
    void UpdatePlayerTurn(bool isPlayerOneTurn)
    {
        isPlayer1Turn = isPlayerOneTurn;
        UpdatePlayerTurnText(); // This should update the UI to reflect whose turn it is
    }

    [PunRPC]
    void UpdateScoresOnAllClients(int score1, int score2, string message, float r, float g, float b, float a)
    {
        player1Score = score1;
        player2Score = score2;
        nonTMPText.text = message;
        nonTMPText.color = new Color(r, g, b, a);
        UpdateScoreText();
    }

    [PunRPC]
    void ResetDefaultText()
    {
        nonTMPText.text = defaultResultText;
        nonTMPText.color = Color.cyan;
    }

    [PunRPC]
    void ClearText()
    {
        TMPText.text = ""; // Clear food name
        gramsText.text = ""; // Clear grams display
    }

    private IEnumerator UpdateScorePlayer1(float percentageDifference)
    {
        Debug.Log($"Player 1 scored with a percentage difference of: {percentageDifference}"); // Debugging line

        string message = "Invalid input";  // This is a fallback message
        Color color = Color.gray;  // Default color is gray
        int pointsToAdd = 0;

        if (percentageDifference <= 2.5f)
        {
            pointsToAdd = 12;
            message = "Perfect Player 1!!!! +12 Points";
            color = Color.cyan;
        }
        else if (percentageDifference <= 7f)
        {
            pointsToAdd = 6;
            message = "Excellent Player 1!!! +6 Points";
            color = Color.green;
        }
        else if (percentageDifference <= 9f)
        {
            pointsToAdd = 5;
            message = "Great Job Player 1! +5 Points";
            color = Color.green;
        }
        else if (percentageDifference <= 30f)
        {
            pointsToAdd = 4;
            message = "Good Job Player 1 +4 Points";
            color = Color.yellow;
        }
        else if (percentageDifference <= 40f)
        {
            pointsToAdd = 3;
            message = "Unsatisfactory Player 1. +3 Points";
            color = Color.yellow;
        }
        else if (percentageDifference <= 50f)
        {
            pointsToAdd = 2;
            message = "Bad Player 1.. +2 Points";
            color = Color.red;
        }
        else if (percentageDifference > 50f)
        {
            message = "Disappointing Player 1...";
            color = Color.red;
        }

        player1Score += pointsToAdd;
        nonTMPText.text = message;
        nonTMPText.color = color;
        photonView.RPC("UpdateScoresOnAllClients", RpcTarget.All, player1Score, player2Score, message, color.r, color.g, color.b, color.a);
        UpdateScoreText();

        yield return new WaitForSeconds(2f);

        photonView.RPC("ResetDefaultText", RpcTarget.All);

        // Transition to the next player or round
        isPlayer1Turn = false; // Switch turn
        photonView.RPC("UpdatePlayerTurn", RpcTarget.All, false); // It's now Player 2's turn
        UpdatePlayerTurnText();
        if (isPlayer1Turn)
        {
            StartCoroutine(GenerateChickenBreastCoroutine());
        }
        else
        {
            photonView.RPC("StartTimer", RpcTarget.All);
        }
    }

    private IEnumerator UpdateScorePlayer2(float percentageDifference)
    {
        string message = "Invalid input";  // This is a fallback message
        Color color = Color.gray;  // Default color is gray
        int pointsToAdd = 0;

        if (percentageDifference <= 2.5f)
        {
            pointsToAdd = 12;
            message = "Perfect Player 2!!!! +12 Points";
            color = Color.cyan;
        }
        else if (percentageDifference <= 7f)
        {
            pointsToAdd = 6;
            message = "Excellent Player 2!!! +6 Points";
            color = Color.green;
        }
        else if (percentageDifference <= 9f)
        {
            pointsToAdd = 5;
            message = "Great Job Player 2! +5 Points";
            color = Color.green;
        }
        else if (percentageDifference <= 30f)
        {
            pointsToAdd = 4;
            message = "Good Job Player 2 +4 Points";
            color = Color.yellow;
        }
        else if (percentageDifference <= 40f)
        {
            pointsToAdd = 3;
            message = "Unsatisfactory Player 2. +3 Points";
            color = Color.yellow;
        }
        else if (percentageDifference <= 50f)
        {
            pointsToAdd = 2;
            message = "Bad Player 2.. +2 Points";
            color = Color.red;
        }
        else if (percentageDifference > 50f)
        {
            message = "Disappointing Player 2...";
            color = Color.red;
        }

        player2Score += pointsToAdd;
        nonTMPText.text = message;
        nonTMPText.color = color;
        photonView.RPC("UpdateScoresOnAllClients", RpcTarget.All, player1Score, player2Score, message, color.r, color.g, color.b, color.a);
        UpdateScoreText();

        yield return new WaitForSeconds(2f);

        // Display the expected result
        DisplayExpectedResult();

        yield return new WaitForSeconds(2f); // Show the expected result for 2 seconds

        photonView.RPC("ResetDefaultText", RpcTarget.All);
        photonView.RPC("ClearText", RpcTarget.All);

        // Check if the current round is the last round
        round++; // Increment round count
        if (round > maxRounds)
        {
            // This is the final round, so we handle the end of the game
            DisplayExpectedResult();

            // Show the expected result for 2 seconds
            yield return new WaitForSeconds(2f); 

            nonTMPText.text = ""; // Clear the text
            photonView.RPC("ClearText", RpcTarget.All);

            // Call the end of game handling method on the GameManager
            photonView.RPC("HandleGameEnd", RpcTarget.All);  // Notify all clients that the game has ended.
        }
        else
        {
            // Not the final round, so just prepare for the next round
            isPlayer1Turn = true; // Switch turn back to player 1
            photonView.RPC("UpdatePlayerTurn", RpcTarget.All, true); // It's now Player 2's turn
            UpdatePlayerTurnText();
            StartCoroutine(GenerateChickenBreastCoroutine());
        }
    }

    [PunRPC]
    void HandleGameEnd()
    {
        gameOfficiallyEnded = true;  // Set a flag to indicate that the game has officially ended.

        // Deactivate the game canvas
        if (gameCanvas != null)
        {
            gameCanvas.SetActive(false);
        }

        // Check if a user is logged in with PlayFab
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            // Send scores to PlayFab for both players
            if (PhotonNetwork.IsMasterClient) // Master client sends Player 1's score
            {
                playfabManager.SendLeaderboard(player1Score);
            }
            else // Regular client sends Player 2's score
            {
                playfabManager.SendLeaderboard(player2Score);
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
        else
        {
            Debug.Log("No user is logged in. Leaderboard skipped.");
            // Show the appropriate victory screen without updating the leaderboard
            if (player1Score > player2Score)
            {
                player1VictoryScreen.SetActive(true);
            }
            else if (player2Score > player1Score)
            {
                player2VictoryScreen.SetActive(true);
            }
            else // It's a tie
            {
                tieScreen.SetActive(true);
            }
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log("Player " + otherPlayer.NickName + " has left the room.");

        if (!gameOfficiallyEnded) // Only show quit victory screen if game hasn't ended officially
        {
            foreach (PlayerName player in FindObjectsOfType<PlayerName>())
            {
                if (player.GetPhotonView().Owner != otherPlayer)
                {
                    string playerType = player.playerTypeName;
                    Debug.Log("Remaining player is " + playerType);

                    if (playerType == "Player1")
                    {
                        ShowVictoryScreenQuit(player1VictoryScreenQuit);
                        Debug.Log("Showing Player 1 Victory Screen.");
                    }
                    else if (playerType == "Player2")
                    {
                        ShowVictoryScreenQuit(player2VictoryScreenQuit);
                        Debug.Log("Showing Player 2 Victory Screen.");
                    }
                    break;
                }
            }
        }
    }

    private void ShowVictoryScreenQuit(GameObject victoryScreen)
    {
        eventSystemGame.SetActive(false);
        eventSystemWinQuit.SetActive(true);
        victoryScreen.SetActive(true);
    }
    
    [PunRPC]
    void DisplayExpectedResultRPC(float expectedOutput) {
        nonTMPText.text = "Answer: " + expectedOutput.ToString("F2") + "g";
        nonTMPText.color = Color.cyan; // Set the text color as needed
    }

    private void DisplayExpectedResult()
    {
        float expectedOutput = proteins[currentProtein] * currentGrams;
        photonView.RPC("DisplayExpectedResultRPC", RpcTarget.All, expectedOutput);
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

    [PunRPC]
    void UpdateProteinAndGrams(string protein, int grams, int roundNumber)
    {
        Debug.Log($"RPC called: Protein={protein}, Grams={grams}, Round={roundNumber}");
        currentProtein = protein;
        currentGrams = grams;
        round = roundNumber;

        StartCoroutine(UpdateUIWithDelays(grams, protein));
    }

    IEnumerator UpdateUIWithDelays(int grams, string protein)
    {
        yield return new WaitForSeconds(1.5f);
        gramsText.text = grams + "g";  // Display the generated grams in UI
        scaleGramsText.text = grams + "";  // Update any other UI elements as necessary

        yield return new WaitForSeconds(2.5f);
        TMPText.text = protein + "?";
        UpdateRoundText(); // Updates the round information on the UI
    }

    void SendProteinAndGramsUpdate()
    {
        // Regardless of who is the master, this sends the update to all
        photonView.RPC("UpdateProteinAndGrams", RpcTarget.All, currentProtein, currentGrams, round);
    }

    [PunRPC]
    void UpdateFoodImageOnAllClients(string imageId)
    {
        int parsedId = int.Parse(imageId);
        // Check if the image is already loaded
        if (!foodImages.ContainsKey(parsedId))
        {
            // If not, initiate download and ensure it updates once done
            StartCoroutine(DownloadAndSetImage(imageId, true));  // true ensures RPC update call post download
        }
        else
        {
            // Activate or update the sprite renderer with the new image
            ActivateFoodSprite(parsedId);
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

            // Once set, update all clients
            SendProteinAndGramsUpdate();
            
            yield return new WaitForSeconds(1.5f);
            gramsText.text = currentGrams + "g";  // Display the generated grams in UI
            scaleGramsText.text = currentGrams + "";  // Update any other UI elements as necessary

            LogFoodInformation();
            yield return new WaitForSeconds(2.5f);
            TMPText.text = currentProtein + "?";
            
            // Get the image ID for the current protein and load it
            currentImageID = GetImageID(currentProtein);
            if (currentImageID != -1)
            {
                // Only load the image if it hasn't been loaded before
                if (!foodImages.ContainsKey(currentImageID))
                {
                    // DownloadAndSetImage should now be responsible for activating the sprite renderer once the image is ready
                    StartCoroutine(DownloadAndSetImage(currentImageID.ToString(), true));
                }
                else
                {
                    // If the image is already loaded, just activate it and start the falling effect

                    ActivateFoodSprite(currentImageID);
                    photonView.RPC("UpdateFoodImageOnAllClients", RpcTarget.All, currentImageID.ToString());
                }
            }

            photonView.RPC("StartTimer", RpcTarget.All);
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
        string line = reader.ReadLine();  // Skip header if present.

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

    private void LoadFoodImages()
    {
        // Assuming you have a way to map from protein names to their IDs
        foreach (var protein in proteins.Keys)
        {
            string imageId = GetImageID(protein).ToString();
            StartCoroutine(DownloadAndSetImage(imageId));
        }
    }

    // DownloadAndSetImage now expects an ID, not a name
    private IEnumerator DownloadAndSetImage(string imageId, bool triggerUpdate = false)
    {
        string accessToken = "636d15af-2cb2-4628-8c0c-bd30fd47629f"; // Example token
        string imageUrl = $"https://firebasestorage.googleapis.com/v0/b/mackies-9f1b0.appspot.com/o/FoodImages%2F{imageId}.png?alt=media&token={accessToken}";

        Debug.Log("Downloading image from URL: " + imageUrl);
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(www);
            Sprite newSprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            foodImages[int.Parse(imageId)] = newSprite;

            if (triggerUpdate)
            {
                SetFoodImage();
                photonView.RPC("UpdateFoodImageOnAllClients", RpcTarget.All, imageId);
            }
        }
        else
        {
            Debug.LogError($"Failed to download image ID {imageId}: {www.error}, URL: {imageUrl}");
        }
    }

    private void ActivateFoodSprite(int imageId)
    {
        if (foodImages.ContainsKey(imageId))
        {
            foodSpriteRenderer.sprite = foodImages[imageId];
            foodSpriteRenderer.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Attempted to activate a sprite that doesn't exist: " + imageId);
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
        if (foodImagesIds.TryGetValue(protein, out int imageId))
        {
            return imageId;
        }
        return -1; // Return -1 if no matching image ID is found
    }

    [PunRPC]
    void StartTimer()
    {
        StartCoroutine(TimerCoroutine());
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

        // Switch turns
        isPlayer1Turn = !isPlayer1Turn;
        photonView.RPC("UpdatePlayerTurn", RpcTarget.All, isPlayer1Turn);
        UpdatePlayerTurnText();
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

            // Enable input for Player 1 if this client is the master client
            userInputField.interactable = PhotonNetwork.IsMasterClient;
        }
        else
        {
            playerTurn.text = "Player 2's Turn";
            playerTurn.colorGradientPreset = player2Gradient;

            // Enable input for Player 2 if this client is not the master client
            userInputField.interactable = !PhotonNetwork.IsMasterClient;
        }
        playerTurn.ForceMeshUpdate(); // Update the mesh to apply the gradient
    }

}