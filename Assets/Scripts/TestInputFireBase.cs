using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Firebase.Storage;
using UnityEngine.Networking;

public class TestInputFireBase : MonoBehaviour
{
    public InputField userInputField;
    public Text nonTMPText;
    public Text timerText;
    public TextMeshProUGUI TMPText; // Reference to the TextMeshProUGUI for displaying the protein
    public TextMeshProUGUI gramsText; // Reference to the TextMeshProUGUI for displaying the grams
    public TextMeshProUGUI scaleGramsText; // Reference to the TextMeshProUGUI for displaying scale grams
    public TextMeshProUGUI scoreText; // Reference to the TextMeshProUGUI for displaying the score
    public TextMeshProUGUI roundText; // Reference to the TextMeshProUGUI for displaying the round number
   
    public SpriteRenderer foodSpriteRenderer; // Reference to the SpriteRenderer for displaying the food image

    private Dictionary<string, float> proteins = new Dictionary<string, float>(); // Dictionary to store proteins and their values
    private Dictionary<int, Sprite> foodImages = new Dictionary<int, Sprite>(); // Dictionary to store food images

    private Dictionary<string, int> foodImagesIds = new Dictionary<string, int>();
    private int currentGrams; // Variable to store the generated grams value
    private string currentProtein; // Variable to store the selected protein
    private int currentImageID; // Variable to store the selected food image ID

    private Vector3 startingPosition; // Variable to store the starting position of the sprite object

    private int round = 0; // Variable to store the round number

    private const float timerDuration = 10f; // Duration of the timer in seconds
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

    private void Start()
    {
        startingPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1.1f, 10));
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

    public int score = 0; // Variable to store the score

    private void UpdateRoundText()
    {
        roundText.text = "Round: " + round.ToString();
    }

    public void ValidateUserInput()
    {
        string input = userInputField.text;
        float userInput;
        bool isValidInput = float.TryParse(input, out userInput);

        if (isValidInput)
        {
            // Clear old information
            TMPText.text = "";
            gramsText.text = "";
            scaleGramsText.text = "";

            float proteinValue = proteins[currentProtein];
            float expectedOutput = proteinValue * currentGrams;

            // Update score based on percentage difference
            float percentageDifference = Mathf.Abs(userInput - expectedOutput) / expectedOutput * 100;
            UpdateScore(percentageDifference);

            // Update the score text
            scoreText.text = "PLAYER 1 SCORE: " + score.ToString();

            // Deactivate the food image
            foodSpriteRenderer.gameObject.SetActive(false);

            // Reset the position of the sprite object to its starting position
            ResetFoodSpritePosition();

            StopAllCoroutines(); // Stop the timer coroutine if running

            // Generate new grams and protein
            StartCoroutine(GenerateChickenBreastCoroutine());

            // Clear the input field
            userInputField.text = "";
        }
        else
        {
            nonTMPText.text = "Please enter a valid number.";
            nonTMPText.color = Color.red;
        }
    }

    private void UpdateScore(float percentageDifference)
    {
        if (percentageDifference <= 1f)
        {
            score += 12;
            nonTMPText.text = "Perfect!!!! +12 Points";
            nonTMPText.color = Color.cyan;
        }
        else if (percentageDifference <= 3f)
        {
            score += 6;
            nonTMPText.text = "Excellent!!! +6 Points";
            nonTMPText.color = Color.green;
        }
        else if (percentageDifference <= 5f)
        {
            score += 5;
            nonTMPText.text = "Great Job! +5 Points";
            nonTMPText.color = Color.green;
        }
        else if (percentageDifference <= 20f)
        {
            score += 4;
            nonTMPText.text = "Good Job +4 Points";
            nonTMPText.color = Color.yellow;
        }
        else if (percentageDifference <= 30f)
        {
            score += 3;
            nonTMPText.text = "Unsatisfactory. +3 Points";
            nonTMPText.color = Color.yellow;
        }
        else if (percentageDifference <= 40f)
        {
            score += 2;
            nonTMPText.text = "Bad.. +2 Points";
            nonTMPText.color = Color.red;
        }
        else
        {
            nonTMPText.text = "Disappointing...";
            nonTMPText.color = Color.red;
        }
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

        round++;
        UpdateRoundText();

        if (proteins.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, proteins.Count);
            currentProtein = new List<string>(proteins.Keys)[randomIndex];
            
            // Generate random grams here
            currentGrams = GenerateRandomGrams();
            gramsText.text = currentGrams + "g";  // Display the generated grams in UI
            scaleGramsText.text = currentGrams + "";  // Update any other UI elements as necessary

            yield return new WaitForSeconds(3.5f);
            TMPText.text = currentProtein + "?";
            

            // Get the image ID for the current protein and load it
            currentImageID = GetImageID(currentProtein);
            if (currentImageID != -1)
            {
                // Only load the image if it hasn't been loaded before
                if (!foodImages.ContainsKey(currentImageID))
                {
                    // DownloadAndSetImage should now be responsible for activating the sprite renderer once the image is ready
                    StartCoroutine(DownloadAndSetImage(currentImageID.ToString()));
                }
                else
                {
                    // If the image is already loaded, just activate it
                    ActivateFoodSprite(currentImageID);
                }
            }

            StartCoroutine(TimerCoroutine());
        }
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

    // Example of adjusting the DownloadAndSetImage call
    private void StartLoadingImages()
    {
        // Assuming you have a way to map from protein names to their IDs
        foreach (var protein in proteins.Keys)
        {
            string imageId = GetImageID(protein).ToString();
            StartCoroutine(DownloadAndSetImage(imageId));
        }
    }

    // DownloadAndSetImage now expects an ID, not a name
    private IEnumerator DownloadAndSetImage(string imageId)
    {
        string accessToken = "636d15af-2cb2-4628-8c0c-bd30fd47629f"; // Example token
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
                ActivateFoodSprite(parsedId); // Now activate the sprite
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

    private IEnumerator TimerCoroutine()
    {
        float timer = timerDuration;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            int roundedTimer = Mathf.CeilToInt(timer);
            timerText.text = roundedTimer.ToString(); // Update the text of the timerText object
            
            // Change color based on timer value
            if (roundedTimer == 10 || roundedTimer == 9 || roundedTimer == 8)
            {
                timerText.color = Color.cyan; // 10, 9, 8 will be cyan
            }
            else if (roundedTimer == 7 || roundedTimer == 6) // 7 and 6 will be green
            {
                timerText.color = Color.green;
            }
            else if (roundedTimer == 5 || roundedTimer == 4) // 5 and 4 will be yellow
            {
                timerText.color = Color.yellow;
            }
            else // 1, 2, and 3 will be red
            {
                timerText.color = Color.red;
            }

            yield return null;
        }

        // If the timer expires, handle it
        HandleTimerExpired();
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
}