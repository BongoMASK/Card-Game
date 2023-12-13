using System.Collections.Generic;
using UnityEngine;

public class CardValidator : MonoBehaviour
{
    public static CardValidator instance;

    [Header("Cards and Card Placers")]

    public List<CardPlacer> allCardPlacers;
    public List<CardPlacer> user1CardPlacers = new List<CardPlacer>();
    public List<CardPlacer> user2CardPlacers = new List<CardPlacer>();

    public List<BaseCard> allCards = new List<BaseCard>();
    public List<BaseCard> user1Cards = new List<BaseCard>();
    public List<BaseCard> user2Cards = new List<BaseCard>();

    [Header("Assignables")]

    public AttackSystem attackSystem;

    private void Awake() {
        instance = this;
        
        allCardPlacers = new List<CardPlacer>(FindObjectsOfType<CardPlacer>());
    }

    private void Start() {
        foreach (CardPlacer c in allCardPlacers) {
            if (c.owner == GameManager.instance.user1) {
                user1CardPlacers.Add(c);
            }
            else {
                user2CardPlacers.Add(c);
            }
        }
    }

    public void CheckForAllCardBuffs() {
        foreach (BaseCard card in allCards) {
            card.RemoveAllBuffs();
            card.CheckForBuffs();
        }
    }
}
