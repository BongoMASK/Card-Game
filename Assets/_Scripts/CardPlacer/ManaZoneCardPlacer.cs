using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaZoneCardPlacer : CardPlacer {
    
    public override void OnCardPlaced(BaseCard card) {
        base.OnCardPlaced(card);

        // Get Mana from card
        owner.currentMaxRoundMana += card.cardStats.sacrificeCost;
    }

    public override void OnCardRemoved(BaseCard card) {
        base.OnCardRemoved(card);

        // Remove mana from player
        owner.currentMaxRoundMana -= card.cardStats.sacrificeCost;
    }
}