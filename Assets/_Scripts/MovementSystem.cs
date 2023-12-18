using UnityEngine;

public class MovementSystem : MonoBehaviour {

    public bool ValidateMovement(CardPlacer original, CardPlacer target, BaseCard card) {  

        bool canMoveCard = original.movePlacers.Contains(target);

        if (target.currentCard != null)
            canMoveCard = false;

        // Check if it is a mana card placer.
        // User can only sacrifice card to mana placer once per round
        var placer = target as ManaCardPlacer;

        if (placer != null) {
            if (card.cardOwner.hasGivenCardToManaZone) {
                canMoveCard = false;
            }
        }

        // Place card in the new position if everything is good
        if (canMoveCard) {
            original.OnCardRemoved(card);

            card.currentCardPos = target;

            target.currentCard = card;
            target.OnCardPlaced(card);
        }

        // Place card back to original position if there is a problem
        else {
            card.transform.position = original.transform.position;
        }

        return canMoveCard;
    }
}