using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayfabManager : MonoBehaviour
{
    public GameObject rowPrefab;
    public Transform rowsParent;

    [Header("UI")]
    public TMP_Text messageText;
    public TMP_Text loginButtonText;

    public InputField emailInput;
    public InputField passwordInput;

    public GameObject displayNameUI;
    public GameObject accountLogin;
    public InputField displayNameInputField;
    public TMP_Text mainMenuDisplayNameText;
    public GameObject warningText;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

        void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateDisplayNameOnUI();
        ToggleTextGameObjectBasedOnLogin();
    }

    public void UpdateLoginButtonText()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            loginButtonText.text = "LOGOUT";  // Set the button text to LOGOUT if the user is logged in
        }
        else
        {
            loginButtonText.text = "LOGIN";  // Set the button text to LOGIN if the user is not logged in
        }
    }

    public static class UserSessionInfo
    {
        public static string DisplayName { get; private set; }

        public static void SetDisplayName(string displayName)
        {
            DisplayName = displayName;
        }
    }

    public void ClearLoginFields() 
    {
        if (emailInput != null) {
            emailInput.text = ""; // Clear the email input field
        }
        if (passwordInput != null) {
            passwordInput.text = ""; // Clear the password input field
        }
        if (messageText != null) {
            messageText.text = ""; // Reset the message text
        }
    }

    public void UpdateDisplayNameOnUI()
    {
        if (PlayFabClientAPI.IsClientLoggedIn() && mainMenuDisplayNameText != null)
        {
            mainMenuDisplayNameText.text = "LOGGED IN AS: " + UserSessionInfo.DisplayName;
        }
    }

    public void ToggleTextGameObjectBasedOnLogin()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            // Disable the text GameObject if the player is logged in
            if (warningText != null)
                warningText.SetActive(false);
        }
        else
        {
            // Enable the text GameObject if the player is not logged in
            if (warningText != null)
                warningText.SetActive(true);
        }
    }

    public void RegisterButton() {
        if (passwordInput.text.Length < 6) {
            messageText.text = "PASSWORD TOO SHORT!";
            return;
        }
        var request = new RegisterPlayFabUserRequest {
            Email = emailInput.text,
            Password = passwordInput.text,
            RequireBothUsernameAndEmail = false
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }

    void OnRegisterSuccess(RegisterPlayFabUserResult result) {
        messageText.text = "REGISTERED AND LOGGED IN!";
        GetAccountInfo(result.PlayFabId);
    }

    public void LoginButton() {
        // Toggle between Login and Logout based on the current text of the button
        if (loginButtonText.text == "LOGOUT") {
            Logout();
        } else {
            var request = new LoginWithEmailAddressRequest {
                Email = emailInput.text,
                Password = passwordInput.text
            };
            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
        }
    }

    void Logout() {
        PlayFabClientAPI.ForgetAllCredentials(); // This effectively logs the user out
        loginButtonText.text = "LOGIN";
        messageText.text = "LOGGED OUT!";
        mainMenuDisplayNameText.text = "LOGGED IN AS: GUEST";
    }

    public void ResetPasswordButton() {
        var request = new SendAccountRecoveryEmailRequest {
            Email = emailInput.text,
            TitleId = "3C3FD"
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordReset, OnError);
    }

    void OnPasswordReset(SendAccountRecoveryEmailResult result) {
        messageText.text = "PASSWORD RECOVERY SENT!";
    }

    // Start is called before the first frame update
    void Start()
    {
        //Login();
    }

    // Update is called once per frame
    void Login()
    {
        var request = new LoginWithCustomIDRequest {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnError);
    }

    void OnLoginSuccess(LoginResult result) {
        Debug.Log("OnLoginSuccess called");
        messageText.text = "LOGGED IN!";
        loginButtonText.text = "LOGOUT"; // Update the button text to "LOGOUT"
        GetAccountInfo(result.PlayFabId);
        ToggleTextGameObjectBasedOnLogin();
    }

    void GetAccountInfo(string playFabId) {
        var request = new GetAccountInfoRequest {
            PlayFabId = playFabId
        };
        PlayFabClientAPI.GetAccountInfo(request, OnAccountInfoReceived, OnError);
    }

    void OnAccountInfoReceived(GetAccountInfoResult result) {
        var displayName = result.AccountInfo.TitleInfo.DisplayName;
        if (string.IsNullOrEmpty(displayName)) {
            Debug.Log("No display name set.");
            accountLogin.SetActive(false);
            displayNameUI.SetActive(true);
        } else {
            Debug.Log("Display name is set: " + displayName);
            displayNameUI.SetActive(false);
            UpdateMainMenuDisplayName(displayName);
            UserSessionInfo.SetDisplayName(displayName); // Store the display name in the static class
            UpdateDisplayNameOnUI();
        }
    }

    void UpdateMainMenuDisplayName(string displayName) {
        if (mainMenuDisplayNameText != null) {
            mainMenuDisplayNameText.text = "LOGGED IN AS: " + displayName;
        }
    }
    

    public void UpdateDisplayName() {
        string newDisplayName = displayNameInputField.text;
        if (string.IsNullOrWhiteSpace(newDisplayName)) {
            Debug.LogError("Display Name cannot be empty");
            return;
        }

        var request = new UpdateUserTitleDisplayNameRequest { DisplayName = newDisplayName };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameUpdate, OnError);
    }

    void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult result) {
        Debug.Log("Display name updated successfully to: " + result.DisplayName);
        displayNameUI.SetActive(false);
        UpdateMainMenuDisplayName(result.DisplayName);
    }

    void OnError(PlayFabError error) {
        messageText.text = error.ErrorMessage;
        Debug.Log(error.GenerateErrorReport());
        if (!PlayFabClientAPI.IsClientLoggedIn())
            loginButtonText.text = "LOGIN";
    }

    public void SendLeaderboard(int score) {
        var request = new UpdatePlayerStatisticsRequest {
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate {
                    StatisticName = "Score Leaderboard",
                    Value = score
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnError);
    }

    void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result) {
        Debug.Log("Successful leaderboard sent");
    }

    public void ShowPersonalRankingOnly() {
        if (!PlayFabClientAPI.IsClientLoggedIn()) {
            warningText.GetComponent<TMP_Text>().text = "MUST BE LOGGED IN TO SEE LEADERBOARD";
            return;
        }

        // First, clear the existing leaderboard items
        foreach (Transform item in rowsParent) {
            Destroy(item.gameObject);
        }

        // Request to get the player's rank in the leaderboard
        var request = new GetLeaderboardAroundPlayerRequest {
            StatisticName = "Score Leaderboard",
            MaxResultsCount = 1
        };
        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnPersonalRankReceived, OnError);
    }

    void OnPersonalRankReceived(GetLeaderboardAroundPlayerResult result) {
        if (result.Leaderboard != null && result.Leaderboard.Count > 0) {
            foreach (var item in result.Leaderboard) {
                GameObject newGo = Instantiate(rowPrefab, rowsParent);
                TMP_Text[] texts = newGo.GetComponentsInChildren<TMP_Text>();
                texts[0].text = (item.Position + 1).ToString();
                texts[1].text = item.DisplayName;
                texts[2].text = item.StatValue.ToString();
            }
        }
    }

    public void GetLeaderboard() {
        if (!PlayFabClientAPI.IsClientLoggedIn()) {
            // Clear the leaderboard if not logged in
            foreach (Transform item in rowsParent) {
                Destroy(item.gameObject);
            }
            // Ensure the warningText GameObject is set to active and update its text
            if (warningText != null) {
                warningText.SetActive(true);
                warningText.GetComponent<TMP_Text>().text = "MUST BE LOGGED IN TO SEE LEADERBOARD";
            }
            return; // Stop further execution if the user is not logged in
        } else {
            // Hide the warning text when the user is logged in and can see the leaderboard
            if (warningText != null) {
                warningText.SetActive(false);
            }
        }

        var request = new GetLeaderboardRequest {
            StatisticName = "Score Leaderboard",
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnError);
    }

    void OnLeaderboardGet(GetLeaderboardResult result) {

        foreach (Transform item in rowsParent) {
            Destroy(item.gameObject);
        }

        foreach (var item in result.Leaderboard) {
            GameObject newGo = Instantiate(rowPrefab, rowsParent);
            TMP_Text[] texts = newGo.GetComponentsInChildren<TMP_Text>();
            Debug.Log("Number of TMP_Text components: " + texts.Length);
            texts[0].text = (item.Position + 1).ToString();
            texts[1].text = item.DisplayName;
            texts[2].text = item.StatValue.ToString();

            Debug.Log(item.Position + " " + item.PlayFabId + " " + item.StatValue);
        }
    }
}
