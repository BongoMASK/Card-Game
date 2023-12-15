using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour {
    public List<BaseCard> activeCards = new List<BaseCard>();
    public List<CardPlacer> activeCardPlacers = new List<CardPlacer>();

    public Dictionary<Player, Deck> playerDecks = new Dictionary<Player, Deck>();

    [Header("Assignables")]

    [SerializeField] Deck localPlayerDeck;
    [SerializeField] PhotonView PV;
    [SerializeField] NetworkedTurnManager networkedTurnManager;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.V)) {
            SendDeckToMasterClient();
        }

        if(Input.GetKeyDown(KeyCode.S) && PhotonNetwork.IsMasterClient) {
            CreateCardFromPlayerDeck(PhotonNetwork.MasterClient);
        }
    }

    public void CreateCardFromPlayerDeck(Player player) {
        if (!PhotonNetwork.IsMasterClient) {
            return;
        }

        // get card stats
        CardType c = playerDecks[player].GetCardFromDeck();

        object[] cardData = new object[] { c, BaseCard.id };

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
