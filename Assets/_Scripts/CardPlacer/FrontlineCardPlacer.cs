using System.Collections.Generic;
using UnityEngine;

public class FrontlineCardPlacer : CardPlacer
{
    private void Start() {
        Invoke(nameof(SetAttackPlacers), 0.5f);
    }

    void SetAttackPlacers() {
        List<CardPlacer> cardPlacerList;

        if (owner == GameManager.instance.user2)
            cardPlacerList = CardValidator.instance.user1CardPlacers;
        else
            cardPlacerList = CardValidator.instance.user2CardPlacers;

        foreach (CardPlacer cp in cardPlacerList) {

            // If its a Shield Placer
            if (cp is ShieldCardPlacer) {
                if (cp.cardPosition == CardPosition.Mid || cp.cardPosition == cardPosition)
                    attackPlacers.Add(cp);
                continue;
            }

            // If its a Frontline or Backline
            if (cp is not FrontlineCardPlacer && cp is not BackLineCardPlacer)
                continue;

            if (CheckDifference(cp, this) <= 1) { 
                attackPlacers.Add(cp);
            }
        }
    }
}
