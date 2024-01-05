using Photon.Realtime;
using UnityEngine;

public class PassiveHandler : MonoBehaviour
{
    public int extraHealth => GetExtraHealth();
    public int extraDamage => GetExtraHealth();
    public int extraManaCost => GetExtraHealth();

    #region Attacking Cards

    public void Attack(BaseCard attacker, BaseCard target) {
        AttackPassive ap = GetComponent<AttackPassive>();

        if(ap == null)
            DefaultAttack(attacker, target);
        else
            ap.Attack(attacker, target);
    }

    private void DefaultAttack(BaseCard attacker, BaseCard target) { 
        bool b = CardFunctions.instance.CheckForBlockers(target, attacker);

        if (!b) {
            GameControllerUI.instance.SetMessage(attacker.cardStats.cardName + " attacked " + target.cardStats.cardName);
            target.TakeDamage(attacker.effectiveDamage);
        }

        attacker.hasAttacked = true;
        attacker.cardOwner.UsedMana(attacker.cardStats.manaCost);

        AudioManager.instance.Play(SoundNames.attack);
    }

    #endregion

    #region Attack Validation

    public bool ValidateAttack(BaseCard attacker, BaseCard target, Player sender) {
        //AttackPassive ap = GetComponent<AttackPassive>();

        //if (ap == null)
        //    return DefaultAttackValidate(attacker, target, sender);
        //else
        //    return ap.ValidateAttack(attacker, target);

        return DefaultAttackValidate(attacker, target, sender);
    }

    private bool DefaultAttackValidate(BaseCard attacker, BaseCard target, Player sender) {
        if (attacker == null || target == null)
            return false;

        if (attacker.cardOwner.lockInput)
            return false;

        bool b = ValidateAttack(attacker.currentCardPos, target.currentCardPos);

        if (b) {
            //attacker.cardOwner.UsedMana(attacker.cardStats.manaCost); 
        }

        return b;
    }

    private bool ValidateAttack(CardPlacer attacker, CardPlacer target) {
        bool b = attacker.attackPlacers.Contains(target);
        return b && CardFunctions.instance.CanAttack(attacker.currentCard, target.currentCard);
    }

    #endregion

    #region Card Movement

    public void MoveCard(BaseCard card, CardPlacer target, bool canMoveCard) {
        MovePassive ap = GetComponent<MovePassive>();

        if (ap == null)
            DefaultMove(card, target, canMoveCard);
        else
            ap.Move(card, target, canMoveCard);
    }

    private void DefaultMove(BaseCard card, CardPlacer target, bool canMoveCard) {
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
            CardFunctions.instance.CheckForAllCardBuffs();

            // set to true to restrict card from moving twice
            card.hasBeenMoved = true;
        }

        // Place card back to original position if there is a problem
        else {
            card.MoveTo(Vector3.zero);
        }
    }

    #endregion

    #region Movement Validation

    public bool ValidateMovement(BaseCard movingCard, CardPlacer target, Player sender) {
        // Check if sender is owner of card
        if (movingCard.cardOwner.player != sender)
            return false;

        // Check if it is in player hand
        if (movingCard.currentCardPos.pos != Vector2.zero && target.pos != Vector2.zero)
            return BattleFieldMovement(movingCard, target);

        return BringCardToBattleFieldMovement(movingCard, target);
    }

    private bool BattleFieldMovement(BaseCard card, CardPlacer target) {
        MovePassive ap = GetComponent<MovePassive>();

        if (ap == null)
            return DefaultValidateCardMovement(card, target);
        else
            return ap.ValidateMove(card, target);
    }

    private bool DefaultValidateCardMovement(BaseCard card, CardPlacer target) {
        bool canCardMove = true;

        if (card.cardOwner.mana < card.cardStats.moveCost) {
            GameControllerUI.instance.SetMessageError("Not enough Mana");
            canCardMove = false;
        }

        if (card.hasBeenMoved) {
            GameControllerUI.instance.SetMessageError("Card has already been moved");
            canCardMove = false;
        }

        if (CardFunctions.instance.CheckDifference(card.currentCardPos.pos, target.pos) > 1) {
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

    private bool BringCardToBattleFieldMovement(BaseCard card, CardPlacer target) {
        // Cannot move card to enemy zone
        if (card.cardOwner != target.owner) {
            GameControllerUI.instance.SetMessageError("Cannot move card to enemy's area");
            return false;
        }

        // Do this if it is in Player hand
        bool canMoveCard = card.currentCardPos.movePlacers.Contains(target);

        if (target.currentCard != null)
            canMoveCard = false;

        Debug.Log("HEYOOOO" + canMoveCard);

        // Check if it is a mana card placer.
        // User can only sacrifice card to mana placer once per round
        if (target as ManaZoneCardPlacer) {
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

        Debug.Log("HEYOOOO2" + canMoveCard);

        return canMoveCard;
    }

    #endregion

    private int GetExtraHealth() {
        HealthBuff hb = GetComponent<HealthBuff>();

        if (hb != null) 
            return hb.buff;

        return 0;
    }
}