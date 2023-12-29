using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardPosition {
    Left,
    Mid,
    Right,
}

public class BaseCard : MonoBehaviourPun, IDamageable {

    #region Card Stats
    // Card Stats ---------------------------------------------------------------------------------------------------------------

    [SerializeField] CardStats _cardStats;

    public CardStats cardStats {
        get => _cardStats;

        set {
            _cardStats = value;
            defaultHP = _cardStats.maxHP;
        }
    }

    public int defaultHP { get; private set; } = 0;
    public int healthBuff { get; private set; } = 0;
    public int damageBuff { get; private set; } = 0;

    public int passiveRange = 1;

    public int effectiveDamage => damageBuff + cardStats.damage;
    public int effectiveHealth => healthBuff + defaultHP;

    public bool hasBeenMoved = false;
    public bool hasAttacked = false;

    private CardPlacer _currentCardPos;

    public CardPlacer currentCardPos {
        get {
            return _currentCardPos;
        }
        set {
            if (value != null)
                _currentCardPos = value;

            transform.parent = _currentCardPos.transform;
            MoveTo(Vector3.zero);
        }
    }

    public PlayerData cardOwner;

    public static int id = -1;
    public int cardID;

    #endregion

    public delegate void EventFuncs();
    public EventFuncs OnTakeDamage;
    public EventFuncs OnDie;

    [SerializeField] SpriteRenderer border;
    [SerializeField] Animator animator;

    private void Start() {
        defaultHP = cardStats.maxHP;
    }

    private void OnEnable() {
        OnDie += Die;
        OnDie += CardFunctions.instance.CheckForAllCardBuffs;
        //GameManager.instance.OnTurnBegin += HasBeenMovedOverride;
    }

    private void OnDisable() {
        OnDie -= Die;
        OnDie -= CardFunctions.instance.CheckForAllCardBuffs;
        //GameManager.instance.OnTurnBegin -= HasBeenMovedOverride;
    }

    public virtual void ApplyPassive(CardPlacer target) {
    }

    public static BaseCard FindCard(int id) {
        if (id < 0)
            return null;
        
        foreach (var item in FindObjectsOfType<BaseCard>()) {
            if (item.cardID == id) return item;
        }

        Debug.LogError("Card with id: " + id + " does not exist or has been destroyed");
        return null;
    }

    #region Damage and Attack (Health Stuff)

    public virtual bool IsWithinAttackRange() {
        return true;
    }

    public bool CanAttack(BaseCard other) {
        if (cardOwner.mana < cardStats.manaCost) {
            GameManager.instance.SetMessageError("Not enough Mana");
            return false;
        }

        if (hasAttacked) {
            GameManager.instance.SetMessageError("Card has already attacked");
            return false;
        }

        if (!IsWithinAttackRange()) {
            GameManager.instance.SetMessageError("Card not in range");
            return false;
        }

        bool b = effectiveDamage >= other.effectiveHealth;
        if (!b)
            GameManager.instance.SetMessageError("Not enough damage power");

        return b;
    }

    public void TakeDamage(int damage) {
        healthBuff -= damage;

        if (healthBuff < 0) {
            defaultHP += healthBuff;
            healthBuff = 0;
        }

        OnTakeDamage?.Invoke();

        if (defaultHP <= 0)
            OnDie?.Invoke();
    }

    public void Die() {
        animator.SetTrigger("fade");
        AudioManager.instance.Play(SoundNames.burn);

        currentCardPos.OnCardRemoved(this);

        if(GameData.instance.activeCards.Contains(this)) {
            GameData.instance.activeCards.Remove(this);
        }

        Destroy(gameObject, 1);
        Invoke(nameof(CheckAllCardsBuffs), 1.1f);
    }

    public virtual void Attack(BaseCard attackedCard) {
        Debug.Log(name + " trying to attack " + attackedCard.name);

        if (CanAttack(attackedCard)) {

            cardOwner.UsedMana(cardStats.manaCost);

            bool b = attackedCard.CheckForBlockers(this);

            if (!b) {
                GameManager.instance.SetMessage(cardStats.cardName + " attacked " + attackedCard.cardStats.cardName);
                attackedCard.TakeDamage(effectiveDamage);
            }

            hasAttacked = true;

            AudioManager.instance.Play(SoundNames.attack);
        }
    }

    /// <summary>
    /// This code runs on the card that is being attacked.
    /// It searches for blockers that can protect the card
    /// </summary>
    /// <param name="attackerCard"></param>
    /// <returns></returns>
    public bool CheckForBlockers(BaseCard attackerCard) {
        List<BaseCard> cardList;
        if (cardOwner == GameManager.instance.user1)
            cardList = CardValidator.instance.user1Cards;
        else
            cardList = CardValidator.instance.user2Cards;

        foreach (BaseCard blockerCard in cardList) {
            if (blockerCard == this)
                continue;

            if (blockerCard is Tank) {
                if (blockerCard.currentCardPos.attackPlacers.Contains(attackerCard.currentCardPos)) {

                    Debug.Log(blockerCard.cardStats.cardName + blockerCard.cardOwner.username + blockerCard.currentCardPos);

                    animator.SetTrigger("shine");

                    if (attackerCard.CanAttack(blockerCard)) {
                        GameManager.instance.SetMessage(attackerCard.cardStats.cardName + "'s attack to " + cardStats.cardName 
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

    #region Card UI stuff

    public void OnSelected() {
        border.color = Color.white;
        border.enabled = true;

        AudioManager.instance.Play(SoundNames.cardPress);

        if (currentCardPos.pos == Vector2.zero)
            return;

        ShowMovementPowers();
        ShowAttackPowers();
    }

    public void OnSelected(Color32 color) {
        border.enabled = true;
        border.color = color;
    }

    public void OnDeselected() {
        border.enabled = false;

        if (currentCardPos.pos == Vector2.zero)
            return;

        HideMovementPowers();
        HideAttackPowers();
    }

    protected void ShowPowers() {
        if (currentCardPos.pos == Vector2.zero)
            return;

        foreach (CardPlacer cp in currentCardPos.attackPlacers) {
            if (cp.currentCard == null)
                continue;

            cp.currentCard.OnSelected(Color.red);
        }

        List<CardPlacer> cardPlacerList;
        if (cardOwner == GameManager.instance.user1)
            cardPlacerList = GameData.instance.user1CardPlacers;
        else
            cardPlacerList = GameData.instance.user2CardPlacers;


        foreach (CardPlacer cp in cardPlacerList) {
            if (cp == currentCardPos)
                continue;

            if (CheckDifference(currentCardPos.pos, cp.pos) <= 1) {
                if (cp.currentCard == null)
                    cp.OnSelected(Color.white);
            }
        }
    }

    protected virtual void ShowMovementPowers() {
        List<CardPlacer> cardPlacerList = GameData.instance.allCardPlacers;

        foreach (CardPlacer cp in cardPlacerList) {
            if (cp.owner != cardOwner)
                continue;

            if (cp == currentCardPos)
                continue;

            if (CheckDifference(currentCardPos.pos, cp.pos) <= 1) {
                if (cp.currentCard == null)
                    cp.OnSelected(Color.white);
            }
        }
    }

    protected virtual void ShowAttackPowers() {
        foreach (CardPlacer cp in currentCardPos.attackPlacers) {
            if (cp.currentCard == null)
                continue;

            cp.currentCard.OnSelected(Color.red);
        }
    }

    protected virtual void HideMovementPowers() {
        List<CardPlacer> cardPlacerList = GameData.instance.allCardPlacers;

        foreach (CardPlacer cp in cardPlacerList) {
            if (cp.owner != cardOwner)
                continue;

            if (cp == currentCardPos)
                continue;

            if (CheckDifference(currentCardPos.pos, cp.pos) <= 1) {
                cp.OnDeselected();
            }
        }
    }

    protected virtual void HideAttackPowers() {
        foreach (CardPlacer cp in currentCardPos.attackPlacers) {
            if (cp.currentCard == null)
                continue;

            cp.currentCard.border.enabled = false;
        }
    }

    #endregion

    #region Movement Stuff

    protected int CheckDifference(Vector2 v1, Vector2 v2) {
        Vector2 v = v2 - v1;
        return (int)v.magnitude;
    }

    public void MoveCard(CardPlacer target, bool canMoveCard, bool moved = true) {
        // Place card in the new position if everything is good
        if (canMoveCard) {

            // Remove card from card placer
            currentCardPos.OnCardRemoved(this);

            // set new card position. This also starts the animation of the card moving towards the card pos
            currentCardPos = target;

            // set card placer value for current card
            target.currentCard = this;

            // Call Function that places the card on the thing
            target.OnCardPlaced(this);

            // Recheck for all buffs on all cards
            CardValidator.instance.CheckForAllCardBuffs();

            // set to true to restrict card from moving twice
            hasBeenMoved = moved;
        }

        // Place card back to original position if there is a problem
        else {
            MoveTo(Vector3.zero);
        }
    }

    public void HasBeenMovedOverride() {
        if (currentCardPos is DeckCardPlacer) {
            hasBeenMoved = true;
            return;
        }

        hasBeenMoved = false;
        hasAttacked = false;
    }

    bool IsWithinRange(CardPlacer target) {
        return CheckDifference(currentCardPos.pos, target.pos) <= passiveRange;
    }

    public void MoveTo(Vector3 targetPos) {
        float time = 0.4f;

        transform.DOLocalMove(targetPos, time).SetEase(Ease.OutSine);
        //StartCoroutine(DoAnimation(targetPos));
    }

    IEnumerator DoAnimation(Vector3 targetPos) {
        while ((transform.localPosition - targetPos).magnitude > 0.1f) {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, 0.15f);
            yield return null;
        }

        transform.localPosition = targetPos;
    }

    #region Buffs, Debuffs

    public void ApplyHealthBuff(int buff) {
        if (buff > healthBuff)
            healthBuff = buff;
    }

    public void ApplyDamageBuff(int buff) {
        if (buff > damageBuff)
            damageBuff = buff;
    }

    void CheckAllCardsBuffs() {
        foreach (BaseCard card in GameData.instance.activeCards) {
            card.CheckForBuffs();
        }
    }

    public void CheckForBuffs() {
        // Check if buffs should be applied
        if (currentCardPos.pos == Vector2.zero)
            return;

        foreach (BaseCard card in GameData.instance.activeCards) {
            card.ApplyPassive(currentCardPos);
        }
    }

    public void RemoveAllBuffs() {
        damageBuff = 0;
        healthBuff = 0;
    }

    #endregion

    #endregion
}