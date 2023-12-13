using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// TODO:
// Make separate files for each mechanic in this script
// Make Turn states better and compatible with more players

public enum TurnState { TurnStart, PlayerTurn, OtherPlayerTurn, TurnEnd }

public class GameManager : MonoBehaviour {

    #region Variables

    public static GameManager instance;

    [SerializeField] TurnState _currentTurnState;

    public int countdownTimer { get; private set; } = 0;

    public TurnState currentTurnState {
        get => _currentTurnState;

        set {
            _currentTurnState = value;
            Debug.Log(_currentTurnState);
            TurnStateActions(_currentTurnState);
        }
    }

    public delegate void TurnEvents();

    public event TurnEvents OnTurnBegin;
    public event TurnEvents OnPlayerTurn;
    public event TurnEvents OnOtherTurn;
    public event TurnEvents OnTurnEnd;

    public int roundNumber = 1;

    public int maxCountdownTimer { get; private set; } = 5;

    [Header("Assignables")]

    #region User Logic

    [SerializeField] User _user1;
    [SerializeField] User _user2;

    public User user1 { get => _user1; }
    public User user2 { get => _user2; }

    // This is for the more mana system. 
    // TODO: put it in its own separate file.
    User moreManaUser;
    User _currentUser;
    public User currentUser {
        get {
            return _currentUser;
        }
        private set {
            _currentUser = value;
            ShowCurrentTurn(currentUser);
        }
    }

    #endregion

    #region Cards Logic

    // Gets called when the card is selected. Networking should mainly just carry this forward.
    private BaseCard _currentSelectedCard;
    public BaseCard currentSelectedCard {
        get => _currentSelectedCard;

        set {
            if (CardValidator.instance.attackSystem.ValidateAttack(_currentSelectedCard, value)) {
                return;
            }

            if (_currentSelectedCard != null)
                _currentSelectedCard.OnDeselected();

            _currentSelectedCard = value;

            if (_currentSelectedCard != null) {
                _currentSelectedCard.OnSelected();
            }

            cardData.SetUpCardData(_currentSelectedCard);
        }
    }

    #endregion

    #region UI Stuff

    [SerializeField] CardData cardData;

    [SerializeField] TMP_Text roundText;

    [Header("Turn Shower")]

    [SerializeField] Transform turnShower;

    [SerializeField] Transform user1ShowTurnPos;
    [SerializeField] Transform user2ShowTurnPos;

    [Header("Turn Priority")]

    [SerializeField] Transform turnPriorityIcon;

    [SerializeField] Transform user1TurnPriorityIconPos;
    [SerializeField] Transform user2TurnPriorityIconPos;

    [SerializeField] TMP_Text messageText;

    #endregion

    #endregion

    private void Awake() {
        instance = this;

        OnTurnBegin += TurnBeginCall;
        OnPlayerTurn += StartPlayer1Turn;
        OnOtherTurn += StartOtherPlayerTurn;
        OnTurnEnd += TurnEnding;
    }

    private void Start() {
        FindMoreManaUser();
        currentTurnState = TurnState.TurnStart;
    }

    #region Turn Stuff

    public void SetTurn(TurnState move) {
        //if (!PhotonNetwork.IsMasterClient)
        //    return;

        currentTurnState = move;

        //Hashtable ht = PhotonNetwork.CurrentRoom.CustomProperties;

        //ht.Remove(RoomProps.currentTurnState);
        //ht.Add(RoomProps.currentTurnState, (int)currentTurnState);

        //PhotonNetwork.CurrentRoom.SetCustomProperties(ht);

        //PV.RPC("RPC_GetTurn", RpcTarget.All);
    }

    void TurnStateActions(TurnState currentTurnState) {
        switch (currentTurnState) {
            case TurnState.TurnStart:
                OnTurnBegin?.Invoke();
                break;

            case TurnState.PlayerTurn:
                OnPlayerTurn?.Invoke();
                break;

            case TurnState.OtherPlayerTurn:
                OnOtherTurn?.Invoke();
                break;

            case TurnState.TurnEnd:
                OnTurnEnd?.Invoke();
                break;

            default:
                Debug.LogError("No Turn called");
                break;
        }
    }

    #region Turn Start

    void TurnBeginCall() {
        roundText.text = "Round " + roundNumber;

        SetTurn(TurnState.PlayerTurn);
    }

    public void FindMoreManaUser() {
        moreManaUser = FindUserWithMoreMana();

        // Do something to denote more mana user
        ShowCurrentTurnPriorityUser(moreManaUser);
    }

    void ShowCurrentTurnPriorityUser(User user) {
        if (user == user1)
            turnPriorityIcon.position = user1TurnPriorityIconPos.position;

        else
            turnPriorityIcon.position = user2TurnPriorityIconPos.position;
    }

    User FindUserWithMoreMana() {
        int u1Mana = user1.currentRoundMana;
        int u2Mana = user2.currentRoundMana;

        if (u1Mana == u2Mana)
            return GetRandomUser();

        if (u1Mana > u2Mana)
            return user1;

        return user2;
    }

    User GetRandomUser() {
        int rand = Random.Range(0, 2);
        if (rand == 0)
            return user1;

        return user2;
    }

    #endregion

    #region Turn Play

    void StartPlayer1Turn() {
        currentSelectedCard = null;

        //currentUser = user1;
        currentUser = moreManaUser;

        currentUser.PlayTurn();
    }

    void StartOtherPlayerTurn() {
        currentSelectedCard = null;
        currentUser.lockInput = true;

        //currentUser = user2;
        if (user1 == currentUser)
            currentUser = user2;
        else
            currentUser = user1;

        currentUser.PlayTurn();
    }

    void ShowCurrentTurn(User user) {
        if (user == user1) {
            turnShower.position = user1ShowTurnPos.position;
        }
        else
            turnShower.position = user2ShowTurnPos.position;
    }

    #endregion

    #region Turn End

    void TurnEnding() {
        currentUser.lockInput = true;
        roundNumber++;

        SetTurn(TurnState.TurnStart);
    }

    #endregion

    #endregion

    #region Button Funcs

    public void GetNextTurnState() {
        //currentUser.lockInput = true;

        if (currentTurnState == TurnState.PlayerTurn)
            SetTurn(TurnState.OtherPlayerTurn);
        else if (currentTurnState == TurnState.OtherPlayerTurn)
            SetTurn(TurnState.TurnEnd);
    }

    public void ResetGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    #endregion

    public void SetMessageError(string message) {
        AudioManager.instance.Play(SoundNames.error);
        SetMessage("<#A44343>" + message);
    }

    Coroutine msgCor;

    public void SetMessage(string message) {
        messageText.text = message;

        if (msgCor != null)
            StopCoroutine(msgCor);

        msgCor = StartCoroutine(RemoveMessage());
    }

    IEnumerator RemoveMessage() {
        yield return new WaitForSeconds(5);
        messageText.text = "";
    }
}