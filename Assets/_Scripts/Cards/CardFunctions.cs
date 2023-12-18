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

        if (b) {
            attacker.Attack(target);
        }

        return b;
    }

    private bool ValidateAttack(CardPlacer attacker, CardPlacer target) {
        bool b = attacker.attackPlacers.Contains(target);

        return b;
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

    /// <summary>
    /// This code runs on the card that is being attacked.
    /// It searches for blockers that can protect the card
    /// </summary>
    /// <param name="attackerCard"></param>
    /// <returns></returns>
    public bool CheckForBlockers(BaseCard target, BaseCard attackerCard) {
        List<BaseCard> cardList;
        if (target.cardOwner.lockInput)
            cardList = CardValidator.instance.user1Cards;
        else
            cardList = CardValidator.instance.user2Cards;

        foreach (BaseCard blockerCard in cardList) {
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

        // Check if it is in player hand
        if (movingCard.currentCardPos.pos != Vector2.zero && target.pos != Vector2.zero) {
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

    bool NonBattleFieldMovementSystem(BaseCard card, CardPlacer target) {
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
        }

        else if (target as BackLineCardPlacer || target as FrontlineCardPlacer) {
            if (card.cardOwner.hasPlacedCard) {
                canMoveCard = false;

                GameControllerUI.instance.SetMessageError("Can only move 1 card to battle field once per turn");
            }

            card.cardOwner.hasPlacedCard = true;
        }

        //MoveCard(card, target, canMoveCard);
        
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
