using UnityEngine;

public class ManaCardPlacer : CardPlacer
{
    public override void OnCardPlaced(BaseCard card) {
        base.OnCardPlaced(card);

        // Make card die
        owner.currentMaxRoundMana += card.cardStats.sacrificeCost;
        currentCard.Die();
    }
}
