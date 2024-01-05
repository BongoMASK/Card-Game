using UnityEngine;

public class ShieldCardPlacer : CardPlacer
{
    [SerializeField] private BaseCard shieldCard;

    private void Start() {
        //BaseCard c = Instantiate(shieldCard);
        //currentCard = c;
        //c.currentCardPos = this;
        //c.cardOwner = owner;
        //c.hasBeenMoved = true;

        //SpriteRenderer rend = currentCard.transform.GetChild(1).GetComponent<SpriteRenderer>();
        //rend.material.SetFloat("_Thickness", 0.02f);
    }

    void SetCannotMove() {
        if (currentCard != null) {
            currentCard.hasBeenMoved = true;
        }
    }

    public override void OnCardPlaced(BaseCard card) {
        base.OnCardPlaced(card);

        currentCard.cardStats = currentCard.cardStats.upgradedCardStats;

        currentCard.cardOwner = owner;
        currentCard.hasBeenMoved = true;

        SpriteRenderer rend = currentCard.transform.GetChild(1).GetComponent<SpriteRenderer>();
        rend.material.SetFloat("_Thickness", 0.02f);
    }
}