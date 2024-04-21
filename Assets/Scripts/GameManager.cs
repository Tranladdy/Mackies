using System.Collections;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement; // Import this namespace to manage scenes

public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject player1Prefab;
    public GameObject player2Prefab;
    public GameObject eventSystemWaiting;
    public GameObject eventSystemGame;
    public TextMeshProUGUI statusText;

    void Start()
    {
        eventSystemWaiting.SetActive(true);
        photonView.RPC("UpdateStatusTextRPC", RpcTarget.All, "LOOKING FOR PLAYER...");
        StartCoroutine(DelayedInstantiate());
    }

    IEnumerator DelayedInstantiate()
    {
        yield return new WaitForSeconds(1.0f);
        GameObject instantiatedPlayer = PhotonNetwork.Instantiate(
            PhotonNetwork.IsMasterClient ? player1Prefab.name : player2Prefab.name, 
            new Vector3(0, 5, 0), 
            Quaternion.identity
        );
        instantiatedPlayer.GetComponent<PlayerName>().playerTypeName = PhotonNetwork.IsMasterClient ? "Player1" : "Player2";
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            photonView.RPC("UpdateStatusTextRPC", RpcTarget.All, "PLAYER FOUND! LOADING...");
            StartCoroutine(UpdateUIWithDelay(7.0f));
        }
    }

    [PunRPC]
    void ActivateGameUI()
    {
        eventSystemWaiting.SetActive(false);
        eventSystemGame.SetActive(true);
        UpdateStatusText("GAME READY!");
    }

    [PunRPC]
    void UpdateStatusTextRPC(string text)
    {
        if (statusText != null)
            statusText.text = text;
        else
            Debug.LogError("Status Text is not assigned!");
    }

    IEnumerator UpdateUIWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            // Call RPC to update UI on all clients
            photonView.RPC("ActivateGameUI", RpcTarget.All);
        }
    }

    private void UpdateStatusText(string text)
    {
        if (statusText != null)
            statusText.text = text;
        else
            Debug.LogError("Status Text is not assigned!");
    }

    public void OnMainMenuButtonClicked()
    {
        // This function is called when the 'Main Menu' button is clicked on the victory screen.
        // Transition to the main menu, but only for the local player.
        SceneManager.LoadScene("Menu"); // Change "MainMenuSceneName" to your scene's actual name
    }
}