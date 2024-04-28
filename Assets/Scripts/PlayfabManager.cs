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

    public static class UserSessionInfo
    {
        public static string DisplayName { get; private set; }

        public static void SetDisplayName(string displayName)
        {
            DisplayName = displayName;
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
        var request = new LoginWithEmailAddressRequest {
            Email = emailInput.text,
            Password = passwordInput.text
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
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

    public void GetLeaderboard() {
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
