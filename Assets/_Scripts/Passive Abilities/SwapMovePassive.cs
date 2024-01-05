using UnityEngine;

public class SwapMovePassive : MovePassive
{
    public override bool ValidateMove(BaseCard card, CardPlacer target) {
        bool canCardMove = true;

        if (card.cardOwner.mana < card.cardStats.moveCost) {
            GameControllerUI.instance.SetMessageError("Not enough Mana");
            canCardMove = false;
        }

        if (card.hasBeenMoved) {
            GameControllerUI.instance.SetMessageError("Card has already been moved");
            canCardMove = false;
        }

        if (!card.IsWithinRange(target)) {
            GameControllerUI.instance.SetMessageError("Card not in range");
            canCardMove = false;
        }

        if (target.owner != card.cardOwner) {
            GameControllerUI.instance.SetMessageError("Cannot move there");
            canCardMove = false;
        }

        if (canCardMove)
            card.cardOwner.UsedMana(card.cardStats.moveCost);

        if (target.currentCard == null) {
            //MoveCard(card, target, canCardMove);
            card.MoveCard(target, canCardMove);
            return true;
        }

        // Swap cards
        return canCardMove;
    }

    public override void Move(BaseCard card, CardPlacer target, bool canMoveCard) {
        if (target.currentCard != null)
            SwapCards(card, target.currentCard, canMoveCard);

        else
            base.Move(card, target, canMoveCard);
    }

    public void SwapCards(BaseCard card, BaseCard otherCard, bool canMoveCard) {
        // Place cards in the new positions if everything is good
        if (canMoveCard) {

            CardPlacer cp1 = card.currentCardPos;
            CardPlacer cp2 = otherCard.currentCardPos;

            // Card movement requires these specific set of steps in order.
            // All of the movement relies on these sets of steps

            // Remove card from card placer
            cp1.OnCardRemoved(card);
            cp2.OnCardRemoved(otherCard);

            // set new card position. This also starts the animation of the card moving towards the card pos
            card.currentCardPos = cp2;
            otherCard.currentCardPos = cp1;

            // set card placer value for current card
            cp1.currentCard = otherCard;
            cp2.currentCard = card;

            // Call Function that places the card on the thing
            cp1.OnCardPlaced(otherCard);
            cp2.OnCardPlaced(card);

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
}
