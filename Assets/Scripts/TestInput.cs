using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class TestInput : MonoBehaviour
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

    private int currentGrams; // Variable to store the generated grams value
    private string currentProtein; // Variable to store the selected protein
    private int currentImageID; // Variable to store the selected food image ID

    private Vector3 startingPosition; // Variable to store the starting position of the sprite object

    private int round = 0; // Variable to store the round number

    private const float timerDuration = 10f; // Duration of the timer in seconds
    private void LogFoodInformation()
    {
        // Debug log for protein name, grams, and expected output
        Debug.Log("Protein: " + currentProtein + ", Grams: " + currentGrams + ", Expected Output: " + (proteins[currentProtein] * currentGrams));
    }
    private void Start()
    {
        // Get the starting position outside the main camera's view
        startingPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1.1f, 10));
        LoadProteinsFromCSV(); // Load proteins from CSV file
        LoadFoodImages(); // Load food images
        StartCoroutine(GenerateChickenBreastCoroutine()); // Start coroutine
        UpdateRoundText(); // Update round text
        LogFoodInformation(); // Debugger for information
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
        // Increment round number
        round++;

        // Update round text
        UpdateRoundText();

        // Select a random protein
        int randomIndex = UnityEngine.Random.Range(0, proteins.Count);
        currentProtein = new List<string>(proteins.Keys)[randomIndex];
        float proteinValue = proteins[currentProtein];

        // Set the grams after a delay
        yield return new WaitForSeconds(2.5f); // Adjust delay time as needed
        currentGrams = GenerateRandomGrams(); // Generate random grams
        gramsText.text = currentGrams + "g"; // Set the grams text
        scaleGramsText.text = currentGrams + ""; // Set the scaleGrams text

        // Log food information after setting grams and protein
        LogFoodInformation();

        // Set the protein name after a delay
        yield return new WaitForSeconds(3.5f); // Adjust delay time as needed
        TMPText.text = currentProtein;
        TMPText.text = currentProtein + "?"; // Add "?" to the end of the protein name

        // Select the corresponding food image
        currentImageID = GetImageID(currentProtein);
        if (currentImageID != -1)
        {
            if (foodImages.ContainsKey(currentImageID))
            {
                // Set the new food image sprite
                foodSpriteRenderer.sprite = foodImages[currentImageID];
                // Reactivate the food image GameObject
                foodSpriteRenderer.gameObject.SetActive(true);
            }
        }

        // Start the timer coroutine
        StartCoroutine(TimerCoroutine());
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
        Sprite[] sprites = Resources.LoadAll<Sprite>("FoodImages"); // Assuming your images are in a folder named "FoodImages" in the Resources folder

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