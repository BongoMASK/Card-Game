using UnityEngine;

public class MovePassive : AppliedPassive
{
    private void OnEnable() {
        MovePassive ap = GetComponent<MovePassive>();

        if (ap != this)
            Destroy(this);
    }

    public virtual void Move(BaseCard card, CardPlacer target, bool canMoveCard) {
        // Place card in the new position if everything is good
        if (canMoveCard) {

            // Remove card from card placer
            card.currentCardPos.OnCardRemoved(card);

            // set new card position. This also starts the animation of the card moving towards the card pos
            card.currentCardPos = target;

            // set card placer value for current card
            target.currentCard = card;

            // Call Function that places the card on the thing
            target.OnCardPlaced(card);

            // Recheck for all buffs on all cards
            CardFunctions.instance.CheckForAllCardBuffs();

            // set to true to restrict card from moving twice
            card.hasBeenMoved = true;
        }

        // Place card back to original position if there is a problem
        else {
            card.MoveTo(Vector3.zero);
        }
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
