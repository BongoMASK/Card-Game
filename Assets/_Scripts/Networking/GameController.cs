using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameController : MonoBehaviour, INetworkedTurnManagerCallbacks {

    [SerializeField] PhotonView PV;
    [SerializeField] NetworkedTurnManager networkedTurnManager;

    private void Start() {
        Screen.SetResolution(600, 400, false);

        networkedTurnManager.TurnManagerListener = this;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log("lets goo");

            PlayerMove move = new PlayerMove(2, 4, MoveType.Move);
            //networkedTurnManager.SendMove(move.ToByteArray(), false);
            SendMoveToMasterClient(move);
        }
    }

    #region Networked Turn Manager Functions

    public void OnPlayerFinished(Player player, int turn, object[] move) {
        throw new System.NotImplementedException();
    }

    public void OnPlayerMove(Player player, int turn, object[] move) {
        PlayerMove playerMove = PlayerMove.ToPlayerMove(move);
        PerformMove(playerMove);
    }

    public void OnTurnBegins(int turn) {
        throw new System.NotImplementedException();
    }

    public void OnTurnCompleted(int turn) {
        throw new System.NotImplementedException();
    }

    public void OnTurnTimeEnds(int turn) {
        throw new System.NotImplementedException();
    }

    #endregion

    public void SendMoveToMasterClient(PlayerMove move) {
        PV.RPC(nameof(RPC_SendMoveToMasterClient), RpcTarget.MasterClient, move.ToByteArray());
    }

    [PunRPC]
    public void RPC_SendMoveToMasterClient(object[] moveObject) {
        PlayerMove move = PlayerMove.ToPlayerMove(moveObject);
        move.Print();
        Debug.Log("RPC");
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
