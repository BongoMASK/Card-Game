using System.Collections.Generic;
using UnityEngine;

public class Mage : BaseCard
{
    bool IsWithinRange(CardPlacer target) {
        return CheckDifference(currentCardPos.pos, target.pos) <= passiveRange;
    }

    protected override void ShowMovementPowers() {
        List<CardPlacer> cardPlacerList = GameData.instance.allCardPlacers;

        foreach (CardPlacer cp in cardPlacerList) {
            if (cp.owner != cardOwner)
                continue;

            if (cp == currentCardPos)
                continue;

            if (CheckDifference(currentCardPos.pos, cp.pos) <= 1) {
                cp.OnSelected(Color.white);
            }
        }
    }
}
