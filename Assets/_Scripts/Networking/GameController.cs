using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameController : MonoBehaviour, INetworkedTurnManagerCallbacks {

    [SerializeField] PhotonView PV;
    [SerializeField] NetworkedTurnManager networkedTurnManager;

    private void Start() {
        Screen.SetResolution(711, 400, false);

        networkedTurnManager.TurnManagerListener = this;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log("lets goo");

            PlayerMove move = new PlayerMove(2, 4, MoveType.Move);
            //networkedTurnManager.SendMove(move.ToByteArray(), false);
            SendMoveToMasterClient(move, false);
        }

        if(Input.GetKeyDown(KeyCode.K)) {
            networkedTurnManager.BeginTurn();
        }

        if(Input.GetKeyDown(KeyCode.H)) {
            Debug.Log("ending");

            PlayerMove move = new PlayerMove(8, 1, MoveType.Move);
            //networkedTurnManager.SendMove(move.ToByteArray(), false);
            SendMoveToMasterClient(move, true);
        }
    }

    #region Networked Turn Manager Functions

    public void OnPlayerFinished(Player player, int turn, object[] move) {
        Debug.Log(player.NickName + "'s turn is over");
    }

    public void OnPlayerMove(Player player, int turn, object[] move) {
        Debug.Log(player.NickName + " played a move");

        PlayerMove playerMove = PlayerMove.ToPlayerMove(move);
        PerformMove(playerMove);
    }

    public void OnTurnBegins(int turn) {
        Debug.Log("Turn Started");
    }

    public void OnTurnCompleted(int turn) {
        Debug.Log("Turn Finished");
    }

    public void OnTurnTimeEnds(int turn) {
        Debug.Log("Turn time ended");
    }

    public void OnCardCreated(Player owner, int turn, object[] cardData) {
        CardType cardType = (CardType)cardData[0];
        int id = (int)cardData[1];

        Debug.Log(cardType + ", " + id);
    }

    #endregion

    public void SendMoveToMasterClient(PlayerMove move, bool finished) {
        PV.RPC(nameof(RPC_SendMoveToMasterClient), RpcTarget.MasterClient, move.ToByteArray(), finished, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    public void RPC_SendMoveToMasterClient(object[] moveObject, bool finished, int actorNumber) {
        PlayerMove move = PlayerMove.ToPlayerMove(moveObject);
        move.Print();

        networkedTurnManager.SendMove(move.ToByteArray(), finished, PhotonNetwork.CurrentRoom.GetPlayer(actorNumber));
    }

    void PerformMove(PlayerMove playerMove) {
        playerMove.Print();
    }

    void Attack(BaseCard attacker, CardPlacer target) {

    }

    void NormalCardMovement() {

    }

    void CardIntoPlayMovement() {

    }

    
}
