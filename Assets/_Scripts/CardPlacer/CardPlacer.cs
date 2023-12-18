using System.Collections.Generic;
using UnityEngine;

public class CardPlacer : MonoBehaviour
{
    // Stores the co-ordinate position of the card placer
    [SerializeField] protected Vector2 _pos = Vector2.zero;
    public Vector2 pos { get => _pos; }

    public List<CardPlacer> movePlacers;
    public List<CardPlacer> attackPlacers;
    public List<CardPlacer> blockPlacers;

    public CardPosition cardPosition = CardPosition.Left;

    public BaseCard currentCard;

    public PlayerData owner;

    [SerializeField] SpriteRenderer border;

    public static int lastId = 0;

    public int id;

    public static CardPlacer FindCardPlacer(int id) {
        if(id < 0)
            return null;

        foreach (var item in FindObjectsOfType<CardPlacer>()) {
            if (item.id == id) return item;
        }

        Debug.LogError("Card Placer with id: " + id + " does not exist or has been destroyed");
        return null;
    }

    public virtual void OnCardPlaced(BaseCard card) {

    }

    public virtual void OnCardRemoved(BaseCard card) {
        currentCard = null;
    }

    protected int CheckDifference(CardPlacer cp1, CardPlacer cp2) {
        return CheckDifference(cp1.pos, cp2.pos);
    }

    protected int CheckDifference(Vector2 v1, Vector2 v2) {
        Vector2 v = v2 - v1;
        return (int)v.magnitude;
    }

    public void OnSelected(Color32 color) {
        border.enabled = true;
        border.color = color;
    }

    public void OnDeselected() {
        border.enabled = false;
    }
}
