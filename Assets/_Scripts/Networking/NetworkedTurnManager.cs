using System.Collections.Generic;

using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Properties;
using UnityEngine.SceneManagement;

/// <summary>
/// Pun turnBased Game manager.
/// Provides an Interface (IPunTurnManagerCallbacks) for the typical turn flow and logic, between players
/// Provides Extensions for Player, Room and RoomInfo to feature dedicated api for TurnBased Needs
/// </summary>
public class NetworkedTurnManager : MonoBehaviourPunCallbacks, IOnEventCallback {

    /// <summary>
    /// External definition for better garbage collection management, used in ProcessEvent.
    /// </summary>
    Player sender;

    /// <summary>
    /// Wraps accessing the "turn" custom properties of a room.
    /// </summary>
    /// <value>The turn index</value>
    public int Turn {
        get { return PhotonNetwork.CurrentRoom.GetTurn(); }
        private set {

            _isOverCallProcessed = false;

            PhotonNetwork.CurrentRoom.SetTurn(value, true);
        }
    }


    /// <summary>
    /// The duration of the turn in seconds.
    /// </summary>
    public float TurnDuration = 20f;

    /// <summary>
    /// Gets the elapsed time in the current turn in seconds
    /// </summary>
    /// <value>The elapsed time in the turn.</value>
    public float ElapsedTimeInTurn {
        get { return ((float)(PhotonNetwork.ServerTimestamp - PhotonNetwork.CurrentRoom.GetTurnStartTime())) / 1000.0f; }
    }


    /// <summary>
    /// Gets the remaining seconds for the current turn. Ranges from 0 to TurnDuration
    /// </summary>
    /// <value>The remaining seconds fo the current turn</value>
    public float RemainingSecondsInTurn {
        get { return Mathf.Max(0f, this.TurnDuration - this.ElapsedTimeInTurn); }
    }


    /// <summary>
    /// Gets a value indicating whether the turn is completed by all.
    /// </summary>
    /// <value><c>true</c> if this turn is completed by all; otherwise, <c>false</c>.</value>
    public bool IsCompletedByAll {
        get { return PhotonNetwork.CurrentRoom != null && Turn > 0 && this.playerOrder.Count == 0; }
    }

    /// <summary>
    /// Gets a value indicating whether the current turn is over. That is the ElapsedTimeinTurn is greater or equal to the TurnDuration
    /// </summary>
    /// <value><c>true</c> if the current turn is over; otherwise, <c>false</c>.</value>
    public bool IsOver {
        get { return this.RemainingSecondsInTurn <= 0f; }
    }

    /// <summary>
    /// The turn manager listener. Set this to your own script instance to catch Callbacks
    /// </summary>
    public INetworkedTurnManagerCallbacks TurnManagerListener;


    /// <summary>
    /// Stores Players in the order of their turns
    /// </summary>
    private Queue<Player> playerOrder = new Queue<Player>();

    /// <summary>
    /// Gets the player whose turn it is
    /// </summary>
    public Player activePlayerTurn {
        get { 
            return PhotonNetwork.CurrentRoom.GetActivePlayer();
        }
        private set {
            PhotonNetwork.CurrentRoom.SetActivePlayer(value);
        }
    }


    /// <summary>
    /// The turn manager event offset event message byte. Used internaly for defining data in Room Custom Properties
    /// </summary>
    public const byte TurnManagerEventOffset = 0;

    /// <summary>
    /// The Move event message byte. Used internaly for saving data in Room Custom Properties
    /// </summary>
    public const byte EvMove = 1 + TurnManagerEventOffset;

    /// <summary>
    /// The Final Move event message byte. Used internaly for saving data in Room Custom Properties
    /// </summary>
    public const byte EvFinalMove = 2 + TurnManagerEventOffset;

    /// <summary>
    /// The Final Move event message byte. Used internaly for saving data in Room Custom Properties
    /// </summary>
    public const byte EvCreateCard = 3 + TurnManagerEventOffset;

    // keep track of message calls
    private bool _isOverCallProcessed = false;

    #region MonoBehaviour CallBack


    void Start() { }

    void Update() {
        if (Turn > 0 && this.IsOver && !_isOverCallProcessed) {
            _isOverCallProcessed = true;
            
            this.TurnManagerListener.OnTurnTimeEnds(this.Turn);

            //if (PhotonNetwork.IsMasterClient)
            //    SendMove(null, true, activePlayerTurn);
        }
    }

    #endregion


    /// <summary>
    /// Tells the TurnManager to begins a new turn.
    /// </summary>
    public void BeginTurn() {
        Turn = this.Turn + 1; // note: this will set a property in the room, which is available to the other players.
    }


    /// <summary>
    /// Call to send an action. Optionally finish the turn, too.
    /// The move object can be anything. Try to optimize though and only send the strict minimum set of information to define the turn move.
    /// </summary>
    /// <param name="move"></param>
    /// <param name="finished"></param>
    public void SendMove(object[] move, bool finished, Player sender) {
        if (!IsPlayersTurn(sender)) {
            UnityEngine.Debug.LogWarning("Can't SendMove. Turn is finished by this player.");
            return;
        }

        // along with the actual move, we have to send which turn this move belongs to
        Hashtable moveHt = new Hashtable();
        moveHt.Add("turn", Turn);
        moveHt.Add("move", move);

        byte evCode = (finished) ? EvFinalMove : EvMove;
        PhotonNetwork.RaiseEvent(evCode, moveHt, new RaiseEventOptions() { CachingOption = EventCaching.AddToRoomCache }, SendOptions.SendReliable);

        // the server won't send the event back to the origin (by default). to get the event, call it locally
        // (note: the order of events might be mixed up as we do this locally)
        ProcessOnEvent(evCode, moveHt, sender.ActorNumber);
    }

    public void SendNewCardToAll(object[] cardData, Player owner) {
        // along with the actual move, we have to send which turn this move belongs to
        Hashtable cardHt = new Hashtable();
        cardHt.Add("turn", Turn);
        cardHt.Add("cardData", cardData);
        cardHt.Add("owner", owner.ActorNumber);

        PhotonNetwork.RaiseEvent(EvCreateCard, cardHt, new RaiseEventOptions() { CachingOption = EventCaching.AddToRoomCache }, SendOptions.SendReliable);

        ProcessOnEvent(EvCreateCard, cardHt, owner.ActorNumber);
    }

    /// <summary>
    /// Gets if the player finished the current turn.
    /// </summary>
    /// <returns><c>true</c>, if player finished the current turn, <c>false</c> otherwise.</returns>
    /// <param name="player">The Player to check for</param>
    public bool GetPlayerFinishedTurn(Player player) {
        if (player != null && !this.playerOrder.Contains(player))
            return true;

        return false;
    }

    /// <summary>
    /// Sets player turn order
    /// </summary>
    public void SetPlayerOrder() {
        List<Player> list = new List<Player>(PhotonNetwork.PlayerList);

        // sorting list based on mana
        for (int i = 0; i < list.Count - 1; i++) {
            bool swapped = false;
            for (int j = 0; j < list.Count - i - 1; j++) {
                if ((int)list[j].CustomProperties[PlayerProps.ManaPropKey] < (int)list[j+1].CustomProperties[PlayerProps.ManaPropKey]) {
                    Player t = list[j];
                    list[j] = list[j + 1];
                    list[j + 1] = t;

                    swapped = true;
                }
            }

            if (!swapped)
                break;
        }

        playerOrder = new Queue<Player>(list);
    }

    public void SetActivePlayer() {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (playerOrder.TryPeek(out Player result))
            activePlayerTurn = result;
        else
            Debug.LogError("Could not set player turn");

        //Debug.Log(activePlayerTurn.NickName + "'s Turn");
    }

    public bool IsPlayersTurn(Player player) {
        if(activePlayerTurn == null) 
            return true;

        return player.ActorNumber == activePlayerTurn.ActorNumber;
    }

    
    #region Callbacks

    // called internally
    void ProcessOnEvent(byte eventCode, object content, int senderId) {
        if (senderId == -1) {
            return;
        }

        sender = PhotonNetwork.CurrentRoom.GetPlayer(senderId);

        switch (eventCode) {
            case EvMove: {
                    Hashtable evTable = content as Hashtable;
                    int turn = (int)evTable["turn"];
                    object[] move = (object[])evTable["move"];

                    this.TurnManagerListener.OnPlayerMove(sender, turn, move);

                    break;
                }

            case EvFinalMove: {
                    Hashtable evTable = content as Hashtable;
                    int turn = (int)evTable["turn"];
                    object[] move = (object[])evTable["move"];

                    if (turn == this.Turn) {
                        if (!this.playerOrder.TryDequeue(out Player result))
                            Debug.LogError("Dequeue in player list failed.");

                        this.TurnManagerListener.OnPlayerFinished(sender, turn, move);
                    }

                    if (IsCompletedByAll) {
                        this.TurnManagerListener.OnTurnCompleted(this.Turn);
                        BeginTurn();
                    }
                    else {
                        SetActivePlayer();
                    }
                    break;
                }

            case EvCreateCard: {
                    Hashtable evTable = content as Hashtable;
                    int turn = (int)evTable["turn"];
                    object[] cardData = (object[])evTable["cardData"];

                    this.TurnManagerListener.OnCardCreated(sender, turn, cardData);

                    break;
                }
        }
    }

    /// <summary>
    /// Called by PhotonNetwork.OnEventCall registration
    /// </summary>
    /// <param name="photonEvent">Photon event.</param>
    public void OnEvent(EventData photonEvent) {
        this.ProcessOnEvent(photonEvent.Code, photonEvent.CustomData, photonEvent.Sender);
    }

    /// <summary>
    /// Called by PhotonNetwork
    /// </summary>
    /// <param name="propertiesThatChanged">Properties that changed.</param>
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {

        if (propertiesThatChanged.ContainsKey("Turn")) {
            Debug.Log("Turn: " + Turn);

            _isOverCallProcessed = false;

            SetPlayerOrder();

            this.TurnManagerListener.OnTurnBegins(this.Turn);
            SetActivePlayer();
        }

        if (propertiesThatChanged.ContainsKey("ActivePlayer")) {
            this.TurnManagerListener.OnPlayerTurnStarts(activePlayerTurn, Turn);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient) {
        base.OnMasterClientSwitched(newMasterClient);

        // Fail safe
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

    #endregion
}


public interface INetworkedTurnManagerCallbacks {
    /// <summary>
    /// Called the turn begins event.
    /// </summary>
    /// <param name="turn">Turn Index</param>
    void OnTurnBegins(int turn);

    /// <summary>
    /// Called when a turn is completed (finished by all players)
    /// </summary>
    /// <param name="turn">Turn Index</param>
    void OnTurnCompleted(int turn);

    /// <summary>
    /// Called when the players turn starts
    /// </summary>
    /// <param name="player"></param>
    /// <param name="turn"></param>
    /// <param name="OnPlayerMove"></param>
    /// <param name=""></param>
    void OnPlayerTurnStarts(Player player, int turn);

    /// <summary>
    /// Called when a player moved (but did not finish the turn)
    /// </summary>
    /// <param name="player">Player reference</param>
    /// <param name="turn">Turn Index</param>
    /// <param name="move">Move Object data</param>
    void OnPlayerMove(Player player, int turn, object[] move);

    /// <summary>
    /// When a player finishes a turn (includes the action/move of that player)
    /// </summary>
    /// <param name="player">Player reference</param>
    /// <param name="turn">Turn index</param>
    /// <param name="move">Move Object data</param>
    void OnPlayerFinished(Player player, int turn, object[] move);

    /// <summary>
    /// Called when a turn completes due to a time constraint (timeout for a turn)
    /// </summary>
    /// <param name="turn">Turn index</param>
    void OnTurnTimeEnds(int turn);

    /// <summary>
    /// Called when a new card is created
    /// </summary>
    /// <param name="turn">Turn index</param>
    void OnCardCreated(Player owner, int turn, object[] cardData);
}


public static class TurnExtensions {
    /// <summary>
    /// currently ongoing turn number
    /// </summary>
    public static readonly string TurnPropKey = "Turn";

    /// <summary>
    /// start (server) time for currently ongoing turn (used to calculate end)
    /// </summary>
    public static readonly string TurnStartTimePropKey = "TStart";

    /// <summary>
    /// Finished Turn of Actor (followed by number)
    /// </summary>
    public static readonly string FinishedTurnPropKey = "FToA";

    /// <summary>
    /// get which players turn it is
    /// </summary>
    public static readonly string ActivePlayerPropKey = "ActivePlayer";

    /// <summary>
    /// Sets the turn.
    /// </summary>
    /// <param name="room">Room reference</param>
    /// <param name="turn">Turn index</param>
    /// <param name="setStartTime">If set to <c>true</c> set start time.</param>
    public static void SetTurn(this Room room, int turn, bool setStartTime = false) {
        if (room == null || room.CustomProperties == null) {
            return;
        }

        Hashtable turnProps = new Hashtable();
        turnProps[TurnPropKey] = turn;
        if (setStartTime) {
            turnProps[TurnStartTimePropKey] = PhotonNetwork.ServerTimestamp;
        }

        room.SetCustomProperties(turnProps);
    }

    /// <summary>
    /// Gets the current turn from a RoomInfo
    /// </summary>
    /// <returns>The turn index </returns>
    /// <param name="room">RoomInfo reference</param>
    public static int GetTurn(this RoomInfo room) {
        if (room == null || room.CustomProperties == null || !room.CustomProperties.ContainsKey(TurnPropKey)) {
            return 0;
        }

        return (int)room.CustomProperties[TurnPropKey];
    }


    /// <summary>
    /// Returns the start time when the turn began. This can be used to calculate how long it's going on.
    /// </summary>
    /// <returns>The turn start.</returns>
    /// <param name="room">Room.</param>
    public static int GetTurnStartTime(this RoomInfo room) {
        if (room == null || room.CustomProperties == null || !room.CustomProperties.ContainsKey(TurnStartTimePropKey)) {
            return 0;
        }

        return (int)room.CustomProperties[TurnStartTimePropKey];
    }

    /// <summary>
    /// Sets the player whose turn it should be.
    /// </summary>
    /// <param name="room">Room reference</param>
    /// <param name="turn">Turn index</param>
    /// <param name="setStartTime">If set to <c>true</c> set start time.</param>
    public static void SetActivePlayer(this Room room, Player player) {
        if (room == null || room.CustomProperties == null) {
            return;
        }

        Hashtable turnProps = new Hashtable();
        turnProps[ActivePlayerPropKey] = player;
        turnProps[TurnStartTimePropKey] = PhotonNetwork.ServerTimestamp;

        room.SetCustomProperties(turnProps);
    }

    /// <summary>
    /// Gets the player whose turn it should be.
    /// </summary>
    /// <param name="room"></param>
    /// <returns></returns>
    public static Player GetActivePlayer(this RoomInfo room) {
        if (room == null || room.CustomProperties == null || !room.CustomProperties.ContainsKey(ActivePlayerPropKey)) {
            Debug.LogError("Could not get Active Player");
            return null;
        }
        
        return (Player)room.CustomProperties[ActivePlayerPropKey];
    }

}