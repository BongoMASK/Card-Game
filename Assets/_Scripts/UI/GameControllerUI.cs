using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Properties;
using System.Collections;
using TMPro;
//using UnityEditor;
//using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class GameControllerUI : MonoBehaviourPunCallbacks
{
    public static GameControllerUI instance;

    [Header("UI")]

    [SerializeField] CardData cardData;

    [SerializeField] TMP_Text roundText;
    [SerializeField] TMP_Text messageText;

    [SerializeField] Button startGameButton;


    [Header("Turn Shower")]

    [SerializeField] Transform turnShower;

    [SerializeField] Transform user1ShowTurnPos;
    [SerializeField] Transform user2ShowTurnPos;


    [Header("Turn Priority")]

    [SerializeField] Transform turnPriorityIcon;

    [SerializeField] Transform user1TurnPriorityIconPos;
    [SerializeField] Transform user2TurnPriorityIconPos;


    [Header("Player list")]

    [SerializeField] GameObject textPrefab;
    [SerializeField] Transform playerListParent;

    [SerializeField] Color32[] textPrefabColours;


    [Space]
    [Header("Assignables")]

    [SerializeField] GameController gameController;
    [SerializeField] NetworkedTurnManager networkedTurnManager;
    [SerializeField] GameData gameData;

    private void Awake() {
        instance = this;
    }

    private void Start() {
        Invoke(nameof(UpdateList), 0.2f);

        if(!PhotonNetwork.IsMasterClient)
            startGameButton.gameObject.SetActive(false);
    }

    private void OnEnable() {
        GameData.instance.OnCardSelected.AddListener(cardData.SetUpCardData);
    }

    private void OnDisable() {
        GameData.instance.OnCardSelected.RemoveListener(cardData.SetUpCardData);
    }

    #region Button Funcs

    public void StartGame() {
        if (!PhotonNetwork.IsMasterClient)
            return;

        networkedTurnManager.BeginTurn();
        startGameButton.gameObject.SetActive(false);
    }

    public void EndTurn() {
        gameController.SendMoveToMasterClient(new PlayerMove(-1, -1, MoveType.Move), true);
    }

    #endregion

    #region UI Funcs

    public void UpdateRoundText() {
        roundText.text = "Round " + (int)PhotonNetwork.CurrentRoom.CustomProperties[RoomProps.TurnPropKey];
    }

    public void UpdateList() {
        foreach (Transform item in playerListParent) {
            Destroy(item.gameObject);
        }

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++) {
            AddPlayerTextToPlayerListParent(PhotonNetwork.PlayerList[i].NickName, i);
        }
    }

    void AddPlayerTextToPlayerListParent(string playerName, int index) {
        GameObject g = Instantiate(textPrefab, playerListParent);
        g.GetComponentInChildren<Image>().color = textPrefabColours[index];
        g.GetComponentInChildren<TMP_Text>().text = playerName;
    }

    Coroutine msgCor;
    
    public void SetMessageError(string message) {
        AudioManager.instance.Play(SoundNames.error);
        SetMessage("<#A44343>" + message);
    }

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

    #endregion

    #region PUN Callbacks

    public override void OnMasterClientSwitched(Player newMasterClient) {
        base.OnMasterClientSwitched(newMasterClient);

        if(PhotonNetwork.IsMasterClient)
            startGameButton.gameObject.SetActive(true);
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);

        if (propertiesThatChanged.ContainsKey(RoomProps.TurnPropKey)) {
            UpdateRoundText();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) {
        base.OnPlayerEnteredRoom(newPlayer);

        UpdateList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) {
        base.OnPlayerLeftRoom(otherPlayer);

        UpdateList();
    }

    #endregion
}

//[CustomEditor(typeof(GameControllerUI))]
//public class GameControllerUIEditor :Editor {
//    public override void OnInspectorGUI() {
//        base.OnInspectorGUI();

//        if (GUILayout.Button("Set Card Placers")) {
//            SetCardPlacersID();
//        }
//    }

//    private void SetCardPlacersID() {
//        CardPlacer.lastId = 0;

//        CardPlacer[] cardPlacers = FindObjectsOfType<CardPlacer>();
//        foreach (var item in cardPlacers) {
//            item.id = CardPlacer.lastId++;

//            EditorUtility.SetDirty(item);
//            EditorSceneManager.MarkSceneDirty(item.gameObject.scene);
//        }
//    }
//}