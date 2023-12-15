using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class RoomManager : MonoBehaviourPunCallbacks
{
    string playerPath = "";

    [SerializeField] TMP_Text messageText;
    [SerializeField] TMP_InputField inputFieldText;

    private void Start() {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.L))
            StartGame();
    }

    #region Networking

    public void ConnectToMaster() {
        messageText.text = "Connecting...";
        PhotonNetwork.ConnectUsingSettings();

        PhotonNetwork.NickName = inputFieldText.text;
    }

    public override void OnConnectedToMaster() {
        base.OnConnectedToMaster();

        messageText.text = "Connected To Master";
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby() {
        base.OnJoinedLobby();

        PhotonNetwork.JoinOrCreateRoom("test", null, null);
        messageText.text = "We're connected in a room now";
    }

    public override void OnJoinedRoom() {
        base.OnJoinedRoom();

        messageText.text = PhotonNetwork.MasterClient.NickName + "'s Lobby\nWaiting For Players...";

        //GameObject player = PhotonNetwork.Instantiate(playerPath, Vector3.zero, Quaternion.identity);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) {
        base.OnPlayerEnteredRoom(newPlayer);

        if (!PhotonNetwork.IsMasterClient)
            return;

        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2) {
            messageText.text = "Starting Game...";
            Invoke(nameof(StartGame), 1);
        }
    }

    public void StartGame() {
        PhotonNetwork.LoadLevel(1);
    }

    #endregion
}
