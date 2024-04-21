using UnityEngine;
using Photon.Pun;

public class PlayerName : MonoBehaviourPun // Standard MonoBehaviour
{
    public string playerTypeName; // Set this in the Inspector to "Player1" or "Player2"

    // Public method to access PhotonView
    public PhotonView GetPhotonView()
    {
        return photonView; // Return the protected photonView
    }
}