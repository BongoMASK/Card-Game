using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameController : MonoBehaviour, INetworkedTurnManagerCallbacks {

    public static GameController instance;

    [SerializeField] PhotonView PV;
    [SerializeField] NetworkedTurnManager networkedTurnManager; 
    [SerializeField] CardFunctions cardFunctions;

    private void Awake() {
        instance = this;
    }

    private void Start() {
        Screen.SetResolution(711, 400, false);
        if (!PhotonNetwork.IsMasterClient)
            Camera.main.transform.rotation = Quaternion.Euler(0, 0, -180);
        

        BaseCard.id = 0;
        networkedTurnManager.TurnManagerListener = this;
    }

    #region Networked Turn Manager Functions

    public void OnPlayerTurnStarts(Player player, int turn) {
        Debug.Log(player.NickName + "'s turn");
    }

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

        ResetAllCards();

        GameData.instance.CheckIfNewCardsNeeded();
        GameData.instance.ResetAllPlayersValues();
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
        int cardPlacerID = (int)cardData[2];

        Debug.Log(cardType + ", " + id);

        cardFunctions.CreateCard(id, cardType, cardPlacerID, owner);
    }

    #endregion

    public void SendMoveToMasterClient(PlayerMove move, bool finished) {
        PV.RPC(nameof(RPC_SendMoveToMasterClient), RpcTarget.MasterClient, move.ToByteArray(), finished, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    public void RPC_SendMoveToMasterClient(object[] moveObject, bool finished, int actorNumber) {
        PlayerMove move = PlayerMove.ToPlayerMove(moveObject);
        move.Print();

        BaseCard card = BaseCard.FindCard(move.cardID);
        CardPlacer cardPlacer = CardPlacer.FindCardPlacer(move.cardID);

        Player p = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);

        bool isMoveValid = true;

        //switch(move.moveType) {
        //    case MoveType.Move:
        //        isMoveValid = cardFunctions.ValidateMovement(card, cardPlacer, p);
        //        break;

        //    case MoveType.Attack:
        //        isMoveValid = cardFunctions.ValidateAttack(card, cardPlacer, p);
        //        break;

        //    default:
        //        isMoveValid = false;
        //        break;
        //}

        if (isMoveValid) {
            networkedTurnManager.SendMove(move.ToByteArray(), finished, PhotonNetwork.CurrentRoom.GetPlayer(actorNumber));
        }
        else
            Debug.LogWarning("Move sent by " + p.NickName + " is not valid.");
    }

    void PerformMove(PlayerMove move) {
        BaseCard card = BaseCard.FindCard(move.cardID);
        CardPlacer cardPlacer = CardPlacer.FindCardPlacer(move.cardPlacerID);
        move.Print();

        switch(move.moveType) {
            case MoveType.Move:
                cardFunctions.MoveCard(card, cardPlacer, true);
                break;
            case MoveType.Attack:
                cardFunctions.Attack(card, cardPlacer);
                break;
            case MoveType.Swap:
                cardFunctions.SwapCards(card, cardPlacer, true);
                break;
        }
    }

    private void ResetAllCards() {
        foreach (var item in FindObjectsOfType<BaseCard>()) {
            item.hasBeenMoved = false;
            item.hasAttacked = false;
        }
    }
}
