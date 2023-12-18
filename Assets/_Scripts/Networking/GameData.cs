using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
//using UnityEditor;
//using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;

public class GameData : MonoBehaviour {

    public static GameData instance { get; private set; }

    public List<BaseCard> activeCards = new List<BaseCard>();
    public List<CardPlacer> activeCardPlacers = new List<CardPlacer>();

    public Dictionary<Player, Deck> playerDecks = new Dictionary<Player, Deck>();

    private BaseCard _currentSelectedCard;

    public BaseCard currentSelectedCard {
        get => _currentSelectedCard;

        set {

            if (CardFunctions.instance.ValidateAttack(_currentSelectedCard, value)) {
                return;
            }

            if (_currentSelectedCard != null)
                _currentSelectedCard.OnDeselected();

            _currentSelectedCard = value;

            if (_currentSelectedCard != null) {
                _currentSelectedCard.OnSelected();
                 OnCardSelected?.Invoke(_currentSelectedCard);
            }

            //cardData.SetUpCardData(_currentSelectedCard);
        }
    }

    [HideInInspector]
    public UnityEvent<BaseCard> OnCardSelected;

    [Header("Assignables")]

    [SerializeField] Deck localPlayerDeck;
    [SerializeField] PhotonView PV;
    [SerializeField] NetworkedTurnManager networkedTurnManager;

    private void Awake() {
        instance = this;
    }

    private void Start() {
        Invoke(nameof(SendDeckToMasterClient), 1);
    }

    public void CheckIfNewCardsNeeded() {
        if (!PhotonNetwork.IsMasterClient)
            return;

        foreach (var item in FindObjectsOfType<HandCardPlacer>()) {
            if (item.currentCard == null)
                CreateCardFromPlayerDeck(item.id, item.owner.player);
        }
    }

    public void CreateCardFromPlayerDeck(int cpID, Player player) {
        if (!PhotonNetwork.IsMasterClient) {
            return;
        }

        // get card stats
        CardType c = playerDecks[player].GetCardFromDeck();

        object[] cardData = new object[] { c, BaseCard.id, cpID };

        // send new card to all using game controller
        networkedTurnManager.SendNewCardToAll(cardData, player);
    }

    public BaseCard FindCard(int id) {
        foreach (BaseCard item in activeCards) {
            if (item.cardID == id)
                return item;
        }

        Debug.LogWarning("Could not find cardID");
        return null;
    }

    public void ResetAllPlayersValues() {
        foreach (var item in FindObjectsOfType<PlayerData>()) {
            item.ResetBools();
        }
    }

    public void SendDeckToMasterClient() {
        object[] o = localPlayerDeck.ToByteArray();
        PV.RPC(nameof(RPC_SendDeckToMasterClient), RpcTarget.MasterClient, o, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    public void RPC_SendDeckToMasterClient(object[] deckArray, int sender) {
        Deck deck = Deck.ToDeck(deckArray);
        Player p = PhotonNetwork.CurrentRoom.GetPlayer(sender);
        playerDecks[p] = deck;

        playerDecks[p].Print();
    }
}

//[CustomEditor(typeof(GameData))]
//public class GameDataEditor : Editor {

//    GameData gameData;

//    public override void OnInspectorGUI() {
//        base.OnInspectorGUI();

//        if (GUILayout.Button("Set Card Placers")) {
//            gameData = (GameData)target;
//            SetCardPlacers();
//        }
//    }

//    private void SetCardPlacers() {
//        gameData.activeCardPlacers = new List<CardPlacer>(FindObjectsOfType<CardPlacer>());

//        EditorUtility.SetDirty(gameData);
//        EditorSceneManager.MarkSceneDirty(gameData.gameObject.scene);
//    }
//}