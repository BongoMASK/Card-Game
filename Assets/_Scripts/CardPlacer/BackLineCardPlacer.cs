using System.Collections.Generic;
using UnityEngine;

public class BackLineCardPlacer : CardPlacer
{
    private void Start() {
        Invoke(nameof(SetAttackPlacers), 0.5f);
    }

    void SetAttackPlacers() {
        List<CardPlacer> cardPlacerList = GameData.instance.allCardPlacers;

        foreach (CardPlacer cp in cardPlacerList) {
            if (cp.owner == owner)
                continue;

            if (cp is not FrontlineCardPlacer)
                continue;

            if (CheckDifference(cp, this) <= 1) {
                attackPlacers.Add(cp);
            }
        }
    }
}
