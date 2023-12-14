using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;

public class TestNetworking1 : MonoBehaviour, IPunTurnManagerCallbacks
{

    [SerializeField] PunTurnManager punTurnManager;

    private void Start() {
        punTurnManager.TurnManagerListener = this;
    }

    public void OnPlayerFinished(Player player, int turn, object move) {
        throw new System.NotImplementedException();
    }

    public void OnPlayerMove(Player player, int turn, object move) {
        Debug.Log(move);
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

    private void Update() {
        if (Input.GetKeyDown(KeyCode.A)) {
            object o = 3;
            punTurnManager.SendMove(o, false);
        }
    }
}