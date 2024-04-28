using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
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
            PhotonNetwork.ConnectUsingSettings();  // Only connect if not already connected
        }
    }

    public void Connect()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();  // Try joining a room if already connected to Photon
        }
        else
        {
            ConnectToPhoton();  // Attempt to connect to Photon if disconnected
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Tried to join a room and failed, creating a new room.");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a room!");
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(4);
        }
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
            // The new master client might need to perform cleanup or other actions.
        }
    }

    public void LeaveRoomAndReturnToLobby()
    {
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