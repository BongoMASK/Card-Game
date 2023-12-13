using UnityEngine;

public enum CardType {
    Normal,
    Assassin,
    Mage,
    Tank,
    Beserker,
    Healer,
    Warrior,
}

[CreateAssetMenu(fileName ="New Card", menuName = "ScriptableObjects/New Card")]
public class CardStats : ScriptableObject
{
    public string cardName;

    public CardStats upgradedCardStats;
    public CardStats normalCardStats;

    [Header("Stats")]

    /// <summary>
    /// Card class
    /// </summary>
    public CardType cardType;

    /// <summary>
    /// Maximum health the card can have
    /// </summary>
    public int maxHP = 1;

    /// <summary>
    /// Mana given to Mana pool when giving it to the Mana Zone
    /// </summary>
    public int sacrificeCost = 1;

    /// <summary>
    /// Amount of damage that will dealt
    /// </summary>
    public int damage = 1; 

    /// <summary>
    /// Mana taken to make attack
    /// </summary>
    public int manaCost = 1;

    /// <summary>
    /// Mana taken to move
    /// </summary>
    public int moveCost = 1;

    [Header("UI")]

    public Sprite sprite;

    public string passiveName;

    /// <summary>
    /// Info about the passive ability
    /// </summary>
    [TextArea]
    public string passiveDescription = "";
}
