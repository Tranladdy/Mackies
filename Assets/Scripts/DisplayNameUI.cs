using UnityEngine;
using Photon.Pun;
using TMPro;

public class DisplayNameUI : MonoBehaviour
{
    public TMP_Text playerNameText;
    private PhotonView playerPhotonView;

    // This method is called by the player object to pass its PhotonView
    public void Initialize(PhotonView photonView)
    {
        playerPhotonView = photonView;
    }

    void Update()
    {
        if (playerPhotonView != null && playerPhotonView.IsMine && playerNameText != null)
        {
            playerNameText.text = playerPhotonView.Owner.NickName; // Display the Photon player's nickname
        }
    }
}