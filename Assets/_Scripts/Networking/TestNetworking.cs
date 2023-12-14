using Photon.Realtime;
using UnityEngine;

public class TestNetworking : MonoBehaviour, INetworkedTurnManagerCallbacks
{

    [SerializeField] NetworkedTurnManager networkedTurnManager;

    private void Start() {
        networkedTurnManager.TurnManagerListener = this;
    }

    public void OnPlayerFinished(Player player, int turn, object[] move) {
        throw new System.NotImplementedException();
    }

    public void OnPlayerMove(Player player, int turn, object[] move) {
        Move2 m = new Move2();
        m.ToMove2(move);
        m.Print();
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

        if (Input.GetKeyDown(KeyCode.Space)) {
            Move2 move = new Move2(3, 5);
            object[] o = move.ToByteArray();
            networkedTurnManager.SendMove(o, false);
        }
    }
}

public class Move2 {
    int id = 0;
    int lol = 4;

    public Move2() { }

    public Move2(int id, int lol) {
        this.id = id;
        this.lol = lol;
    }

    public void Print() {
        Debug.Log(id + " " + lol);
    }

    public object[] ToByteArray() {
        return new object[] { id, lol };
    }

    public void ToMove2(object[] objects) {
        id = (int)objects[0];  
        lol = (int)objects[1];
    }
}
