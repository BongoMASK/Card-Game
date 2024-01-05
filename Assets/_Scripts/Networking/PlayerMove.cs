using UnityEngine;

public enum MoveType {
    Move,
    Attack,
    Swap,
    Finish
}

public class PlayerMove
{
    public int cardID;
    public int cardPlacerID;
    public MoveType moveType;

    public PlayerMove(int cardID, int cardPlacerID, MoveType moveType) {
        this.cardID = cardID;
        this.cardPlacerID = cardPlacerID;
        this.moveType = moveType;
    }

    public object[] ToByteArray() {
        return new object[] { cardID, cardPlacerID, moveType };
    }

    public static PlayerMove ToPlayerMove(object[] objs) {
        return new PlayerMove((int)objs[0], (int)objs[1], (MoveType)objs[2]);
    }

    public void Print() {
        string msg = "CardID: " + cardID + ", " + "CardPlacerID: " + cardPlacerID + ", " + "Movetype: " + moveType;
        Debug.Log(msg);
    }
}
