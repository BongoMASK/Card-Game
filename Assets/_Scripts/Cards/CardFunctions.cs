using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class CardFunctions : MonoBehaviour
{

    public static CardFunctions instance;

    [SerializeField] NetworkedTurnManager networkedTurnManager;

    [SerializeField] List<BaseCard> cardPrefabs = new List<BaseCard>();

    private void Awake() {
        instance = this;
    }

    #region Attack Functions

    public bool ValidateAttack(BaseCard attacker, CardPlacer target, Player attackingPlayer) {
        if (attacker == null || target == null)
            return false;

        if (attacker.cardOwner.lockInput)
            return false;

        // will work after setting up attack placers
        bool b = ValidateAttack(attacker.currentCardPos, target);

        if (b) {
            attacker.Attack(target.currentCard);
        }

        return b;
    }

    public bool ValidateAttack(BaseCard attacker, BaseCard target) {

        if (attacker == null || target == null)
            return false;

        if (attacker.cardOwner.lockInput)
            return false;

        // will work after setting up attack placers
        bool b = ValidateAttack(attacker.currentCardPos, target.currentCardPos);

        return b;
    }

    private bool ValidateAttack(CardPlacer attacker, CardPlacer target) {
        bool b = attacker.attackPlacers.Contains(target);
        return b && CanAttack(attacker.currentCard, target.currentCard);
    }

    public void Attack(BaseCard attacker, CardPlacer target) {
        Attack(attacker, target.currentCard);
    }

    public void Attack(BaseCard attacker, BaseCard target) {
        Debug.Log(name + " trying to attack " + target.name);

        if (attacker.CanAttack(target)) {

            //cardOwner.UsedMana(cardStats.manaCost);

            bool b = CheckForBlockers(target, attacker);

            if (!b) {
                GameControllerUI.instance.SetMessage(attacker.cardStats.cardName + " attacked " + target.cardStats.cardName);
                target.TakeDamage(attacker.effectiveDamage);
            }

            //hasAttacked = true;

            AudioManager.instance.Play(SoundNames.attack);
        }
    }

    private bool CanAttack(BaseCard attacker, BaseCard other) {
        if (attacker.cardOwner.mana < attacker.cardStats.manaCost) {
            GameControllerUI.instance.SetMessageError("Not enough Mana");
            return false;
        }

        if (attacker.hasAttacked) {
            GameControllerUI.instance.SetMessageError("Card has already attacked");
            return false;
        }

        if (!attacker.IsWithinAttackRange()) {
            GameControllerUI.instance.SetMessageError("Card not in range");
            return false;
        }

        bool b = attacker.effectiveDamage >= other.effectiveHealth;
        if (!b)
            GameControllerUI.instance.SetMessageError("Not enough damage power");

        return b;
    }

    /// <summary>
    /// This code runs on the card that is being attacked.
    /// It searches for blockers that can protect the card
    /// </summary>
    /// <param name="attackerCard"></param>
    /// <returns></returns>
    public bool CheckForBlockers(BaseCard target, BaseCard attackerCard) {
        List<BaseCard> cardList = GameData.instance.activeCards;

        foreach (BaseCard blockerCard in cardList) {
            if (blockerCard.cardOwner != target.cardOwner)
                continue;

            if (blockerCard == this)
                continue;

            if (blockerCard is Tank) {
                if (blockerCard.currentCardPos.attackPlacers.Contains(attackerCard.currentCardPos)) {

                    Debug.Log(blockerCard.cardStats.cardName + blockerCard.cardOwner.username + blockerCard.currentCardPos);

                    target.GetComponent<Animator>()?.SetTrigger("shine");

                    if (attackerCard.CanAttack(blockerCard)) {
                        GameControllerUI.instance.SetMessage(attackerCard.cardStats.cardName + "'s attack to " + target.cardStats.cardName
                            + " was blocked by " + blockerCard.cardStats.cardName);

                        blockerCard.TakeDamage(attackerCard.effectiveDamage);
                    }

                    return true;
                }
            }
        }

        return false;
    }

    #endregion

    #region Move Functions

    protected int CheckDifference(Vector2 v1, Vector2 v2) {
        Vector2 v = v2 - v1;
        return (int)v.magnitude;
    }

    public bool ValidateMovement(BaseCard movingCard, CardPlacer target, Player sender) {
        // Check if sender is owner of card
        if(movingCard.cardOwner.player != sender) 
            return false;

        // Check if it is in player hand
        if (movingCard.currentCardPos.pos != Vector2.zero && target.pos != Vector2.zero) {
            if (movingCard.cardStats.cardType == CardType.Mage)
                return MageBattleFieldMovementSystem(movingCard, target);

            return BattleFieldMovementSystem(movingCard, target);
        }

        return NonBattleFieldMovementSystem(movingCard, target);
    }

    private bool BattleFieldMovementSystem(BaseCard card, CardPlacer target) {
        bool canCardMove = true;

        if (card.cardOwner.mana < card.cardStats.moveCost) {
            GameControllerUI.instance.SetMessageError("Not enough Mana");
            canCardMove = false;
        }

        if (card.hasBeenMoved) {
            GameControllerUI.instance.SetMessageError("Card has already been moved");
            canCardMove = false;
        }

        if (CheckDifference(card.currentCardPos.pos, target.pos) > 1) {
            GameControllerUI.instance.SetMessageError("Card not in range");
            canCardMove = false;
        }

        if (target.currentCard != null) {
            GameControllerUI.instance.SetMessageError("Cannot move there");
            canCardMove = false;
        }

        if (target.owner != card.cardOwner) {
            GameControllerUI.instance.SetMessageError("Cannot move there");
            canCardMove = false;
        }

        if (canCardMove)
            card.cardOwner.UsedMana(card.cardStats.moveCost);

        //MoveCard(card, target, canCardMove);

        return canCardMove;
    }

    private bool MageBattleFieldMovementSystem(BaseCard card, CardPlacer target) {
        bool canCardMove = true;

        if (card.cardOwner.mana < card.cardStats.moveCost) {
            GameControllerUI.instance.SetMessageError("Not enough Mana");
            canCardMove = false;
        }

        if (card.hasBeenMoved) {
            GameControllerUI.instance.SetMessageError("Card has already been moved");
            canCardMove = false;
        }

        if (!IsWithinRange(card.currentCardPos, target, card.passiveRange)) {
            GameControllerUI.instance.SetMessageError("Card not in range");
            canCardMove = false;
        }

        if (target.owner != card.cardOwner) {
            GameControllerUI.instance.SetMessageError("Cannot move there");
            canCardMove = false;
        }

        if (canCardMove)
            card.cardOwner.UsedMana(card.cardStats.moveCost);

        if (target.currentCard == null) {
            MoveCard(card, target, canCardMove);
            return true;
        }

        // Swap cards
        return canCardMove;
    }

    private bool NonBattleFieldMovementSystem(BaseCard card, CardPlacer target) {
        Debug.Log(card.cardOwner.hasGivenCardToManaZone);

        // Cannot move card to enemy zone
        if (card.cardOwner != target.owner) {
            GameControllerUI.instance.SetMessageError("Cannot move card to enemy's area");
            return false;
        }

        // Do this if it is in Player hand
        bool canMoveCard = card.currentCardPos.movePlacers.Contains(target);

        if (target.currentCard != null)
            canMoveCard = false;

        // Check if it is a mana card placer.
        // User can only sacrifice card to mana placer once per round
        if (target as ManaCardPlacer != null) {
            if (card.cardOwner.hasGivenCardToManaZone) {
                canMoveCard = false;

                GameControllerUI.instance.SetMessageError("Can only move 1 card to mana zone per turn");
            }
            else
                card.cardOwner.hasGivenCardToManaZone = true;
        }

        else if (target as BackLineCardPlacer || target as FrontlineCardPlacer) {
            if (card.cardOwner.hasPlacedCard) {
                canMoveCard = false;

                GameControllerUI.instance.SetMessageError("Can only move 1 card to battle field once per turn");
            }
            else
                card.cardOwner.hasPlacedCard = true;
        }
        
        return canMoveCard;
    }

    public void MoveCard(BaseCard card, CardPlacer target, bool canMoveCard, bool moved = true) {
        // Place card in the new position if everything is good
        if (canMoveCard) {

            // Remove card from card placer
            card.currentCardPos.OnCardRemoved(card);

            // set new card position. This also starts the animation of the card moving towards the card pos
            card.currentCardPos = target;

            // set card placer value for current card
            target.currentCard = card;

            // Call Function that places the card on the thing
            target.OnCardPlaced(card);

            // Recheck for all buffs on all cards
            CheckForAllCardBuffs();

            // set to true to restrict card from moving twice
            card.hasBeenMoved = moved;
        }

        // Place card back to original position if there is a problem
        else {
            card.MoveTo(Vector3.zero);
        }
    }

    public void SwapCards(BaseCard card, CardPlacer cardPlacer, bool canMoveCard) {
        if (cardPlacer.currentCard != null)
            SwapCards(card, cardPlacer.currentCard, canMoveCard);

        else
            MoveCard(card, cardPlacer, canMoveCard);
    }

    public void SwapCards(BaseCard card, BaseCard otherCard, bool canMoveCard) { 
        // Place cards in the new positions if everything is good
        if (canMoveCard) {

            CardPlacer cp1 = card.currentCardPos;
            CardPlacer cp2 = otherCard.currentCardPos;

            // Card movement requires these specific set of steps in order.
            // All of the movement relies on these sets of steps

            // Remove card from card placer
            cp1.OnCardRemoved(card);
            cp2.OnCardRemoved(otherCard);

            // set new card position. This also starts the animation of the card moving towards the card pos
            card.currentCardPos = cp2;
            otherCard.currentCardPos = cp1;

            // set card placer value for current card
            cp1.currentCard = otherCard;
            cp2.currentCard = card;

            // Call Function that places the card on the thing
            cp1.OnCardPlaced(otherCard);
            cp2.OnCardPlaced(card);

            // Recheck for all buffs on all cards
            CheckForAllCardBuffs();

            // set to true to restrict card from moving twice
            card.hasBeenMoved = true;
        }

        // Place card back to original position if there is a problem
        else {
            card.MoveTo(Vector3.zero);
        }
    }

    bool IsWithinRange(CardPlacer a, CardPlacer b, int range) {
        return CheckDifference(a.pos, b.pos) <= range;
    }

    private void SendMoveToAll(BaseCard card, CardPlacer cardPlacer, MoveType moveType, Player sender) {
        PlayerMove move = new PlayerMove(card.cardID, cardPlacer.id, moveType);
        networkedTurnManager.SendMove(move.ToByteArray(), false, sender);
    }

    #endregion

    #region Card Creation

    public void CreateCard(int id, CardType cardType, int cardPlacerID, Player owner) {
        BaseCard c;

        switch (cardType) {
            case CardType.Tank:
                c = Instantiate(cardPrefabs[0]);
                break;

            case CardType.Beserker:
                c = Instantiate(cardPrefabs[1]);
                break;

            case CardType.Mage:
                c = Instantiate(cardPrefabs[2]);
                break;

            default:
                return;
        }

        c.transform.rotation = Quaternion.Euler(0, 0, Camera.main.transform.rotation.eulerAngles.z);
        CardPlacer cp = CardPlacer.FindCardPlacer(cardPlacerID);

        c.cardID = id;
        c.currentCardPos = cp;
        cp.currentCard = c;
        c.cardOwner = cp.owner;

        BaseCard.id = id + 1;
    }

    #endregion

    #region Buffs / Debuffs

    public void CheckForAllCardBuffs() {
        foreach (BaseCard card in GameData.instance.activeCards) {
            card.RemoveAllBuffs();
            card.CheckForBuffs();
        }
    }

    #endregion
}
