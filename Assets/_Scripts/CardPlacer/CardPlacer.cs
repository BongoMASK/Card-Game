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

    public User owner;

    [SerializeField] SpriteRenderer border;

    static int lastId = 0;

    public int id = 0;

    private void Start() {
        id = lastId++;
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
