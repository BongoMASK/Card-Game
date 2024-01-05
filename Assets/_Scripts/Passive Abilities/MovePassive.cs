using UnityEngine;

public class MovePassive : AppliedPassive
{
    private void OnEnable() {
        MovePassive ap = GetComponent<MovePassive>();

        if (ap != this)
            Destroy(this);
    }

    public virtual void Move(BaseCard card, CardPlacer target, bool canMoveCard) {
        Debug.Log(card.cardStats.cardName + " trying to move to " + target.name);
    }

    public virtual bool ValidateMove(BaseCard card, CardPlacer target) {
        Debug.Log(card.cardStats.cardName + " wants to move to " + target.name);

        bool canCardMove = true;

        if (card.cardOwner.mana < card.cardStats.moveCost) {
            GameControllerUI.instance.SetMessageError("Not enough Mana");
            canCardMove = false;
        }

        if (card.hasBeenMoved) {
            GameControllerUI.instance.SetMessageError("Card has already been moved");
            canCardMove = false;
        }

        if (CardFunctions.instance.CheckDifference(card.currentCardPos.pos, target.pos) > 1) {
            GameControllerUI.instance.SetMessageError("Card not in range");
            canCardMove = false;
        }

        if (target.currentCard != null) {
            GameControllerUI.instance.SetMessageError("Cannot move there");
            canCardMove = false;
        }

        if (target.owner != card.cardOwner) {
            GameControllerUI.instance.SetMessageError("Cannot move there");
            canCardMove = false;
        }

        if (canCardMove)
            card.cardOwner.UsedMana(card.cardStats.moveCost);

        //MoveCard(card, target, canCardMove);

        return canCardMove;
    }
}
