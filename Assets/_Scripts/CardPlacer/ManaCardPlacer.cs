using UnityEngine;

public class ManaCardPlacer : CardPlacer
{
    [SerializeField] CardPlacer[] manaZones;

    public bool hasSpace => HasSpace();

    public override void OnCardPlaced(BaseCard card) {
        base.OnCardPlaced(card);

        //// Make card die
        //owner.currentMaxRoundMana += card.cardStats.sacrificeCost;
        //currentCard.Die();

        foreach (var item in manaZones) {
            if (item.currentCard == null)
                card.currentCardPos = item;
        }
    }

    private bool HasSpace() {
        foreach (var item in manaZones) {
            if (item.currentCard == null)
                return true;
        }

        return false;
    }
}
