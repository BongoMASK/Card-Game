using UnityEngine;

public class HandCardPlacer : CardPlacer
{
    [SerializeField] DeckCardPlacer deck;
    
    public override void OnCardPlaced(BaseCard card) {
        base.OnCardPlaced(card);
    }

    public override void OnCardRemoved(BaseCard card) {
        base.OnCardRemoved(card);

        // Summoning Sickness 
        card.hasAttacked = true;
        card.hasBeenMoved = true;
    }
}