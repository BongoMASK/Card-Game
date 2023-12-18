using System.Collections.Generic;
using UnityEngine;

public class Mage : BaseCard
{
    public override void BattleFieldMovementSystem(CardPlacer target) {
        bool canCardMove = true;

        if (cardOwner.mana < cardStats.moveCost) {
            GameManager.instance.SetMessageError("Not enough Mana");
            canCardMove = false;
        }

        if (hasBeenMoved) {
            GameManager.instance.SetMessageError("Card has already been moved");
            canCardMove = false;
        }

        if (!IsWithinRange(target)) {
            GameManager.instance.SetMessageError("Card not in range");
            canCardMove = false;
        }

        if (target.owner != cardOwner) {
            GameManager.instance.SetMessageError("Cannot move there");
            canCardMove = false;
        }

        if(canCardMove) 
            cardOwner.UsedMana(cardStats.moveCost);

        if (target.currentCard == null) {
            MoveCard(target, canCardMove);
            return;
        }

        // Swap cards
        SwapCards(target.currentCard, canCardMove);
    }

    void SwapCards(BaseCard otherCard, bool canMoveCard) {
        // Place cards in the new positions if everything is good
        if (canMoveCard) {

            CardPlacer cp1 = currentCardPos;
            CardPlacer cp2 = otherCard.currentCardPos;

            // Card movement requires these specific set of steps in order.
            // All of the movement relies on these sets of steps

            // Remove card from card placer
            cp1.OnCardRemoved(this);
            cp2.OnCardRemoved(otherCard);

            // set new card position. This also starts the animation of the card moving towards the card pos
            currentCardPos = cp2;
            otherCard.currentCardPos = cp1;

            // set card placer value for current card
            cp1.currentCard = otherCard;
            cp2.currentCard = this;

            // Call Function that places the card on the thing
            cp1.OnCardPlaced(otherCard);
            cp2.OnCardPlaced(this);

            // Recheck for all buffs on all cards
            CardValidator.instance.CheckForAllCardBuffs();

            // set to true to restrict card from moving twice
            hasBeenMoved = true;
        }

        // Place card back to original position if there is a problem
        else {
            transform.position = currentCardPos.transform.position;
        }
    }

    bool IsWithinRange(CardPlacer target) {
        return CheckDifference(currentCardPos.pos, target.pos) <= passiveRange;
    }

    protected override void ShowMovementPowers() {
        List<CardPlacer> cardPlacerList;
        if (cardOwner == GameManager.instance.user1)
            cardPlacerList = CardValidator.instance.user1CardPlacers;
        else
            cardPlacerList = CardValidator.instance.user2CardPlacers;


        foreach (CardPlacer cp in cardPlacerList) {
            if (cp == currentCardPos)
                continue;

            if (CheckDifference(currentCardPos.pos, cp.pos) <= 1) {
                cp.OnSelected(Color.white);
            }
        }
    }
}
