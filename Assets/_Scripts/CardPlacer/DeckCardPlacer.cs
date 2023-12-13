using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Photon.Pun;

public class DeckCardPlacer : CardPlacer
{
    [SerializeField] List<BaseCard> cardPrefabs = new List<BaseCard>();
    [SerializeField] List<string> cardPrefabsPath = new List<string>();
    [SerializeField] List<HandCardPlacer> handCardPlacers = new List<HandCardPlacer>();

    private void Start() {
        OnCardRemoved(null);
        GameManager.instance.OnTurnBegin += DrawCards;
        DrawCards();
    }

    public override void OnCardRemoved(BaseCard card) {
        base.OnCardRemoved(card);

        int rand = Random.Range(0, cardPrefabs.Count);

        //BaseCard c = Instantiate(cardPrefabs[rand]);
        GameObject g = PhotonNetwork.Instantiate(cardPrefabsPath[rand], Vector3.one * 100, Quaternion.identity);
        BaseCard c = g.GetComponent<BaseCard>();

        currentCard = c;
        c.currentCardPos = this;
        c.cardOwner = owner;

        //owner.hasPlacedCard = true;
    }

    void DrawCards() {
        StartCoroutine(CardDrawAnim());
    }

    IEnumerator CardDrawAnim() {
        foreach (HandCardPlacer hand in handCardPlacers) {
            if (hand.currentCard != null)
                continue;

            currentCard.MoveCard(hand, true, false);

            AudioManager.instance.Play(SoundNames.cardPress);

            yield return new WaitForSeconds(0.5f);
        }

        currentCard.HasBeenMovedOverride();
    }
}
