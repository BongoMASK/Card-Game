using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Healer : BaseCard
{
    public override void ApplyPassive(CardPlacer target) {
        // Check if card can apply passive
        if (currentCardPos.pos == Vector2.zero || target.owner != currentCardPos.owner)
            return;

        if (target == currentCardPos)
            return;

        if (IsWithinRange(target)) {
            base.ApplyPassive(target);

            //target.currentCard.ApplyHealthBuff(1);
            if (!target.currentCard.GetComponent<HealthBuff>())
                target.currentCard.AddComponent<HealthBuff>();

            Debug.Log(this.cardStats.cardName + " applying health buff to " + target.currentCard.cardStats.cardName);
        }
    }

    bool IsWithinRange(CardPlacer target) {
        return CheckDifference(currentCardPos.pos, target.pos) <= passiveRange;
    }
}
