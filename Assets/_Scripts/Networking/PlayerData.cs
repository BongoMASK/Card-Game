using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Properties;
using UnityEngine;

public class PlayerData : MonoBehaviourPunCallbacks
{
    //[SerializeField] PhotonView PV;
    [SerializeField] int playerIndex = 0;
    
    public HandCardPlacer[] handCardPlacers;
    
    public Player player => PhotonNetwork.PlayerList[playerIndex];

    public string username => player.NickName;

    public const int maxMana = 12;

    public int mana {
        get => (int)player.CustomProperties[PlayerProps.ManaPropKey];
        set {
            SetMana(value);
        }
    }

    public int currentMaxRoundMana {
        get => (int)player.CustomProperties[PlayerProps.MaxManaPropKey];
        set {
            SetRoundMaxMana(value);
        }
    }

    public bool hasGivenCardToManaZone {
        get => (bool)player.CustomProperties[PlayerProps.hasPutInManaZonePropKey];
        set {
            SetHasPutInManaZone(value);
        }
    }

    public bool hasMoved {
        get => (bool)player.CustomProperties[PlayerProps.hasMovedPropKey];
        set {
            SetHasMoved(value);
        }
    }

    public bool hasAttacked {
        get => (bool)player.CustomProperties[PlayerProps.hasAttackedPropKey];
        set {
            SetHasAttacked(value);
        }
    }

    public bool hasPlacedCard {
        get => (bool)player.CustomProperties[PlayerProps.hasPlacedCardPropKey];
        set {
            SetHasPlacedCard(value);
        }
    }

    public bool lockInput => PhotonNetwork.CurrentRoom.CustomProperties[RoomProps.ActivePlayerPropKey] != player;

    #region Update Player Props Funcs

    private void SetMana(int value) {
        Room room = PhotonNetwork.CurrentRoom;
        if (room == null || room.CustomProperties == null) {
            return;
        }

        Hashtable playerProps = new Hashtable();
        playerProps[PlayerProps.ManaPropKey] = value;

        player.SetCustomProperties(playerProps);
    }

    private void SetRoundMaxMana(int value) {
        Room room = PhotonNetwork.CurrentRoom;
        if (room == null || room.CustomProperties == null) {
            return;
        }

        Hashtable playerProps = new Hashtable();
        playerProps[PlayerProps.MaxManaPropKey] = value;

        player.SetCustomProperties(playerProps);
    }

    private void SetHasPutInManaZone(bool value) {
        Room room = PhotonNetwork.CurrentRoom;
        if (room == null || room.CustomProperties == null) {
            return;
        }

        Hashtable playerProps = new Hashtable();
        playerProps[PlayerProps.hasPutInManaZonePropKey] = value;

        player.SetCustomProperties(playerProps);
    }

    private void SetHasMoved(bool value) {
        Room room = PhotonNetwork.CurrentRoom;
        if (room == null || room.CustomProperties == null) {
            return;
        }

        Hashtable playerProps = new Hashtable();
        playerProps[PlayerProps.hasMovedPropKey] = value;

        player.SetCustomProperties(playerProps);
    }

    private void SetHasAttacked(bool value) {
        Room room = PhotonNetwork.CurrentRoom;
        if (room == null || room.CustomProperties == null) {
            return;
        }

        Hashtable playerProps = new Hashtable();
        playerProps[PlayerProps.hasAttackedPropKey] = value;

        player.SetCustomProperties(playerProps);
    }
     
    private void SetHasPlacedCard(bool value) {
        Room room = PhotonNetwork.CurrentRoom;
        if (room == null || room.CustomProperties == null) {
            return;
        }

        Hashtable playerProps = new Hashtable();
        playerProps[PlayerProps.hasPlacedCardPropKey] = value;

        player.SetCustomProperties(playerProps);
    }

    #endregion

    private void Start() {
        Invoke(nameof(SetUpPlayerData), 0.2f);
    }

    private void SetUpPlayerData() {
        if (!PhotonNetwork.IsMasterClient)
            return;

        mana = 0;
        currentMaxRoundMana = 0;

        hasGivenCardToManaZone = false;
        hasMoved = false;
        hasAttacked = false;
        hasPlacedCard = false;
    }

    public void ResetBools() {
        if (!PhotonNetwork.IsMasterClient)
            return;

        hasGivenCardToManaZone = false;
        hasMoved = false;
        hasAttacked = false;
        hasPlacedCard = false;

        mana = currentMaxRoundMana;
    }

    private void FindPlayer() {
        //player = PhotonView.Find(PV.ViewID).Owner;
    }

    public void UsedMana(int amount) {
        mana -= amount;
    }

    public static PlayerData FindPlayerData(Player p) {
        foreach (var item in FindObjectsOfType<PlayerData>()) {
            if (item.player == p)
                return item;
        }

        return null;
    }

    public static PlayerData FindPlayerData(int i) {
        foreach (var item in FindObjectsOfType<PlayerData>()) {
            if (item.playerIndex == i)
                return item;
        }

        return null;
    }

    #region PUN Callbacks

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
    }

    #endregion
}
