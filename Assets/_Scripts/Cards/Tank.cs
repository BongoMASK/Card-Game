using UnityEngine;

public class Tank : BaseCard
{

    public override void ApplyPassive(CardPlacer target) {
        // Check if card can apply passive
        if (currentCardPos.pos == Vector2.zero || target.owner != currentCardPos.owner)
            return;

        if (target == currentCardPos)
            return;

        if (IsWithinRange(target)) {
            base.ApplyPassive(target);

            target.currentCard.ApplyHealthBuff(1);
        }
    }

    bool IsWithinRange(CardPlacer target) {
        return CheckDifference(currentCardPos.pos, target.pos) <= passiveRange;
    }
}
