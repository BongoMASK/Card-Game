using System.Collections.Generic;
using UnityEngine;

public class BackLineCardPlacer : CardPlacer
{
    private void Start() {
        //Invoke(nameof(SetAttackPlacers), 0.5f);
    }

    void SetAttackPlacers() {
        List<CardPlacer> cardPlacerList;

        if (owner == GameManager.instance.user2)
            cardPlacerList = CardValidator.instance.user1CardPlacers;
        else
            cardPlacerList = CardValidator.instance.user2CardPlacers;

        foreach (CardPlacer cp in cardPlacerList) {
            if (cp is not FrontlineCardPlacer)
                continue;

            if (CheckDifference(cp, this) <= 1) {
                attackPlacers.Add(cp);
            }
        }
    }
}
