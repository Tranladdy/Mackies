using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class NetworkManager : MonoBehaviourPunCallbacks
{

    [SerializeField]
    private string preferredRegion = "usw"; // Set this to your preferred region

    void Start()
    {
        ConnectToPhoton();
    }

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = preferredRegion;
            PhotonNetwork.ConnectUsingSettings();  // Only connect if not already connected
        }
    }

    public void Connect()
    {
        if (PhotonNetwork.IsConnected)
        {
            // SQL-like filter to only join rooms where gameOver is false
            Hashtable expectedProperties = new Hashtable { { "gameOver", false } };
            PhotonNetwork.JoinRandomRoom(expectedProperties, 0);  // Try joining a room that is not marked as game over
        }
        else
        {
            ConnectToPhoton();  // Attempt to connect to Photon if disconnected
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Tried to join a room and failed, no available rooms that meet the criteria. Creating a new room.");
        RoomOptions roomOptions = new RoomOptions { 
            MaxPlayers = 2, 
            CustomRoomProperties = new Hashtable { { "gameOver", false } },
            CustomRoomPropertiesForLobby = new string[] { "gameOver" }  // Ensure 'gameOver' is visible in lobby filters
        };
        PhotonNetwork.CreateRoom(null, roomOptions);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a room!");

        // Optionally, double-check the room's game over status (for edge cases)
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameOver", out object status) && (bool)status)
        {
            Debug.Log("Joined a room that was marked as game over, leaving...");
            PhotonNetwork.LeaveRoom();
        }
        else if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(4);
        }
    }

    public void SetGameOver(bool isGameOver)
    {
        // Set room property when game is over
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "gameOver", isGameOver } });
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left the room, loading main menu.");
        PhotonNetwork.LoadLevel("Menu");  // Load the main menu scene
    }

    // Reset game objects without destroying them
    public void ResetGameObjects()
    {
        foreach (var go in FindObjectsOfType<PhotonView>())
        {
            go.gameObject.SetActive(false); // Deactivate instead of destroying
            // Reset other necessary components or properties
        }
    }

    // Call this method when leaving a room or changing scenes
    private void CleanUpScene()
    {
        Debug.Log("Initiating cleanup and leaving the room...");
        if (PhotonNetwork.IsMasterClient)
        {
            ResetGameObjects();  // Reset or deactivate game objects as needed
        }
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();  // Leave the room
        }
        else
        {
            PhotonNetwork.LoadLevel("Menu");  // Directly load the main menu if not in a room
        }
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        Debug.Log("New Master Client: " + newMasterClient.NickName);

        if (PhotonNetwork.IsMasterClient)
        {
            // Check if the game is over, and if so, set the room property accordingly
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameOver", out object status) && (bool)status)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "gameOver", true } });
            }
        }
    }

    public void LeaveRoomAndReturnToLobby()
    {
        // Set the room as "gameOver" before leaving
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "gameOver", true } });
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from the game server: " + cause);
        ConnectToPhoton();  // Attempt reconnection
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server. Now you can join or create a room.");
        PhotonNetwork.JoinLobby();  // Optionally join a lobby if needed
    }
}