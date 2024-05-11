using UnityEngine;
using Photon.Pun;
using PlayFab;

public class PlayerName : MonoBehaviourPun
{
    public string playerTypeName; // Set this in the Inspector to "Player1" or "Player2"

    void Start()
    {
        if (photonView.IsMine) // Ensure we are only setting the name for the local player
        {
            SetPhotonPlayerName();
        }
    }

    void SetPhotonPlayerName()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            string displayName = PlayfabManager.UserSessionInfo.DisplayName;
            if (!string.IsNullOrEmpty(displayName))
            {
                PhotonNetwork.NickName = displayName; // Set Photon Network NickName
                photonView.Owner.NickName = displayName; // Set the owner's nickname
            }
            else
            {
                PhotonNetwork.NickName = "Guest"; // Fallback if no display name is set
                photonView.Owner.NickName = "Guest";
            }
        }
    }

    public PhotonView GetPhotonView()
    {
        return photonView;
    }
}