using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Deck
{
    const int deckCardVars = 2;

    public List<DeckCard> cards = new List<DeckCard>(); 
    public int count => cards.Count;

    public bool isEmpty => IsDeckFinished();

    private bool IsDeckFinished() {
        foreach (var item in cards) {
            if (item.cardCount > 0)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets a card from the deck at random
    /// Check if the deck is empty before calling this
    /// </summary>
    /// <returns></returns>
    public CardType GetCardFromDeck() {
        if (isEmpty)
            return CardType.None;

        int rand = Random.Range(0, count);
        if (cards[rand].cardCount == 0)
            return this.GetCardFromDeck();

        cards[rand].cardCount--;
        return cards[rand].cardType;
    }

    public object[] ToByteArray() {
        object[] array = new object[count * deckCardVars];
        
        for (int i = 0; i < count; i++) {
            array[deckCardVars * i] = cards[i].cardType;
            array[deckCardVars * i + 1] = cards[i].cardCount;
        }

        return array;
    }

    public static Deck ToDeck(object[] objs) {
        Deck deck = new Deck();

        Debug.Log(objs.Length);

        for (int i = 0; i < objs.Length / 2; i++) {
            DeckCard deckCard = new DeckCard();
            deckCard.cardType = (CardType)objs[deckCardVars * i];
            deckCard.cardCount = (int)objs[deckCardVars * i + 1];

            deck.cards.Add(deckCard);
        }

        return deck;
    }

    public void Print() {
        foreach (var item in cards) {
            Debug.Log(item.cardType + ", " + item.cardCount);
        }
    }
}

[System.Serializable]
public class DeckCard {
    public CardType cardType = CardType.Tank;
    public int cardCount = 0;
}
