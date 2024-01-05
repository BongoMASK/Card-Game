using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

public class GameController : MonoBehaviour, INetworkedTurnManagerCallbacks {

    public static GameController instance;

    [SerializeField] PhotonView PV;
    [SerializeField] NetworkedTurnManager networkedTurnManager;
    [SerializeField] CardFunctions cardFunctions;

    [HideInInspector] public UnityEvent<Player, int> Ev_OnPlayerTurnStarts;
    [HideInInspector] public UnityEvent<Player, int, object[]> Ev_OnPlayerFinished;
    [HideInInspector] public UnityEvent<Player, int, object[]> Ev_OnPlayerMove;
    [HideInInspector] public UnityEvent<Player, int, object[]> Ev_OnCardCreated;
    [HideInInspector] public UnityEvent<int> Ev_OnTurnBegins;
    [HideInInspector] public UnityEvent<int> Ev_OnTurnCompleted;
    [HideInInspector] public UnityEvent<int> Ev_OnTurnTimeEnds;

    private void Awake() {
        instance = this;
    }

    private void Start() {
        if (!PhotonNetwork.IsMasterClient)
            Camera.main.transform.rotation = Quaternion.Euler(0, 0, -180);
        
        BaseCard.id = 0;
        networkedTurnManager.TurnManagerListener = this;
    }

    #region Networked Turn Manager Functions

    public void OnPlayerTurnStarts(Player player, int turn) {
        Debug.Log(player.NickName + "'s turn");

        Ev_OnPlayerTurnStarts?.Invoke(player, turn);
    }

    public void OnPlayerFinished(Player player, int turn, object[] move) {
        Debug.Log(player.NickName + "'s turn is over");

        Ev_OnPlayerFinished?.Invoke(player, turn, move);
    }

    public void OnPlayerMove(Player player, int turn, object[] move) {
        Debug.Log(player.NickName + " played a move");

        PlayerMove playerMove = PlayerMove.ToPlayerMove(move);
        PerformMove(playerMove);

        Ev_OnPlayerMove?.Invoke(player, turn, move);
    }

    public void OnTurnBegins(int turn) {
        Debug.Log("Turn Started");

        ResetAllCards();

        GameData.instance.CheckIfNewCardsNeeded();
        ResetAllPlayersValues();

        Ev_OnTurnBegins?.Invoke(turn);
    }

    public void OnTurnCompleted(int turn) {
        Debug.Log("Turn Finished");

        Ev_OnTurnCompleted?.Invoke(turn);
    }

    public void OnTurnTimeEnds(int turn) {
        Debug.Log("Turn time ended");

        Ev_OnTurnTimeEnds?.Invoke(turn);
    }

    public void OnCardCreated(Player owner, int turn, object[] cardData) {
        CardType cardType = (CardType)cardData[0];
        int id = (int)cardData[1];
        int cardPlacerID = (int)cardData[2];

        Debug.Log(cardType + ", " + id);

        cardFunctions.CreateCard(id, cardType, cardPlacerID, owner);

        Ev_OnCardCreated.Invoke(owner, turn, cardData);
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

        if (!PhotonNetwork.IsMasterClient) {
            switch (move.moveType) {
                case MoveType.Move:
                    isMoveValid = cardFunctions.ValidateMovement(card, cardPlacer, p);
                    break;

                case MoveType.Swap:
                    isMoveValid = cardFunctions.ValidateMovement(card, cardPlacer, p);
                    break;

                case MoveType.Attack:
                    isMoveValid = cardFunctions.ValidateAttack(card, cardPlacer, p);
                    break;

                case MoveType.Finish:
                    isMoveValid = p == PhotonNetwork.CurrentRoom.GetActivePlayer();
                    break;

                default:
                    isMoveValid = false;
                    break;
            }
        }

        if (isMoveValid) {
            networkedTurnManager.SendMove(move.ToByteArray(), finished, p);
        }
        else
            Debug.LogWarning("Move sent by " + p.NickName + " is not valid. Player may be cheating.");
    }

    void PerformMove(PlayerMove move) {
        BaseCard card = BaseCard.FindCard(move.cardID);
        CardPlacer cardPlacer = CardPlacer.FindCardPlacer(move.cardPlacerID);
        move.Print();

        switch(move.moveType) {
            case MoveType.Move:
                card.MoveCard(cardPlacer, true);
                //cardFunctions.MoveCard(card, cardPlacer, true);
                break;
            case MoveType.Swap:
                card.MoveCard(cardPlacer, true);
                //cardFunctions.SwapCards(card, cardPlacer, true);
                break;
            case MoveType.Attack:
                card.Attack(cardPlacer.currentCard);
                //cardFunctions.Attack(card, cardPlacer);
                break;
        }
    }

    private void ResetAllCards() {
        foreach (var item in FindObjectsOfType<BaseCard>()) {
            item.hasBeenMoved = false;
            item.hasAttacked = false;
        }
    }

    public void ResetAllPlayersValues() {
        if (!PhotonNetwork.IsMasterClient)
            return;

        foreach (var item in FindObjectsOfType<PlayerData>()) {
            item.ResetBools();
        }
    }
}
