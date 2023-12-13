using UnityEngine;

public class Move : MonoBehaviour
{
    public BaseCard card1;
    public BaseCard card2;

    public CardPlacer fromCardPlacer;
    public CardPlacer toCardPlacer;

    public void RegisterMove(CardPlacer from, CardPlacer to, BaseCard card1, BaseCard card2 = null) {
        fromCardPlacer = from;
        toCardPlacer = to;
        this.card1 = card1;
        this.card2 = card2;
    }

    public void UndoMove() {
        // Figure out how to change all values and stuff after undo
    }
}
