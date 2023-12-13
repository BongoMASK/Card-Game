using System.Collections;
using UnityEngine;

public class HandCardPlacer : CardPlacer
{
    [SerializeField] DeckCardPlacer deck;
    
    public override void OnCardPlaced(BaseCard card) {
        base.OnCardPlaced(card);

        CardValidator.instance.allCards.Add(currentCard);
        if (owner == GameManager.instance.user1)
            CardValidator.instance.user1Cards.Add(currentCard);
        else
            CardValidator.instance.user2Cards.Add(currentCard);
    }

    public override void OnCardRemoved(BaseCard card) {
        base.OnCardRemoved(card);

        // Summoning Sickness 
        card.hasAttacked = true;
        card.hasBeenMoved = true;
    }

    void GetCard() {
        if (currentCard != null)
            return;

        StartCoroutine(GetCardFromDeck());
    }

    IEnumerator GetCardFromDeck() {
        char c = name[name.Length - 2];
        int i = int.Parse(c.ToString());

        Debug.Log(i);

        yield return new WaitForSeconds((float)i / 2);

        deck.currentCard.MoveCard(this, true);
    }
}
